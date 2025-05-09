using Microsoft.Win32;

namespace UnityRoslynUpdater;

public static class EditorFinder
{
    /// <summary>
    /// Including both Unity Hub managed and standalone installations.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetRegistryEditorPaths()
    {
        var editorPaths = new List<string>();
        var regPaths = new[] {
            Registry.CurrentUser,
            Registry.LocalMachine
        };

        foreach (var hive in regPaths)
        {
            using var key = hive.OpenSubKey(@"SOFTWARE\Unity Technologies\Installer");
            if (key == null) continue;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                var installPath = subKey?.GetValue("Location x64") as string;

                if (string.IsNullOrEmpty(installPath)) continue;

                var editorPath = Path.Combine(installPath, "Editor");
                editorPaths.Add(editorPath);
            }
        }

        return editorPaths;
    }

    /// <summary>
    /// Let user interactively choose one installation path by typing list index.
    /// </summary>
    public static string ChooseEditorFullPath()
    {
        var editorPaths = new List<string>();
        editorPaths.AddRange(GetRegistryEditorPaths());

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