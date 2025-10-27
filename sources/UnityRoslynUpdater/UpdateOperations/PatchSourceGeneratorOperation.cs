using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace UnityRoslynUpdater;

internal sealed class PatchSourceGeneratorOperation : IUpdateOperation
{
    public Task ExecuteAsync(UpdateContext context)
    {
        foreach (string filePath in Directory.EnumerateFiles(Path.Combine(context.EditorDataPath, "Tools"), "Unity.SourceGenerators.dll", SearchOption.AllDirectories))
        {
            //
            // Starting with Unity 2022, Unity.SourceGenerators.dll is used by Unity to process
            // .cs files and discover MonoBehaviour implementations in them. However, currently
            // it uses the NamespaceDeclarationSyntax class, not BaseNamespaceDeclarationSyntax 
            // which causes it to fail on FileScopedNamespaceDeclarationSyntax.
            //
            // We patch the TypeNameHelper.GetTypeInformation function to replace all references
            // to NamespaceDeclarationSyntax with BaseNamespaceDeclarationSyntax, which fixes it.
            //
            PatchSourceGenerator(context, filePath);
        }

        return Task.CompletedTask;
    }

    private static bool PatchSourceGenerator(UpdateContext context, string dllPath)
    {
        const string NamespaceDeclarationSyntax = "Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax";
        var assembly = AssemblyDefinition.FromFile(dllPath);

        if (assembly.ManifestModule is not { } module)
            return false;

        var target = module.GetTopLevelType("Unity.MonoScriptGenerator", "TypeNameHelper");
        var method = target?.Methods.FirstOrDefault(m => m.Name == "GetTypeInformation");
        var patches = 0;

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
            else if (instructions[i].Operand is MemberReference { IsMethod: true, Parent: TypeReference { FullName: NamespaceDeclarationSyntax } } member)
            {
                var reference = new MemberReference(baseNamespaceDeclarationSyntax, member.Name, member.Signature as MemberSignature);
                instructions[i].Operand = module.DefaultImporter.ImportMethod(reference);
                patches++;
            }
        }

        if (patches > 0)
        {
            var backup = File.ReadAllBytes(dllPath);

            try
            {
                assembly.Write(dllPath);
            }
            catch
            {
                File.WriteAllBytes(dllPath, backup);
                throw;
            }

            Console.WriteLine($"Patched source generator assembly at {Path.GetRelativePath(context.EditorDataPath, dllPath)}.");
        }

        return true;
    }
}
