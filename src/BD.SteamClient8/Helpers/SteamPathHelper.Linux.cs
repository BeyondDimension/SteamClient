#if LINUX
namespace BD.SteamClient8.Helpers;

partial class SteamPathHelper
{
    public static partial string? GetSteamProgramPath()
    {
        const string exePath = "/usr/bin/steam";
        return exePath;
    }

    public static partial string? GetUnixRegistryVdfFilePath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var registryVdfFilePath = $"{userProfile}/.steam/registry.vdf";
        return registryVdfFilePath;
    }
}
#endif