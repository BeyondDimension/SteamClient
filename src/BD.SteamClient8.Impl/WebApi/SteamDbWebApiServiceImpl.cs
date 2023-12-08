namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600
internal sealed class SteamDbWebApiServiceImpl : WebApiClientFactoryService, ISteamDbWebApiService
{
    const string TAG = "SteamDbWebApiS";

    /// <summary>
    /// HttpClient 标识名称
    /// </summary>
    protected override string? ClientName => TAG;

    protected override SystemTextJsonSerializerContext? JsonSerializerContext => DefaultJsonSerializerContext_.Default;

    private readonly IHttpPlatformHelperService http_helper;

    public SteamDbWebApiServiceImpl(
        IClientHttpClientFactory clientFactory,
        IHttpPlatformHelperService http_helper,
        ILoggerFactory loggerFactory) : base(
            loggerFactory.CreateLogger(TAG),
            http_helper,
            clientFactory)
    {
        this.http_helper = http_helper;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<T?> GetAsync<T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        try
        {
            var client = CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd(accept);
            request.Headers.TryAddWithoutValidation("User-Agent", http_helper.UserAgent);
            var response = await client.SendAsync(request, cancellationToken);
            return await ReadFromSJsonAsync<T>(response.Content, null);
        }
        catch
        {
            return default;
        }
    }

    public async Task<ApiRspImpl<SteamUser>> GetUserInfo(long steamId64)
    {
        var requestUri = string.Format(SteamApiUrls.STEAMDB_USERINFO_URL, steamId64);
        var user = await GetAsync<SteamUser>(requestUri) ?? new SteamUser() { SteamId64 = steamId64 };
        return user!;
    }

    public async Task<ApiRspImpl<List<SteamUser>>> GetUserInfo(IEnumerable<long> steamId64s)
    {
        List<SteamUser> users = [];
        foreach (var i in steamId64s)
            users.Add((await GetUserInfo(i)).Content!);
        return users!;
    }

    public async Task<ApiRspImpl<SteamApp>> GetAppInfo(int appId)
    {
        var requestUri = string.Format(SteamApiUrls.STEAMDB_APPINFO_URL, appId);
        var app = await GetAsync<SteamApp>(requestUri) ?? new SteamApp() { AppId = (uint)appId };
        return app!;
    }
}