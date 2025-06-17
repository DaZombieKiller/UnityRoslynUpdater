using System.Diagnostics;
using System.IO.Compression;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using UnityRoslynUpdater;

const string errorMsg =
    """
    Please provide the path to the 'Editor' directory of the Unity
    installation that you wish to link to a newer .NET SDK version.
    """;

string editorPath;

if (args.Length >= 1)
{
    editorPath = Path.GetFullPath(args[0]);
}
else
{
    editorPath = EditorFinder.ChooseEditorFullPath();

    if (string.IsNullOrEmpty(editorPath))
    {
        Console.Error.WriteLine(errorMsg);
        return;
    }
}

var dataPath = EditorFinder.GetDataPath(editorPath);

if (!Directory.Exists(dataPath))
{
    Console.Error.WriteLine(errorMsg);
    return;
}

// We want to use the latest SDK available on the machine.
var sdk = DotNetInstallation.Current.EnumerateSDKs().OrderBy(static sdk => sdk.Version).Last();

//
// In order to take advantage of the updated .NET SDK, we need to change the LangVersion
// value that Unity uses when it generates project files and compiles scripts.
//
// We achieve this by patching the parameterless constructor of the ScriptCompilerOptions
// type, which propagates the LangVersion change to all necessary locations.
//
// This approach is preferred because it does not require modifying anything else, all of
// the IDE support packages should automatically pick up this change without further edits.
//

if (!TryPatchCompilerOptions())
{
    Console.Error.WriteLine(
        """
        This version of Unity does not appear to be compatible with
        the changes applied by this patch. Aborting process...
        """
        );

    return;
}

bool TryPatchCompilerOptions()
{
    var dllPath = Path.Combine(dataPath, "Managed", "UnityEngine", "UnityEditor.CoreModule.dll");

    if (!File.Exists(dllPath))
        return false;

    // We're going to find a call to set_LanguageVersion in the default constructor, and then
    // change the value it passes in to whatever the latest C# language version is at this time.
    var assembly = AssemblyDefinition.FromFile(dllPath);
    var target   = assembly.Modules[0].GetTypeByFullName("UnityEditor.Compilation.ScriptCompilerOptions");
    var version  = sdk.ComputeLatestCSharpLangVersion();

    if (target is null)
        return false;

    var method = target.GetDefaultConstructor();
    var setter = target.GetMethodByName("set_LanguageVersion");

    if (method is null || setter is null)
        return false;

    if (method.CilMethodBody is not { Instructions: var instructions })
        return false;

    for (int i = 1; i < instructions.Count; i++)
    {
        if (instructions[i].OpCode == CilOpCodes.Call && instructions[i].Operand == setter)
        {
            Debug.Assert(instructions[i - 1].OpCode == CilOpCodes.Ldstr);
            instructions[i - 1].Operand = version;

            // Instruction has been patched, now save our changes and move on.
            assembly.Write(dllPath);
            Console.WriteLine($"Updated language version to {version}.");
            return true;
        }
    }

    return false;
}

//
// Starting with Unity 2022, Unity.SourceGenerators.dll is used by Unity to process
// .cs files and discover MonoBehaviour implementations in them. However, currently
// it uses the NamespaceDeclarationSyntax class, not BaseNamespaceDeclarationSyntax 
// which causes it to fail on FileScopedNamespaceDeclarationSyntax.
//
// We patch the TypeNameHelper.GetTypeInformation function to replace all references
// to NamespaceDeclarationSyntax with BaseNamespaceDeclarationSyntax, which fixes it.
//

TryPatchSourceGenerator(Path.Combine(dataPath, "Tools", "Unity.SourceGenerators", "Unity.SourceGenerators.dll"));
TryPatchSourceGenerator(Path.Combine(dataPath, "Tools", "Compilation", "Unity.SourceGenerators", "Unity.SourceGenerators.dll"));

