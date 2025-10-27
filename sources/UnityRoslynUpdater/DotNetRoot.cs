using Microsoft.Win32;
using NuGet.Versioning;

namespace UnityRoslynUpdater;

internal sealed class DotNetRoot
{
    public static IEnumerable<(string Location, SemanticVersion Version)> EnumerateSDKs(string root)
    {
        foreach (string directory in Directory.EnumerateDirectories(Path.Combine(root, "sdk")))
        {
            var directoryName = Path.GetFileName(directory);

            if (!SemanticVersion.TryParse(directoryName, out SemanticVersion? version))
                continue;

            yield return (Path.GetFullPath(directory), version);
        }
    }

    public static string GetLocation()
    {
        string? location = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        if (string.IsNullOrEmpty(location))
            location = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\dotnet\Setup\InstalledVersions\x64", "InstallLocation", null)?.ToString();

        if (string.IsNullOrEmpty(location))
            location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");

        return location;
    }
}
