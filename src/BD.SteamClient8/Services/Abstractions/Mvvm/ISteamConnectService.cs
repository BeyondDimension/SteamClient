#if !(IOS || ANDROID)
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;

namespace BD.SteamClient8.Services.Abstractions.Mvvm;

/// <summary>
/// 带有属性通知的 MVVM 服务接口
/// </summary>
public interface ISteamConnectService
{
    /// <summary>
    /// 连接 SteamClient 是否成功
    /// </summary>
    bool IsConnectToSteam { get; set; }

    /// <summary>
    /// 获取 Steam 语言字符串
    /// </summary>
    string? SteamLanguageString { get; }

    IReadOnlyList<SteamApp> SteamApps { get; }

    //IReadOnlyList<SteamApp> DownloadApps { get; }

    IReadOnlyList<SteamUser> SteamUsers { get; }

    SteamUser? CurrentSteamUser { get; }
}
#endif