bool TryPatchSourceGenerator(string dllPath)
{
    const string NamespaceDeclarationSyntax = "Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax";

    if (!File.Exists(dllPath))
        return false;

    var assembly = AssemblyDefinition.FromFile(dllPath);
    var module   = assembly.Modules[0];
    var target   = module.GetTypeByFullName("Unity.MonoScriptGenerator.TypeNameHelper");
    var method   = target?.GetMethodByName("GetTypeInformation");
    var patches  = 0;
    
    if (target is null || method is null)
        return false;

    if (method.CilMethodBody is not { Instructions: var instructions } body)
        return false;

    var baseNamespaceDeclarationSyntax = module.DefaultImporter.ImportType(
        new TypeReference(
            module.GetAssemblyReferenceByName("Microsoft.CodeAnalysis.CSharp"),
            "Microsoft.CodeAnalysis.CSharp.Syntax",
            "BaseNamespaceDeclarationSyntax"));

    foreach (var local in body.LocalVariables)
    {
        if (local.VariableType is TypeDefOrRefSignature { FullName: NamespaceDeclarationSyntax } type)
        {
            local.VariableType = new TypeDefOrRefSignature(baseNamespaceDeclarationSyntax);
            patches++;
        }
    }

    for (int i = 0; i < instructions.Count; i++)
    {
        if (instructions[i].Operand is TypeReference { FullName: NamespaceDeclarationSyntax } type)
        {
            instructions[i].Operand = baseNamespaceDeclarationSyntax;
            patches++;
        }
        else if (instructions[i].Operand is MemberReference
            {
                IsMethod: true,
                Parent: TypeReference { FullName: NamespaceDeclarationSyntax } parent
            } member)
        {
            var reference = new MemberReference(baseNamespaceDeclarationSyntax, member.Name, member.Signature as MemberSignature);
            instructions[i].Operand = module.DefaultImporter.ImportMethod(reference);
            patches++;
        }
    }

    if (patches > 0)
    {
        assembly.Write(dllPath);
        Console.WriteLine($"Patched source generator assembly at {Path.GetRelativePath(editorPath, dllPath)}.");
    }

    return true;
}

//
// We need to redirect two important directories:
// * Editor/Data/NetCoreRuntime
// * Editor/Data/DotNetSdkRoslyn
//
// To avoid conflicts, we move the original directories
// into a new "BuiltInDotNetSdk" directory.
//
Directory.CreateDirectory(Path.Combine(dataPath, "BuiltInDotNetSdk"));
ProcessBuiltInSdkDirectory(Path.Combine(dataPath, "DotNetSdkRoslyn"));
ProcessBuiltInSdkDirectory(Path.Combine(dataPath, "NetCoreRuntime"));

void ProcessBuiltInSdkDirectory(string path)
{
    if (Directory.ResolveLinkTarget(path, returnFinalTarget: true) is null)
    {
        // The directory is not a symbolic link, so we probably haven't
        // patched this Unity installation yet. We'll move the directory
        // into the BuiltInDotNetSdk directory so the symbolic link can
        // be created.
        var destination = Path.Combine(dataPath, "BuiltInDotNetSdk", Path.GetFileName(path));
        Directory.Move(path, destination);
        return;
    }

    // The directory IS a symbolic link, meaning we have most likely
    // patched this installation previously. We'll delete the link so
    // it can be updated.
    Directory.Delete(path, recursive: false);
}

Directory.CreateSymbolicLink(Path.Combine(dataPath, "NetCoreRuntime"), DotNetInstallation.Current.Location);
Directory.CreateSymbolicLink(Path.Combine(dataPath, "DotNetSdkRoslyn"), sdk.RoslynLocation);
Console.WriteLine($"Linked to .NET SDK at {sdk.Location}");

// Add netstandard.xml if not already present, to provide IntelliSense support for the BCL.
var netStandardLink = @"https://www.nuget.org/api/v2/package/NETStandard.Library.Ref/2.1.0";
var netStandardPath = Path.Combine(dataPath, "NetStandard", "Ref", "2.1.0", "netstandard.xml");

if (!File.Exists(netStandardPath) && File.Exists(Path.ChangeExtension(netStandardPath, ".dll")))
{
    Console.WriteLine("Downloading NETStandard.Library.Ref 2.1.0...");
    using var stream = new MemoryStream();
    using var client = new HttpClient();
    using var result = await client.GetAsync(netStandardLink, HttpCompletionOption.ResponseHeadersRead);
    result.EnsureSuccessStatusCode();

    await using (var content = await result.Content.ReadAsStreamAsync())
        await content.CopyToAsync(stream);

    using var zip = new ZipArchive(stream);
    using var xml = zip.GetEntry("ref/netstandard2.1/netstandard.xml")!.Open();

    // Save netstandard.xml
    using var destination = File.Create(netStandardPath);
    await xml.CopyToAsync(destination);
    Console.WriteLine($"Added missing netstandard.xml at {netStandardPath}");
}
