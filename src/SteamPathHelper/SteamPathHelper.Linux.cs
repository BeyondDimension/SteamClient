#if LINUX || (PROJ_3RD_PARTY_SAM_API && !WINDOWS && !MACOS)
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
#if PROJ_3RD_PARTY_SAM_API && !LINUX
    static string? GetSteamDirPathByLINUX()
#else
    public static partial string? GetSteamDirPath()
#endif
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var steamDirPath = $"{userProfile}/.steam/steam";
        return steamDirPath;
    }

#if PROJ_3RD_PARTY_SAM_API && !LINUX
    static string? GetSteamClientNativeLibraryPathByLINUX()
#else
    public static partial string? GetSteamClientNativeLibraryPath()
#endif
    {
        string? steamDirPath = GetSteamDirPath();
        if (steamDirPath == null)
        {
            return null;
        }

        // /home/{0}/.local/share/Steam/linux64/steamclient.so
        var libraryPath = Path.Combine(steamDirPath,
            Environment.Is64BitProcess ?
                "linux64" :
                "linux32",
            "steamclient.so");
        return libraryPath;
    }
}
#endif