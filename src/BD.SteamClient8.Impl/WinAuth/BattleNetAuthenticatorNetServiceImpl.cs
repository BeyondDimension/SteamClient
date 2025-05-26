using BD.Common8.Helpers;
using BD.Common8.Http.ClientFactory.Models;
using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Models.WinAuth;
using Microsoft.Extensions.Logging;
using System.Extensions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BD.SteamClient8.WinAuth;

/// <summary>
/// Initializes a new instance of the <see cref="BattleNetAuthenticatorNetServiceImpl"/> class.
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
sealed class BattleNetAuthenticatorNetServiceImpl(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG), serviceProvider), BattleNetAuthenticator.IAuthenticatorNetService
{
    const string TAG = "BattleNetAuthenticatorNetService";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

    async Task<(HttpStatusCode statusCode, byte[]? responseData)> ReadAsByteArrayAsync(HttpRequestMessage req, int size, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var rsp = await client.UseDefaultSendAsync(req,
            HttpCompletionOption.ResponseHeadersRead, // 先读取头看看响应内容长度是否符合预期
            cancellationToken);
        if (!rsp.IsSuccessStatusCode)
        {
            return (rsp.StatusCode, null);
        }

        var contentLength = rsp.Content.Headers.ContentLength; // 响应头长度可能有，也可能没有
        if (contentLength.HasValue && contentLength.Value != size)
        {
            return (rsp.StatusCode, null); // 返回 null 表示请求的内容长度超过了预期的大小
        }

        var rspContent = rsp.Content.ReadAsStream(cancellationToken);
        byte[] responseData = new byte[size];
        int n = rspContent.Read(responseData);
        if (n != size)
        {
            return (rsp.StatusCode, null); // 返回 null 表示读取的内容长度不符合预期
        }
        var last = rspContent.ReadByte(); // 读取最后一个字节，确保流位置正确
        if (last != -1)
        {
            return (rsp.StatusCode, null); // 返回 null 表示流未正确结束
        }
        return (rsp.StatusCode, responseData);
    }

    /// <inheritdoc/>
    public async Task<(HttpStatusCode statusCode, byte[]? responseData)> PostByteArrayAsync(string requestUri, byte[] bytes, int size, CancellationToken cancellationToken = default)
    {
        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs#L363-L400
        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs#L553-L592
        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs#L643-L680

        using HttpRequestMessage req = new(HttpMethod.Post, requestUri);
        req.Content = new ByteArrayContent(bytes);
        req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var result = await ReadAsByteArrayAsync(req, size, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<(HttpStatusCode statusCode, byte[]? responseData)> GetByteArrayAsync(string requestUri, int size, CancellationToken cancellationToken = default)
    {
        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs#L477-L512

        using HttpRequestMessage req = new(HttpMethod.Get, requestUri);

        var result = await ReadAsByteArrayAsync(req, size, cancellationToken);
        return result;
    }
}
