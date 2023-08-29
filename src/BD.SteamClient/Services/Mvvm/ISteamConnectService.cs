namespace BD.SteamClient.Services.Mvvm;

public interface ISteamConnectService
{
    /// <summary>
    /// 连接 SteamClient 是否成功
    /// </summary>
    bool IsConnectToSteam { get; set; }

    string? SteamLanguageString { get; }

    SourceCache<SteamApp, uint> SteamApps { get; }

    SourceCache<SteamApp, uint> DownloadApps { get; }

    SourceCache<SteamUser, long> SteamUsers { get; }

    SteamUser? CurrentSteamUser { get; }
}
