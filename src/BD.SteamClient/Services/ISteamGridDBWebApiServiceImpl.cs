namespace BD.SteamClient.Services;

/// <summary>
/// SteamGridDB WebApi 服务
/// </summary>
public interface ISteamGridDBWebApiServiceImpl
{
    const string SteamGridDBApiKey = "ae93db7411cac53190aa5a9b633bf5e2";

    static ISteamGridDBWebApiServiceImpl Instance => Ioc.Get<ISteamGridDBWebApiServiceImpl>();

    Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId);

    Task<List<SteamGridItem>?> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid);

    static string? ApiKey { get; set; } = SteamGridDBApiKey;
}