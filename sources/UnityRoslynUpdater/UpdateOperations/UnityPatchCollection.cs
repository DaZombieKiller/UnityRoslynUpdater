using System.Text.Json.Serialization;

namespace UnityRoslynUpdater;

internal sealed class UnityPatchCollection
{
    [JsonPropertyName("path")]
    public string[]? Path { get; set; }

    [JsonPropertyName("patches")]
    public UnityPatch[]? Patches { get; set; }
}
