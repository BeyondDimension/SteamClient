#if !(IOS || ANDROID)
namespace BD.SteamClient8.Helpers;

partial class SteamPathHelper // UserVdfPath | AppInfoPath | ConfigVdfPath | LibrarycacheDirPath
{
    /// <summary>
    /// <list type="bullet">
    ///   <item>
    ///     Windows：~\Steam\config\loginusers.vdf
    ///   </item>
    ///   <item>
    ///     Linux：~/.steam/steam/config/loginusers.vdf
    ///   </item>
    ///   <item>
    ///     Mac：~/Library/Application Support/Steam/config/loginusers.vdf
    ///   </item>
    /// </list>
    /// </summary>
    /// <param name="verify"></param>
    /// <returns></returns>
    public static string? GetUserVdfPath(bool verify = true)
    {
        var steamDirPath = GetSteamDirPath();
        if (steamDirPath != null)
        {
            var userVdfPath = Path.Combine(steamDirPath, "config", "loginusers.vdf");
            if (!verify || File.Exists(userVdfPath))
            {
                return userVdfPath;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取应用程序信息文件的路径
    /// </summary>
    /// <param name="verify"></param>
    /// <returns></returns>
    public static string? GetAppInfoPath(bool verify = true)
    {
        var steamDirPath = GetSteamDirPath();
        if (steamDirPath != null)
        {
            var appinfoVdfPath = Path.Combine(steamDirPath, "appcache", "appinfo.vdf");
            if (!verify || File.Exists(appinfoVdfPath))
            {
                return appinfoVdfPath;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取配置 Vdf 文件路径
    /// </summary>
    /// <param name="verify"></param>
    /// <returns></returns>
    public static string? GetConfigVdfPath(bool verify = true)
    {
        var steamDirPath = GetSteamDirPath();
        if (steamDirPath != null)
        {
            var configVdfPath = Path.Combine(steamDirPath, "config", "config.vdf");
            if (!verify || File.Exists(configVdfPath))
            {
                return configVdfPath;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取程序库缓存目录的路径
    /// </summary>
    /// <param name="verify"></param>
    /// <returns></returns>
    public static string? GetLibrarycacheDirPath(bool verify = true)
    {
        var steamDirPath = GetSteamDirPath();
        if (steamDirPath != null)
        {
            var librarycacheDirPath = Path.Combine(steamDirPath, "appcache", "librarycache");
            if (!verify || Directory.Exists(librarycacheDirPath))
            {
                return librarycacheDirPath;
            }
        }
        return null;
    }
}
#endif