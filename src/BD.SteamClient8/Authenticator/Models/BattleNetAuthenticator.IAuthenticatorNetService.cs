#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

partial class BattleNetAuthenticator
{
    /// <summary>
    /// 战网令牌相关网络服务
    /// </summary>
    public interface IAuthenticatorNetService
    {
        static IAuthenticatorNetService Instance => Ioc.Get<IAuthenticatorNetService>();

        /// <summary>
        /// 地理位置 IP
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ApiRspImpl<string>> GEOIP(CancellationToken cancellationToken = default);

        /// <summary>
        /// 令牌添加
        /// </summary>
        /// <param name="region"></param>
        /// <param name="encrypted"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ApiRspImpl<byte[]>> EnRoll(string region, byte[] encrypted, CancellationToken cancellationToken = default);

        /// <summary>
        /// 令牌同步
        /// </summary>
        /// <param name="region"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ApiRspImpl<byte[]>> Sync(string region, CancellationToken cancellationToken = default);

        /// <summary>
        /// 令牌恢复
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="serialBytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ApiRspImpl<byte[]>> ReStore(string serial, byte[] serialBytes, CancellationToken cancellationToken = default);

        /// <summary>
        /// 令牌恢复验证
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="postbytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ApiRspImpl<byte[]>> ReStoreValidate(string serial, byte[] postbytes, CancellationToken cancellationToken = default);
    }
}
