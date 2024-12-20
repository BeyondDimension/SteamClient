namespace BD.SteamClient8.WinAuth;

/// <summary>
/// Initializes a new instance of the <see cref="GoogleAuthenticatorNetServiceImpl"/> class.
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
sealed class GoogleAuthenticatorNetServiceImpl(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG), serviceProvider), GoogleAuthenticator.IAuthenticatorNetService
{
    const string TAG = "GoogleAuthenticatorNetService";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

    /// <summary>
    /// 用于同步时间的 URL
    /// </summary>
    const string TIME_SYNC_URL = "http://www.google.com";

    /// <inheritdoc/>
    public async Task<ApiRspImpl<(HttpStatusCode statusCode, string? date)>> TimeSync(CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, TIME_SYNC_URL);
        using var client = new HttpClient();
        client.Timeout = new TimeSpan(0, 0, 5);
        using var rsp = await client.SendAsync(requestMessage, cancellationToken);
        var date = rsp.Headers.GetValues("Date").First();
        return (rsp.StatusCode, date);
    }
}
