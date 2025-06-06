#if MACCATALYST || MACOS || (PROJ_3RD_PARTY_SAM_API && !WINDOWS && !LINUX)
namespace
#if PROJ_3RD_PARTY_SAM_API
    SAM.API;
#else
    BD.SteamClient8.Helpers;
#endif

#if PROJ_3RD_PARTY_SAM_API
partial class Helpers
#else
partial class SteamPathHelper
#endif
{
#if PROJ_3RD_PARTY_SAM_API && !MACOS
    static string? GetSteamDirPathByMAC()
#else
    public static partial string? GetSteamDirPath()
#endif
    {
        var userName = Environment.UserName;
        var steamDirPath = $"/Users/{userName}/Library/Application Support/Steam";
        return steamDirPath;
    }


#if PROJ_3RD_PARTY_SAM_API && !MACOS
    static string? GetSteamClientNativeLibraryPathByMAC()
#else
    public static partial string? GetSteamClientNativeLibraryPath()
#endif
    {
        var userName = Environment.UserName;
        var libraryPath = $"/Users/{userName}/Library/Application Support/Steam/Steam.AppBundle/Steam/Contents/MacOS/steamclient.dylib";
        return libraryPath;
    }
}
#endif