#if MACCATALYST || MACOS
namespace BD.SteamClient8.Helpers;

partial class SteamPathHelper
{
    public static partial string? GetSteamProgramPath()
    {
        const string exePath = "/Applications/Steam.app/Contents/MacOS/steam_osx";
        return exePath;
    }

    public static partial string? GetUnixRegistryVdfFilePath()
    {
        var userName = Environment.UserName;
        var registryVdfFilePath = $"/Users/{userName}/Library/Application Support/Steam/registry.vdf";
        return registryVdfFilePath;
    }
}
#endif