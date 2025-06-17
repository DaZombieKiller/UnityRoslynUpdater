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

    public IEnumerable<DotNetSdk> EnumerateSDKs()
    {
        foreach (string directory in Directory.EnumerateDirectories(Path.Combine(Location, "sdk")))
        {
            var directoryName = Path.GetFileName(directory);

            if (!SemanticVersion.TryParse(directoryName, out SemanticVersion version))
                continue;

            yield return new DotNetSdk(Path.GetFullPath(directory), version);
        }
    }

    private static DotNetInstallation GetCurrentInstallation()
    {
        string? location = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        if (string.IsNullOrEmpty(location) && PlatformHelper.GetPlatform() is Platform.Windows)
            location = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\dotnet\Setup\InstalledVersions\x64", "InstallLocation", null) as string;

        if (string.IsNullOrEmpty(location))
            location = GetDefaultInstallationLocation();

        return new DotNetInstallation(location);
    }

    private static string GetDefaultInstallationLocation()
    {
        return PlatformHelper.GetPlatform() switch
        {
            Platform.Windows => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet"),
            Platform.OSX => "/usr/local/share/dotnet",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
