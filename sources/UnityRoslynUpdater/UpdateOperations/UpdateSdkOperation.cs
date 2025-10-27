﻿namespace UnityRoslynUpdater;

internal sealed class UpdateSdkOperation : IUpdateOperation
{
    public Task ExecuteAsync(UpdateContext context)
    {
        // We want to use the latest SDK available on the machine.
        var sdk = DotNetRoot.EnumerateSDKs(DotNetRoot.GetLocation()).OrderBy(static sdk => sdk.Version).Last();

        //
        // We need to redirect two important directories:
        // * Editor/Data/NetCoreRuntime
        // * Editor/Data/DotNetSdkRoslyn
        //
        // To avoid conflicts, we move the original directories
        // into a new "BuiltInDotNetSdk" directory.
        //
        Directory.CreateDirectory(Path.Combine(context.EditorDataPath, "BuiltInDotNetSdk"));
        ProcessBuiltInSdkDirectory(Path.Combine(context.EditorDataPath, "DotNetSdkRoslyn"));
        ProcessBuiltInSdkDirectory(Path.Combine(context.EditorDataPath, "NetCoreRuntime"));

        void ProcessBuiltInSdkDirectory(string path)
        {
            if (Directory.ResolveLinkTarget(path, returnFinalTarget: true) is null)
            {
                // The directory is not a symbolic link, so we probably haven't
                // patched this Unity installation yet. We'll move the directory
                // into the BuiltInDotNetSdk directory so the symbolic link can
                // be created.
                var destination = Path.Combine(context.EditorDataPath, "BuiltInDotNetSdk", Path.GetFileName(path));
                Directory.Move(path, destination);
                return;
            }

            // The directory IS a symbolic link, meaning we have most likely
            // patched this installation previously. We'll delete the link so
            // it can be updated.
            Directory.Delete(path, recursive: false);
        }

        Directory.CreateSymbolicLink(Path.Combine(context.EditorDataPath, "NetCoreRuntime"), DotNetRoot.GetLocation());
        Directory.CreateSymbolicLink(Path.Combine(context.EditorDataPath, "DotNetSdkRoslyn"), Path.Combine(sdk.Location, "Roslyn", "bincore"));
        Console.WriteLine($"Linked to .NET SDK at {sdk.Location}");
        return Task.CompletedTask;
    }
}
