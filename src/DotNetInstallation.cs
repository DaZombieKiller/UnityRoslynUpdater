using System.Runtime.Versioning;
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

        if (string.IsNullOrEmpty(location) && OperatingSystem.IsWindows())
            location = GetWindowsRegistryDotNetInstallLocation();

        if (string.IsNullOrEmpty(location))
            location = GetDefaultInstallationLocation();

        return new DotNetInstallation(location);
    }

    private static string GetDefaultInstallationLocation()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");

        if (OperatingSystem.IsMacOS())
            return "/usr/local/share/dotnet";
        
        throw new PlatformNotSupportedException();
    }

    [SupportedOSPlatform("windows")]
    private static string? GetWindowsRegistryDotNetInstallLocation()
    {
        return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\dotnet\Setup\InstalledVersions\x64", "InstallLocation", null) as string;
    }
}
