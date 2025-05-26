using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Models.WinAuth;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Net;

namespace BD.SteamClient8.WinAuth;

/// <summary>
/// Initializes a new instance of the <see cref="GoogleAuthenticatorNetServiceImpl"/> class.
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
sealed class GoogleAuthenticatorNetServiceImpl(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG), serviceProvider), TimeSync6AuthenticatorValueModel.IAuthenticatorNetService
{
    const string TAG = "GoogleAuthenticatorNetService";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

    async Task<(HttpStatusCode statusCode, DateTimeOffset? date)> GetDateTimeAsync(
        [StringSyntax("Uri")] string? requestUri,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        using var rsp = await client.UseDefaultSendAsync(req,
            HttpCompletionOption.ResponseHeadersRead, // 这里不需要响应内容，只要响应头
            cancellationToken);
        var date = rsp.Headers.Date;
        var statusCode = rsp.StatusCode;
        return (statusCode, date);
    }

    /// <inheritdoc/>
    public async Task<(HttpStatusCode statusCode, DateTimeOffset? date)> GetDateTimeAsync(string[] timeSyncUrls, CancellationToken cancellationToken = default)
    {
        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/GoogleAuthenticator.cs#L121-L134

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var firstTcs = new TaskCompletionSource<(HttpStatusCode statusCode, DateTimeOffset? date)>();

        var parallelTask = Parallel.ForEachAsync(timeSyncUrls, async (it, _) =>
        {
            // 并行化 Send 请求，最先成功的返回，后面的取消
            try
            {
                var result = await GetDateTimeAsync(it, cts.Token);
                firstTcs.TrySetResult(result);
                cts.Cancel(); // 取消其他任务
            }
            catch
            {
            }
        });

        await Task.WhenAny(parallelTask, firstTcs.Task);

        if (firstTcs.Task.IsCompletedSuccessfully)
        {
            return firstTcs.Task.Result;
        }

        return (HttpStatusCode.RequestTimeout, null);
    }
}
