using NuGet.Versioning;
using System.Runtime.InteropServices;

namespace UnityRoslynUpdater;

public sealed class DotNetRuntime
{
    public static DotNetRuntime Current { get; } = new(RuntimeEnvironment.GetRuntimeDirectory(), null, null);

    public string Location { get; }

    public DotNetInstallation? Installation { get; }

    public SemanticVersion? Version { get; }

    public DotNetRuntime(string location, DotNetInstallation? installation, SemanticVersion? version)
    {
        Location = location;
        Installation = installation;
        Version = version;
    }
}
