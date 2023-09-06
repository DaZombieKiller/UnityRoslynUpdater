using AsmResolver.DotNet;

namespace UnityRoslynUpdater;

public static class ModuleDefinitionExtensions
{
    public static TypeDefinition? GetTypeByFullName(this ModuleDefinition module, string typeName)
    {
        ArgumentNullException.ThrowIfNull(module);

        foreach (TypeDefinition type in module.GetAllTypes())
        {
            if (type.FullName == typeName)
            {
                return type;
            }
        }

        return null;
    }
}
