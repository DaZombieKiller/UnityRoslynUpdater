using System.Text.Json;

namespace UnityRoslynUpdater;

public static class EditorFinder
{
    /// <summary>
    /// First try parse Unity Hub secondaryInstallPath.json
    /// If unspecified, then fall back to default path
    /// </summary>
    public static string GetHubEditorRoot()
    {
        var userPathFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UnityHub", "secondaryInstallPath.json");

        if (File.Exists(userPathFile))
        {
            try
            {
                var userPath = JsonDocument.Parse(File.ReadAllText(userPathFile)).RootElement.GetString() ?? string.Empty;
                if (!string.IsNullOrEmpty(userPath) && Directory.Exists(userPath))
                {
                    return userPath;
                }
            }
            catch (JsonException)
            {
                Console.Error.WriteLine($"Error parsing {userPathFile}. Falling back to default path.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error reading {userPathFile}: {ex.Message}. Falling back to default path.");
            }
        }

        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Unity", "Hub", "Editor");

        if (Directory.Exists(defaultPath))
        {
            return defaultPath;
        }

        // No unity hub installation path found
        return string.Empty;
    }

    /// <summary>
    /// Gets all editor paths under Unity Hub installation root
    /// </summary>
    public static List<string> GetHubEditorPaths()
    {
        var editorPaths = new List<string>();
        var rootPath = GetHubEditorRoot();
        if (string.IsNullOrEmpty(rootPath)) return editorPaths;

        try
        {
            foreach (var dir in Directory.EnumerateDirectories(rootPath))
            {
                var editorExePath = Path.Combine(dir, "Editor", "Unity.exe");
                if (File.Exists(editorExePath))
                {
                    editorPaths.Add(Path.GetFullPath(Path.Combine(dir, "Editor")));
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error enumerating Unity installations: {ex.Message}");
        }

        return editorPaths;
    }

    /// <summary>
    /// Find standalone Unity installations that are not managed by Unity Hub
    /// Only look for those in Program Files
    /// </summary>
    public static List<string> GetStandaloneEditorPaths()
    {
        var editorPaths = new List<string>();
        var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        try
        {
            foreach (var dir in Directory.EnumerateDirectories(rootPath))
            {
                if (!Path.GetFileName(dir).StartsWith("Unity", StringComparison.OrdinalIgnoreCase)) continue;

                var editorExePath = Path.Combine(dir, "Editor", "Unity.exe");
                if (File.Exists(editorExePath))
                {
                    editorPaths.Add(Path.GetFullPath(Path.Combine(dir, "Editor")));
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error enumerating standalone Unity installations: {ex.Message}");
        }

        return editorPaths;
    }

    /// <summary>
    /// Let user choose from available installations by typing list index
    /// </summary>
    public static string ChooseEditorFullPath()
    {
        var editorPaths = new List<string>();
        editorPaths.AddRange(GetHubEditorPaths());
        editorPaths.AddRange(GetStandaloneEditorPaths());

        if (editorPaths.Count == 0)
        {
            return string.Empty;
        }

        Console.WriteLine("Select an editor to patch:");
        for (var i = 0; i < editorPaths.Count; i++)
        {
            Console.WriteLine($"  [{i}] {editorPaths[i]}");
        }

        while (true)
        {
            Console.Write("Enter index number: ");
            if (int.TryParse(Console.ReadLine(), out var index) && index >= 0 && index < editorPaths.Count)
            {
                return editorPaths[index];
            }

            Console.WriteLine("Invalid selection. Please try again.");
        }
    }
}