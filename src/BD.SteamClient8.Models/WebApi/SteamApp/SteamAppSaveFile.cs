namespace BD.SteamClient8.Models.WebApi.SteamApp;

#pragma warning disable SA1600 // Elements should be documented

public class SteamAppSaveFile
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// <see cref="SteamAppSaveFile"/> 构造
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="root"></param>
    /// <param name="path"></param>
    /// <param name="pattern"></param>
    public SteamAppSaveFile(uint appid, string? root, string? path, string? pattern)
    {
        ParentAppId = appid;
        Root = root;
        Path = path;
        Pattern = pattern;
    }

    /// <summary>
    /// 父级 AppId
    /// </summary>
    public uint ParentAppId { get; set; }

    /// <summary>
    /// 根路径
    /// </summary>
    public string? Root { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    public string? Path { get; set; }

    public string? Pattern { get; set; }

    /// <summary>
    /// 拼接后的文件夹路径
    /// </summary>
    public string? FormatDirPath { get; set; }

    /// <summary>
    /// 拼接后的文件路径
    /// </summary>
    public string? FormatFilePath { get; set; }

    public bool Recursive { get; set; }

    /// <summary>
    /// 系统平台
    /// </summary>
    public string? Platforms { get; set; }
#endif
}
