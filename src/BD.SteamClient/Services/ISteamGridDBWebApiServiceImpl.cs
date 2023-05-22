namespace BD.SteamClient.Services;

/// <summary>
/// SteamGridDB WebApi 服务
/// </summary>
public interface ISteamGridDBWebApiServiceImpl
{
    static ISteamGridDBWebApiServiceImpl Instance => Ioc.Get<ISteamGridDBWebApiServiceImpl>();

    Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId);

    Task<List<SteamGridItem>?> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid);

    static string? ApiKey { get; set; }
}