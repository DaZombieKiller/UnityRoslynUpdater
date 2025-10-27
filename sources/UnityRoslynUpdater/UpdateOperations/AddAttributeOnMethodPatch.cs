using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Text.Json.Serialization;

namespace UnityRoslynUpdater;

internal sealed class AddAttributeOnMethodPatch : UnityPatch
{
    public const string JsonDiscriminator = "AddAttributeOnMethod";

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [JsonPropertyName("attribute")]
    public string[]? Attribute { get; set; }

    public override bool Execute(ModuleDefinition module, TypeDefinition type)
    {
        if (Method is null || Signature is null || Attribute is null)
            return false;

        var method = FindMethod(type, Method, Signature);
        var attribute = FindAttribute(module, Attribute[0], Attribute[1]);
        var constructor = attribute?.CreateMemberReference(".ctor", MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));

        if (method is null)
        {
            Console.WriteLine($"Failed to find {Method} {Signature}.");
            return false;
        }

        if (attribute is null || constructor is null)
        {
            Console.WriteLine($"Failed to find {string.Join('.', Attribute.AsSpan())}.");
            return false;
        }

        if (method.HasCustomAttribute(attribute.Namespace, attribute.Name))
            return true;

        method.CustomAttributes.Add(new CustomAttribute(constructor));
        return true;
    }
}
