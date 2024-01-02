#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

partial class GoogleAuthenticator
{
    /// <summary>
    /// 谷歌令牌相关网络服务
    /// </summary>
    public interface IAuthenticatorNetService
    {
        static IAuthenticatorNetService Instance => Ioc.Get<IAuthenticatorNetService>();

        /// <summary>
        /// 同步时间
        /// </summary>
        /// <returns></returns>
        Task<HttpResponseMessage> TimeSync();
    }
}
