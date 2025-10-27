using System.IO.Compression;

namespace UnityRoslynUpdater;

internal sealed class DownloadBclDocumentationOperation : IUpdateOperation
{
    public async Task ExecuteAsync(UpdateContext context)
    {
        // Add netstandard.xml if not already present, to provide IntelliSense support for the BCL.
        var netStandardLink = @"https://www.nuget.org/api/v2/package/NETStandard.Library.Ref/2.1.0";
        var netStandardPath = Path.Combine(context.EditorDataPath, "NetStandard", "Ref", "2.1.0", "netstandard.xml");

        if (!File.Exists(netStandardPath) && File.Exists(Path.ChangeExtension(netStandardPath, ".dll")))
        {
            Console.WriteLine("Downloading NETStandard.Library.Ref 2.1.0...");
            using var stream = new MemoryStream();
            using var client = new HttpClient();
            using var result = await client.GetAsync(netStandardLink, HttpCompletionOption.ResponseHeadersRead);
            result.EnsureSuccessStatusCode();

            await using (var content = await result.Content.ReadAsStreamAsync())
                await content.CopyToAsync(stream);

            using var zip = new ZipArchive(stream);
            using var xml = zip.GetEntry("ref/netstandard2.1/netstandard.xml")!.Open();

            // Save netstandard.xml
            using var destination = File.Create(netStandardPath);
            await xml.CopyToAsync(destination);
            Console.WriteLine($"Added missing netstandard.xml at {netStandardPath}");
        }
    }
}
