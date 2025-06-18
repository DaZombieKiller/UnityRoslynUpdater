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
        if (OperatingSystem.IsWindows())
        {
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
        }
        else if (OperatingSystem.IsMacOS())
        {
            // There's no central place where macOS Unity installs advertise themselves,
            // so we just check the default installation path in Unity Hub.
            
            var appFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var editors = Path.Combine(appFolder, "Unity", "Hub", "Editor");
            if (Directory.Exists(editors))
            {
                foreach (var versionPath in Directory.EnumerateDirectories(editors))
                {
                    var packagePath = Path.Combine(versionPath, "Unity.app");
                    if (Directory.Exists(packagePath))
                    {
                        editorPaths.Add(packagePath);
                    }
                }
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

    public static string GetDataPath(string editorPath)
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(editorPath, "Data");
    
        if (OperatingSystem.IsMacOS())
            return Path.Combine(editorPath, "Contents");
    
        throw new PlatformNotSupportedException();
    }
}