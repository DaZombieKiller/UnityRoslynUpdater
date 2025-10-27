using AsmResolver.DotNet;
using System.Text.Json.Serialization;

namespace UnityRoslynUpdater;

[JsonPolymorphic]
[JsonDerivedType(typeof(AddAttributeOnParametersPatch), AddAttributeOnParametersPatch.JsonDiscriminator)]
[JsonDerivedType(typeof(AddAttributeOnMethodPatch), AddAttributeOnMethodPatch.JsonDiscriminator)]
internal abstract class UnityPatch
{
    public abstract bool Execute(ModuleDefinition module, TypeDefinition type);

    protected static MethodDefinition? FindMethod(TypeDefinition type, string name, string signature)
    {
        return type.Methods.FirstOrDefault(method => method.Name == name && method.Signature?.ToString() == signature);
    }

    protected static ITypeDefOrRef? FindAttribute(ModuleDefinition module, string? ns, string name)
    {
        return (ITypeDefOrRef?)module.CreateTypeReference(ns, name).Resolve() ?? module.CorLibTypeFactory.CorLibScope.CreateTypeReference(ns, name);
    }
}
