#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
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
