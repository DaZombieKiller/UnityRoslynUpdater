using System.Runtime.InteropServices;

namespace UnityRoslynUpdater;

public enum Platform
{
    Windows,
    OSX
}

public static class PlatformHelper
{
    /// <summary>
    /// Gets the current OS platform. Currently only supports Windows or OSX
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static Platform GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Platform.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Platform.OSX;
        
        throw new PlatformNotSupportedException();
    }
}