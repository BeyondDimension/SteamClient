namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600
internal sealed class SteamGridDBWebApiServiceImpl : WebApiClientFactoryService, ISteamGridDBWebApiServiceImpl
{
    const string TAG = "SteamGridDBWebApiS";

    protected override string? ClientName => TAG;

    protected override SystemTextJsonSerializerContext? JsonSerializerContext => DefaultJsonSerializerContext_.Default;

    private readonly IHttpPlatformHelperService http_helper;

    public SteamGridDBWebApiServiceImpl(
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
            var client = CreateClient(); ;
            var apiKeySteamGridDB = ISteamGridDBWebApiServiceImpl.ApiKey;
            apiKeySteamGridDB.ThrowIsNull();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKeySteamGridDB);
            request.Headers.Accept.ParseAdd(accept);
            var userAgent = http_helper.UserAgent;
            if (userAgent != null)
                request.Headers.UserAgent.ParseAdd(userAgent);

            var rsp = await client.SendAsync(request);
            return await ReadFromSJsonAsync<T>(rsp.Content, null);
        }
        catch (Exception)
        {
            return default;
        }
    }

    public async Task<ApiRspImpl<SteamGridApp?>> GetSteamGridAppBySteamAppId(long appId)
    {
        var url = string.Format(SteamGridDBApiUrls.RetrieveGameBySteamAppId_Url, appId);
        var rsp = await GetAsync<SteamGridAppData>(url, MediaTypeNames.JSON, default);

        if (rsp != null)
        {
            if (rsp.Success == true)
            {
                return rsp.Data;
            }
            else
            {
                Log.Error(nameof(GetSteamGridAppBySteamAppId), string.Join(",", rsp.Errors));
            }
        }
        return null;
    }

    public async Task<ApiRspImpl<List<SteamGridItem>?>> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid)
    {
        var url = type switch
        {
            SteamGridItemType.Hero => string.Format(SteamGridDBApiUrls.RetrieveHeros_Url, gameId),
            SteamGridItemType.Logo => string.Format(SteamGridDBApiUrls.RetrieveLogos_Url, gameId),
            SteamGridItemType.Icon => string.Format(SteamGridDBApiUrls.RetrieveIcons_Url, gameId),
            SteamGridItemType.Header => string.Format(SteamGridDBApiUrls.RetrieveGrids_Url, gameId),
            _ => string.Format(SteamGridDBApiUrls.RetrieveGrids_Url, gameId),
        };

        //url += "?nsfw=any&humor=any";

        url += "?types=static,animated";

        if (type == SteamGridItemType.Header)
        {
            url += "&dimensions=460x215,920x430";
        }
        else if (type == SteamGridItemType.Grid)
        {
            url += "&dimensions=600x900";
        }

        var rsp = await GetAsync<SteamGridItemData>(url, MediaTypeNames.JSON, default);

        if (rsp != null)
        {
            if (rsp.Success == true)
            {
                foreach (var item in rsp.Data)
                {
                    item.GridType = type;
                }
                return rsp.Data;
            }
            else
            {
                logger.LogError("GetSteamGridItemsByGameId fail, errors: {errors}",
                    new LogStrJoin(rsp.Errors));
            }
        }
        return null;
    }

    readonly struct LogStrJoin
    {
        public LogStrJoin(IEnumerable<string> strings)
        {
            Strings = strings;
        }

        IEnumerable<string> Strings { get; }

        public override string ToString() => string.Join(", ", Strings);
    }
}