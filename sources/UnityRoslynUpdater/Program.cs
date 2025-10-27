using UnityRoslynUpdater;

string editorPath = args.Length >= 1 ? Path.GetFullPath(args[0]) : EditorFinder.ChooseEditorFullPath();

if (string.IsNullOrEmpty(editorPath) || !Directory.Exists(Path.Combine(editorPath, "Data")))
{
    Console.Error.WriteLine(
        """
        Please provide the path to the 'Editor' directory of the Unity
        installation that you wish to link to a newer .NET SDK version.
        """
    );

    return;
}

var context = new UpdateContext
{
    EditorPath = editorPath
};

IUpdateOperation[] operations =
[
    new UpdateSdkOperation(),
    new PatchSourceGeneratorOperation(),
    new PatchUnityAssembliesOperation(),
    new DownloadBclDocumentationOperation(),
];

foreach (var operation in operations)
{
    await operation.ExecuteAsync(context);
}
