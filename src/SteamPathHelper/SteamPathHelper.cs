#if !(IOS || ANDROID)
namespace
#if PROJ_3RD_PARTY_SAM_API
    SAM.API;
#else
    BD.SteamClient8.Helpers;
#endif
#if PROJ_3RD_PARTY_SAM_API
partial class Helpers
#else
partial class SteamPathHelper // SteamDirPath | SteamClientNativeLibraryPath
#endif
{
    public static partial string? GetSteamDirPath();

    public static partial string? GetSteamClientNativeLibraryPath();
}

#if PROJ_3RD_PARTY_SAM_API
partial class Helpers
#else
partial class SteamPathHelper
#endif
{
#if !WINDOWS && !(MACCATALYST || MACOS) && !LINUX
    public static partial string? GetSteamDirPath()
#if PROJ_3RD_PARTY_SAM_API
    {
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
        if (global::System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(global::System.Runtime.InteropServices.OSPlatform.Windows))
#else
#endif
#else
        if (global::System.OperatingSystem.IsWindows())
#endif
        {
            return GetSteamDirPathByWin();
        }
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
        else if (global::System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(global::System.Runtime.InteropServices.OSPlatform.OSX))
#else
#endif
#else
        else if (global::System.OperatingSystem.IsMacOS())
#endif
        {
            return GetSteamDirPathByMAC();
        }
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
        else if (global::System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(global::System.Runtime.InteropServices.OSPlatform.Linux))
#else
#endif
#else
        else if (global::System.OperatingSystem.IsLinux())
#endif
        {
            return GetSteamDirPathByLINUX();
        }
        throw new PlatformNotSupportedException("Unsupported platform for SteamPathHelper.");
    }
#else
        => throw new PlatformNotSupportedException("Unsupported platform for SteamPathHelper.");
#endif
#endif

#if !WINDOWS && !(MACCATALYST || MACOS) && !LINUX
    public static partial string? GetSteamClientNativeLibraryPath()
#if PROJ_3RD_PARTY_SAM_API
    {
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
        if (global::System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(global::System.Runtime.InteropServices.OSPlatform.Windows))
#else
#endif
#else
        if (global::System.OperatingSystem.IsWindows())
#endif
        {
            return GetSteamClientNativeLibraryPathByWin();
        }
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
        else if (global::System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(global::System.Runtime.InteropServices.OSPlatform.OSX))
#else
#endif
#else
        else if (global::System.OperatingSystem.IsMacOS())
#endif
        {
            return GetSteamClientNativeLibraryPathByMAC();
        }
#if NETFRAMEWORK || NETSTANDARD
#if NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
        else if (global::System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(global::System.Runtime.InteropServices.OSPlatform.Linux))
#else
#endif
#else
        else if (global::System.OperatingSystem.IsLinux())
#endif
        {
            return GetSteamClientNativeLibraryPathByLINUX();
        }
        throw new PlatformNotSupportedException("Unsupported platform for SteamPathHelper.");
    }
#else
        => throw new PlatformNotSupportedException("Unsupported platform for SteamPathHelper.");
#endif
#endif
}
#endif