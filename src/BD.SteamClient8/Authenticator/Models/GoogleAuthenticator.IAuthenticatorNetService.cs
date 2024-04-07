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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ApiRspImpl<(HttpStatusCode statusCode, string? date)>> TimeSync(CancellationToken cancellationToken = default);
    }
}
