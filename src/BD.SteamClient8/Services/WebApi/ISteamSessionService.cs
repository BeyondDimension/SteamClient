namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// Steam 登录会话信息服务
/// </summary>
public interface ISteamSessionService
{
    /// <summary>
    /// 获取当前服务实例
    /// </summary>
    static ISteamSessionService Instance => Ioc.Get<ISteamSessionService>();

    /// <summary>
    /// 添加或修改登录会话信息
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    bool AddOrSetSession(SteamSession steamSession);

    /// <summary>
    /// 根据 STEAM ID 获取会话信息，不存在返回 null
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    SteamSession? RentSession(string steam_id);

    /// <summary>
    /// 通过文件加载登录会话信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task<SteamSession?> LoadSession(string filePath);

    /// <summary>
    /// 将登录会话信息本地存储
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    Task<bool> SaveSession(SteamSession steamSession);

    /// <summary>
    /// 删除登录会话信息
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    bool RemoveSession(string steam_id);
}
