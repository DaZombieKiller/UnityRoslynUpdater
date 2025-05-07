using System.Text.Json;

namespace UnityRoslynUpdater;

public static class EditorFinder
{
    /// <summary>
    /// First try parse Unity Hub secondaryInstallPath.json
    /// If unspecified, then fall back to default path
    /// </summary>
    public static string GetEditorRoot()
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
                Console.WriteLine($"Error parsing {userPathFile}. Falling back to default path.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {userPathFile}: {ex.Message}. Falling back to default path.");
            }
        }

        // Use default as fallback
        // this is the case most of the time I think
        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Unity", "Hub", "Editor");

        if (Directory.Exists(defaultPath))
        {
            return defaultPath;
        }

        // No path found
        return string.Empty;
    }

    /// <summary>
    /// Gets all editor paths under Unity Hub installation root
    /// </summary>
    public static List<string> GetEditorFullPaths()
    {
        var editorPaths = new List<string>();

        var rootPath = GetEditorRoot();
        if (string.IsNullOrEmpty(rootPath)) return editorPaths; // return empty if root not found

        try
        {
            foreach (var dir in Directory.GetDirectories(rootPath))
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
            Console.WriteLine($"Error enumerating Unity installations: {ex.Message}");
        }

        return editorPaths;
    }

    /// <summary>
    /// Let user choose from available installations by typing list index
    /// </summary>
    public static string ChooseEditorFullPath()
    {
        var editorPaths = GetEditorFullPaths();

        if (editorPaths.Count == 0)
        {
            return string.Empty;
        }

        Console.WriteLine("Select an editor to patch:");
        // Console.WriteLine("Select an editor to patch. Ctrl+C to cancel:");
        for (var i = 0; i < editorPaths.Count; i++)
        {
            // string versionInfo = Path.GetFileName(Path.GetDirectoryName(editorPaths[i]) ?? string.Empty);
            // Console.WriteLine($"  [{i}] {versionInfo}");
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