using BD.SteamClient8.WinAuth.Models.Abstractions;
using System.Net;

namespace BD.SteamClient8.WinAuth.Services.Abstractions;

/// <summary>
/// 令牌相关网络服务
/// </summary>
public interface IAuthenticatorNetService
{
    static IAuthenticatorNetService Instance => Ioc.Get<IAuthenticatorNetService>();

    /// <summary>
    /// 发送 POST 请求，获取指定大小的响应内容字节数组
    /// </summary>
    Task<(HttpStatusCode statusCode, byte[]? responseData)> PostByteArrayAsync(string requestUri, byte[] bytes, int size, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 GET 请求，获取指定大小的响应内容字节数组
    /// </summary>
    Task<(HttpStatusCode statusCode, byte[]? responseData)> GetByteArrayAsync(string requestUri, int size, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务器时间
    /// </summary>
    Task<(HttpStatusCode statusCode, DateTimeOffset? date)> GetDateTimeAsync(string[] timeSyncUrls, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 Steam 服务器时间，单位为秒
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> GetSteamServerTimeAsync(CancellationToken cancellationToken = default);

    ISteamConvertSteamDataJsonStruct? Deserialize(string? json);
}
