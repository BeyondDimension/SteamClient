#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Services;

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
    /// 通过 AppId 获取 SteamGridApp 信息
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamGridApp?>> GetSteamGridAppBySteamAppId(long appId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过 GameId 获取 SteamGrid 详情列表
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<List<SteamGridItem>?>> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid, CancellationToken cancellationToken = default);

    /// <summary>
    /// SteamGridDB 接口密钥
    /// </summary>
    static string? ApiKey { get; set; } = SteamGridDBApiKey;
}