using System.Extensions;
using System.Net;
using System.Net.Http.Headers;

namespace BD.SteamClient8.Services.WinAuth;

partial class AuthenticatorNetServiceImpl
{
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

        var rspContent = await rsp.Content.ReadAsStreamAsync(cancellationToken);
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