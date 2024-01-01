#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Services;

/// <summary>
/// Steamworks WebApi 服务
/// </summary>
public interface ISteamworksWebApiService
{
    /// <summary>
    /// 获取当前服务实例
    /// </summary>
    static ISteamworksWebApiService Instance => Ioc.Get<ISteamworksWebApiService>();

    /// <summary>
    /// 获取所有游戏 JSON 字符串
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<string>> GetAllSteamAppsString(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有 Steam 游戏列表
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<List<SteamApp>>> GetAllSteamAppList(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 Steam 个人资料
    /// </summary>
    /// <param name="steamId64"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamUser>> GetUserInfo(long steamId64, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 Mini 资料
    /// </summary>
    /// <param name="steamId3"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamMiniProfile?>> GetUserMiniProfile(long steamId3, CancellationToken cancellationToken = default);
}