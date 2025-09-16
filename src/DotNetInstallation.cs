using Microsoft.Win32;
using NuGet.Versioning;

namespace UnityRoslynUpdater;

public sealed class DotNetInstallation
{
    public string Location { get; }

    public static DotNetInstallation Current { get; } = GetCurrentInstallation();

    public DotNetInstallation(string location)
    {
        ArgumentNullException.ThrowIfNull(location);
        Location = location;
    }

    public IEnumerable<DotNetRuntime> EnumerateRuntimes()
    {
        foreach (string directory in Directory.EnumerateDirectories(Path.Combine(Location, "shared", "Microsoft.NETCore.App")))
        {
            var directoryName = Path.GetFileName(directory);

            if (!SemanticVersion.TryParse(directoryName, out SemanticVersion? version))
                continue;

            yield return new DotNetRuntime(Path.GetFullPath(directory), this, version);
        }
    }

    public IEnumerable<DotNetSdk> EnumerateSDKs()
    {
        foreach (string directory in Directory.EnumerateDirectories(Path.Combine(Location, "sdk")))
        {
            var directoryName = Path.GetFileName(directory);

            if (!SemanticVersion.TryParse(directoryName, out SemanticVersion? version))
                continue;

            yield return new DotNetSdk(Path.GetFullPath(directory), this, version);
        }
    }

    private static DotNetInstallation GetCurrentInstallation()
    {
        string? location = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        if (string.IsNullOrEmpty(location))
            location = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\dotnet\Setup\InstalledVersions\x64", "InstallLocation", null) as string;

        if (string.IsNullOrEmpty(location))
            location = GetDefaultInstallationLocation();

        return new DotNetInstallation(location);
    }

    private static string GetDefaultInstallationLocation()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
    }
}
