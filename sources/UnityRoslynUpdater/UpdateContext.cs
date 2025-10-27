namespace UnityRoslynUpdater;

internal sealed class UpdateContext
{
    /// <summary>The path to the Unity editor installation.</summary>
    public required string EditorPath { get; init; }

    /// <summary>The path to the Unity editor data directory.</summary>
    public string EditorDataPath => Path.Combine(EditorPath, "Data");
}
