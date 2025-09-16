using System.Reflection;
using NuGet.Versioning;

namespace UnityRoslynUpdater;

public sealed class DotNetSdk
{
    public string Location { get; }

    public SemanticVersion Version { get; }

    public DotNetInstallation Installation { get; }

    public string RoslynLocation => Path.Combine(Location, "Roslyn", "bincore");

    public DotNetSdk(string location, DotNetInstallation installation, SemanticVersion version)
    {
        ArgumentNullException.ThrowIfNull(location);
        Location = location;
        Installation = installation;
        Version = version;
    }

    public string ComputeLatestCSharpLangVersion()
    {
        var runtimes = Installation.EnumerateRuntimes().OrderByDescending(x => x.Version);
        var runtime = runtimes.FirstOrDefault(r => Version.Equals(r.Version)) ?? runtimes.FirstOrDefault() ?? DotNetRuntime.Current;
        using var context = new MetadataLoadContext(new PathAssemblyResolver(Directory.EnumerateFiles(runtime.Location, "*.dll")));
        var assembly = context.LoadFromAssemblyPath(Path.Combine(RoslynLocation, "Microsoft.CodeAnalysis.CSharp.dll"));
        var versions = new List<int>();

        foreach (var field in assembly.GetType("Microsoft.CodeAnalysis.CSharp.LanguageVersion")!.GetFields())
        {
            if (field.Name == "value__")
                continue;

            if (field.GetRawConstantValue() is not int value)
                continue;

            if (value >= int.MaxValue - 2)
                continue;

            versions.Add(value);
        }

        versions.Sort();
        int version = versions[^1];
        (int major, int minor) = version > 7 ? int.DivRem(version, 100) : (version, 0);
        return $"{major}.{minor}";
    }
}
