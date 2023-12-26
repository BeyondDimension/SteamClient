namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// SteamDb WebApi 服务
/// </summary>
public interface ISteamDbWebApiService
{
    /// <summary>
    /// 获取当前服务实例
    /// </summary>
    static ISteamDbWebApiService Instance => Ioc.Get<ISteamDbWebApiService>();

    /// <summary>
    /// 获取用户详情
    /// </summary>
    /// <param name="steamId64"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamUser>> GetUserInfo(long steamId64, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量获取用户详情
    /// </summary>
    /// <param name="steamId64s"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<List<SteamUser>>> GetUserInfo(IEnumerable<long> steamId64s, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过 AppId 获取游戏详情
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamApp>> GetAppInfo(int appId, CancellationToken cancellationToken = default);
}