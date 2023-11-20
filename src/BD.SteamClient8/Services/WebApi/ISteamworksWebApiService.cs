namespace BD.SteamClient8.Services.WebApi;

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
    /// <returns></returns>
    Task<ApiRspImpl<string>> GetAllSteamAppsString();

    /// <summary>
    /// 获取所有 Steam 游戏列表
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<List<SteamApp>>> GetAllSteamAppList();

    /// <summary>
    /// 获取 Steam 个人资料
    /// </summary>
    /// <param name="steamId64"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamUser>> GetUserInfo(long steamId64);

    /// <summary>
    /// 获取 Mini 资料
    /// </summary>
    /// <param name="steamId3"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamMiniProfile?>> GetUserMiniProfile(long steamId3);
}