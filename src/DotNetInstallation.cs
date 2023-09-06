using Microsoft.Win32;
using NuGet.Versioning;

namespace UnityRoslynPatcher;

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
        if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\dotnet\Setup\InstalledVersions\x64", "InstallLocation", null) is not string location)
            location = GetDefaultInstallationLocation();

        return new DotNetInstallation(location);
    }

    private static string GetDefaultInstallationLocation()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
    }
}
