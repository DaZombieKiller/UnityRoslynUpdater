using AsmResolver;
using AsmResolver.DotNet;

namespace UnityRoslynUpdater;

internal static class ModuleDefinitionExtensions
{
    extension(ModuleDefinition module)
    {
        public TypeDefinition? GetTopLevelType(Utf8String? ns, Utf8String? name)
        {
            ArgumentNullException.ThrowIfNull(module);
            return module.TopLevelTypes.FirstOrDefault(type => type.Namespace == ns && type.Name == name);
        }

        public AssemblyReference? GetAssemblyReferenceByName(string name)
        {
            ArgumentNullException.ThrowIfNull(module);
            ArgumentNullException.ThrowIfNull(name);
            return module.AssemblyReferences.FirstOrDefault(reference => reference.Name == name);
        }

        public TypeDefinition? ResolveTypeByPath(params ReadOnlySpan<string?> typePath)
        {
            var typeRef = module.CreateTypeReference(typePath[0], typePath[1]!);

            for (int i = 2; i < typePath.Length; i++)
                typeRef = typeRef.CreateTypeReference(null, typePath[i]!);

            return typeRef.Resolve();
        }
    }
}
