using BD.Common8.Models;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;

namespace BD.SteamClient8.Services.Abstractions.WebApi;

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
    Task<SteamUser?> GetUserInfo(long steamId64, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量获取用户详情
    /// </summary>
    /// <param name="steamId64s"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<SteamUser>?> GetUserInfos(IEnumerable<long> steamId64s, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过 AppId 获取游戏详情
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SteamApp?> GetAppInfo(int appId, CancellationToken cancellationToken = default);
}