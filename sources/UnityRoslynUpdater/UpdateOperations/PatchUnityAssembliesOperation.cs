using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Signatures;
using System.Text.Json;

namespace UnityRoslynUpdater;

internal sealed class PatchUnityAssembliesOperation : IUpdateOperation
{
    public Task ExecuteAsync(UpdateContext context)
    {
        Console.WriteLine("Processing Unity libraries...");
        var editedModules = new HashSet<ModuleDefinition>();
        var embeddedTypes = AssemblyDefinition.FromStream(typeof(Program).Assembly.GetManifestResourceStream("EmbeddedTypes.dll")!);
        var modulePatches = JsonSerializer.Deserialize(typeof(Program).Assembly.GetManifestResourceStream("Patches.json")!, JsonSourceGenerationContext.Default.UnityPatchCollectionArray);

        foreach (string dllPath in Directory.EnumerateFiles(context.EditorDataPath, "UnityEngine.dll", SearchOption.AllDirectories))
        {
            var unityAssembly = AssemblyDefinition.FromFile(dllPath);

            if (unityAssembly.ManifestModule is not ModuleDefinition module)
                continue;

            foreach (var definition in modulePatches ?? [])
            {
                var type = module.ResolveTypeByPath(definition.Path);

                if (type is null || type.DeclaringModule is null || type.DeclaringModule.FilePath is null)
                    continue;
                
                if (embeddedTypes.ManifestModule is not null && !editedModules.Contains(type.DeclaringModule))
                    ImportEmbeddedTypes(embeddedTypes.ManifestModule, type.DeclaringModule);

                foreach (var patch in definition.Patches ?? [])
                {
                    if (!patch.Execute(type.DeclaringModule, type))
                    {
                        Console.WriteLine($"{patch.GetType().Name} failed");
                    }
                }

                editedModules.Add(type.DeclaringModule);
            }
        }

        foreach (var module in editedModules)
        {
            if (module.FilePath is null || module.Assembly is null)
                continue;

            Console.WriteLine($"Patching {Path.GetRelativePath(context.EditorDataPath, module.FilePath)}...");
            var backup = File.ReadAllBytes(module.FilePath);

            try
            {
                module.Assembly.Write(module.FilePath);
            }
            catch
            {
                File.WriteAllBytes(module.FilePath, backup);
                throw;
            }
        }

        return Task.CompletedTask;
    }

    private static void ImportEmbeddedTypes(ModuleDefinition embeddedTypes, ModuleDefinition module)
    {
        var cloner = new MemberCloner(module, context => new EmbeddedTypesReferenceImporter(context));
        cloner.AddListener(new InjectTypeClonerListener(module));
        cloner.AddListener(new AssignTokensClonerListener(module));

        foreach (var attribute in embeddedTypes.FindCustomAttributes(null, "ExportEmbeddedTypeAttribute"))
        {
            if (attribute?.Signature?.FixedArguments is not [{ Element: TypeDefOrRefSignature signature }])
                continue;

            if (signature.Resolve(embeddedTypes) is not TypeDefinition type)
                continue;

            if (module.TopLevelTypes.Any(t => t.IsTypeOf(signature.Namespace, signature.Name)))
                continue;

            cloner.Include(type);
        }

        var clone = cloner.Clone();
    }

    private sealed class EmbeddedTypesReferenceImporter(MemberCloneContext context) : CloneContextAwareReferenceImporter(context)
    {
        protected override ITypeDefOrRef ImportType(TypeDefinition type)
        {
            return Context.Module.GetTopLevelType(type.Namespace, type.Name) ?? base.ImportType(type);
        }
    }
}
