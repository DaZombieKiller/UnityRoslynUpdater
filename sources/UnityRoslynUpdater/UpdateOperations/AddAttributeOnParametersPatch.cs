using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Text.Json.Serialization;

namespace UnityRoslynUpdater;

internal sealed class AddAttributeOnParametersPatch : UnityPatch
{
    public const string JsonDiscriminator = "AddAttributeOnParameters";

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [JsonPropertyName("attribute")]
    public string[]? Attribute { get; set; }

    [JsonPropertyName("parameters")]
    public string[]? Parameters { get; set; }

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

        foreach (var name in Parameters ?? [])
        {
            var parameter = method.ParameterDefinitions.FirstOrDefault(p => p.Name == name);

            if (parameter is null)
                continue;

            if (parameter.HasCustomAttribute(attribute.Namespace, attribute.Name))
                continue;

            parameter.CustomAttributes.Add(new CustomAttribute(constructor));
        }

        return true;
    }
}
