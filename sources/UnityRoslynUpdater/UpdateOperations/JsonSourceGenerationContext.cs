using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnityRoslynUpdater;

[JsonSourceGenerationOptions(ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(UnityPatchCollection[]))]
internal partial class JsonSourceGenerationContext : JsonSerializerContext
{
}
