using NuGet.Versioning;
using AsmResolver.DotNet;

namespace UnityRoslynUpdater;

public sealed class DotNetSdk
{
    public string Location { get; }

    public SemanticVersion Version { get; }

    public string RoslynLocation => Path.Combine(Location, "Roslyn", "bincore");

    public DotNetSdk(string location, SemanticVersion version)
    {
        ArgumentNullException.ThrowIfNull(location);
        Location = location;
        Version = version;
    }

    public string ComputeLatestCSharpLangVersion()
    {
        var assembly = AssemblyDefinition.FromFile(Path.Combine(RoslynLocation, "Microsoft.CodeAnalysis.CSharp.dll"));
        var versions = new List<int>();

        foreach (var field in assembly.Modules[0].GetTypeByFullName("Microsoft.CodeAnalysis.CSharp.LanguageVersion")!.Fields)
        {
            if (field.Name == "value__")
                continue;

            if (field.Constant?.InterpretData() is not int value)
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
