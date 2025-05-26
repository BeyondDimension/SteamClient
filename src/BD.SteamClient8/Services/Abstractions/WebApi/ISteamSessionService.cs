using BD.Common8.Models;
using BD.SteamClient8.Models.WebApi.Logins;

namespace BD.SteamClient8.Services.Abstractions.WebApi;

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
    /// 当前本地的 Steam 登录会话信息键
    /// </summary>
    public const string CurrentSteamUserKey = "CurrentSteamUser";

    /// <summary>
    /// 添加或修改登录会话信息
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> AddOrSetSession(SteamSession steamSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 STEAM ID 获取会话信息，不存在返回 null
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamSession?>> RentSession(string steam_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过文件加载登录会话信息
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamSession?>> LoadSession(CancellationToken cancellationToken = default);

    /// <summary>
    /// 将登录会话信息本地存储
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> SaveSession(SteamSession steamSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除登录会话信息
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> RemoveSession(string steam_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解除家庭监控
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="pinCode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl> UnlockParental(string steam_id, string pinCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 恢复家庭监控
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl> LockParental(string steam_id, CancellationToken cancellationToken = default);
}
