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
        /// <returns></returns>
        Task<HttpResponseMessage> GEOIP();

        /// <summary>
        /// 令牌添加
        /// </summary>
        /// <param name="region"></param>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> EnRoll(string region, byte[] encrypted);

        /// <summary>
        /// 令牌同步
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        HttpResponseMessage Sync(string region);

        /// <summary>
        /// 令牌恢复
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="serialBytes"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> ReStore(string serial, byte[] serialBytes);

        /// <summary>
        /// 令牌恢复验证
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="postbytes"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> ReStoreValidate(string serial, byte[] postbytes);
    }
}
