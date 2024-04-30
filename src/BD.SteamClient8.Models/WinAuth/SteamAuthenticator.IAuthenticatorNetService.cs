namespace BD.SteamClient8.Models.WinAuth;

partial class SteamAuthenticator
{
    /// <summary>
    /// Steam 令牌相关网络服务
    /// </summary>
    public interface IAuthenticatorNetService
    {
        static IAuthenticatorNetService Instance => Ioc.Get<IAuthenticatorNetService>();

        /// <summary>
        /// 服务器时间
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ApiRspImpl<SteamSyncStruct?>> TwoFAQueryTime(CancellationToken cancellationToken = default);
    }
}
