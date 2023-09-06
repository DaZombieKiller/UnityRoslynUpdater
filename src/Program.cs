using System.Diagnostics;
using UnityRoslynPatcher;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

if (args.Length < 1)
{
    Console.Error.WriteLine(
        """
        Please provide the path to the 'Editor' directory of the Unity
        installation that you wish to link to a newer .NET SDK version.
        """);

    return;
}

var editorPath = Path.GetFullPath(args[0]);
var dataPath = Path.Combine(editorPath, "Data");

if (!Directory.Exists(dataPath))
{
    Console.Error.WriteLine(
        """
        The 'Data' directory could not be located. Please provide the
        path to the 'Editor' directory of the Unity installation that
        you wish to link to a newer .NET SDK version.
        """);

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
var corePath = Path.Combine(dataPath, "Managed", "UnityEngine", "UnityEditor.CoreModule.dll");
var assembly = AssemblyDefinition.FromFile(corePath);

// We're going to find a call to set_LanguageVersion in the default constructor, and then
// change the value it passes in to whatever the latest C# language version is at this time.
var target = assembly.Modules[0].GetTypeByFullName("UnityEditor.Compilation.ScriptCompilerOptions");
var version = sdk.ComputeLatestCSharpLangVersion();

if (target is null || !TryPatchLangVersion(target, version))
{
    Console.Error.WriteLine(
        """
        This version of Unity does not appear to be compatible with
        the changes applied by this patch. Aborting process...
        """
        );

    return;
}

bool TryPatchLangVersion(TypeDefinition type, string version)
{
    var method = type.GetDefaultConstructor();
    var setter = type.GetMethodByName("set_LanguageVersion");

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
            return true;
        }
    }

    return false;
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

assembly.Write(corePath);
Console.WriteLine($"Updated language version to {version}.");

Directory.CreateSymbolicLink(Path.Combine(dataPath, "NetCoreRuntime"), sdk.Location);
Directory.CreateSymbolicLink(Path.Combine(dataPath, "DotNetSdkRoslyn"), sdk.RoslynLocation);
Console.WriteLine($"Linked to .NET SDK at {sdk.Location}");
