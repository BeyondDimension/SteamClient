namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// SteamGridDB WebApi 服务
/// </summary>
public interface ISteamGridDBWebApiServiceImpl
{
    /// <summary>
    /// SteamGridDB 接口密钥
    /// </summary>
    const string SteamGridDBApiKey = "ae93db7411cac53190aa5a9b633bf5e2";

    /// <summary>
    /// 获取当前服务实例
    /// </summary>
    static ISteamGridDBWebApiServiceImpl Instance => Ioc.Get<ISteamGridDBWebApiServiceImpl>();

    /// <summary>
    /// 通过 appId 获取 SteamGridApp 信息
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamGridApp?>> GetSteamGridAppBySteamAppId(long appId);

    /// <summary>
    /// 通过 gameId 获取 SteamGrid 详情列表
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<ApiRspImpl<List<SteamGridItem>?>> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid);

    /// <summary>
    /// SteamGridDB 接口密钥
    /// </summary>
    static string? ApiKey { get; set; } = SteamGridDBApiKey;
}