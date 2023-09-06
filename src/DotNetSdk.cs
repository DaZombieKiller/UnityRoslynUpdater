using System.Reflection;
using NuGet.Versioning;

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
        // Load the bundled Roslyn installation.
        var assembly = Assembly.LoadFrom(Path.Combine(RoslynLocation, "Microsoft.CodeAnalysis.CSharp.dll"));

        // We need the LanguageVersion and LanguageVersionFacts types.
        var versions = assembly.GetType("Microsoft.CodeAnalysis.CSharp.LanguageVersion")!;
        var facts = assembly.GetType("Microsoft.CodeAnalysis.CSharp.LanguageVersionFacts")!;

        // Retrieve the value of LanguageVersion.Latest, which we will resolve to a LangVersion string.
        var version = Enum.Parse(versions, "Latest");

        // Convert from "Latest" to a specific version.
        version = facts.GetMethod("MapSpecifiedToEffectiveVersion")!.Invoke(null, new[] { version });

        // Map the version to a LangVersion string.
        return (string)facts.GetMethod("ToDisplayString")!.Invoke(null, new[] { version })!;
    }
}
