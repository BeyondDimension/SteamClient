namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600
internal sealed class SteamworksWebApiServiceImpl : WebApiClientFactoryService, ISteamworksWebApiService
{
    const string TAG = "SteamworksWebApiS";

    protected override string? ClientName => TAG;

    public SteamworksWebApiServiceImpl(
        IClientHttpClientFactory clientFactory,
        IHttpPlatformHelperService platformHelperService,
        ILoggerFactory loggerFactory) : base(
            loggerFactory.CreateLogger(TAG),
            platformHelperService,
            clientFactory)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<T?> GetAsync<T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        try
        {
            var client = CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.TryAddWithoutValidation("accept", accept);
            var response = await client.SendAsync(request, cancellationToken);
            return await ReadFromSJsonAsync<T>(response.Content, null);
        }
        catch
        {
            return default;
        }
    }

    public async Task<ApiRspImpl<string>> GetAllSteamAppsString()
    {
        var rsp = await GetAsync<string>(SteamApiUrls.STEAMAPP_LIST_URL);
        return ApiRspHelper.Ok(rsp ?? string.Empty)!;
    }

    public async Task<ApiRspImpl<List<SteamApp>>> GetAllSteamAppList()
    {
        var rsp = await GetAsync<SteamApps>(SteamApiUrls.STEAMAPP_LIST_URL);
        return (rsp?.AppList?.Apps ?? [])!;
    }

    public async Task<ApiRspImpl<SteamUser>> GetUserInfo(long steamId64)
    {
        //因为某些原因放弃从社区页链接获取详细资料
        //var requestUri = string.Format(SteamApiUrls.STEAM_USERINFO_XML_URL, steamId64);
        //var rsp = await s.GetAsync<SteamUser>(requestUri);

        var data = new SteamUser() { SteamId64 = steamId64 };
        var rsp = (await GetUserMiniProfile(data.SteamId32)).Content;
        if (rsp != null)
        {
            data.MiniProfile = rsp;
            data.SteamID = WebUtility.HtmlDecode(rsp.PersonaName);
            data.AvatarFull = rsp.AvatarUrl;
            data.AvatarMedium = rsp.AvatarUrl;
        }
        return data!;
    }

    public async Task<ApiRspImpl<SteamMiniProfile?>> GetUserMiniProfile(long steamId3)
    {
        var requestUri = string.Format(SteamApiUrls.STEAM_MINIPROFILE_URL, steamId3);
        var rsp = await GetAsync<SteamMiniProfile>(requestUri);
        return rsp;
    }
}