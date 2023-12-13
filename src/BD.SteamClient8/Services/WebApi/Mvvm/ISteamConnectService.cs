namespace BD.SteamClient8.Services.WebApi.Mvvm;

#pragma warning disable SA1600 // Elements should be documented

public interface ISteamConnectService
{
    /// <summary>
    /// 连接 SteamClient 是否成功
    /// </summary>
    bool IsConnectToSteam { get; set; }

    /// <summary>
    /// Steam 语言
    /// </summary>
    string? SteamLanguageString { get; }

    /// <summary>
    /// 本机的 Steam 游戏
    /// </summary>
    SourceCache<SteamApp, uint> SteamApps { get; }

    /// <summary>
    /// 下载的 Steam 游戏
    /// </summary>
    SourceCache<SteamApp, uint> DownloadApps { get; }

    /// <summary>
    /// 用户拥有的 Steam 游戏
    /// </summary>
    SourceCache<SteamUser, long> SteamUsers { get; }

    /// <summary>
    /// 当前 Steam 客户端连接的用户
    /// </summary>
    SteamUser? CurrentSteamUser { get; }
}
