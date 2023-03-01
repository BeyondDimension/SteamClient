using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace BD.SteamClient.Services.Implementation;

internal sealed class SteamGridDBWebApiServiceImpl : GeneralHttpClientFactory, ISteamGridDBWebApiServiceImpl
{
    const string TAG = "SteamGridDBWebApiS";

    protected override string? DefaultClientName => TAG;

    readonly JsonSerializer jsonSerializer = new();

    public SteamGridDBWebApiServiceImpl(
        IHttpClientFactory clientFactory,
        ILoggerFactory loggerFactory,
        IHttpPlatformHelperService http_helper) : base(
            loggerFactory.CreateLogger(TAG),
            http_helper, clientFactory)
    {

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task<T?> GetAsync<T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        var client = CreateClient();
        return SendAsync<T>(client, logger, jsonSerializer, requestUri,
            () =>
            {
                var apiKeySteamGridDB = ISteamGridDBWebApiServiceImpl.ApiKey;
                apiKeySteamGridDB.ThrowIsNull();
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKeySteamGridDB);
                request.Headers.Accept.ParseAdd(accept);
                var userAgent = http_helper.UserAgent;
                if (userAgent != null)
                    request.Headers.UserAgent.ParseAdd(userAgent);
                return request;
            }
            , accept, cancellationToken: cancellationToken);
    }

    public async Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId)
    {
        var url = string.Format(RetrieveGameBySteamAppId_Url, appId);
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

    public async Task<List<SteamGridItem>?> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid)
    {
        var url = type switch
        {
            SteamGridItemType.Hero => string.Format(RetrieveHeros_Url, gameId),
            SteamGridItemType.Logo => string.Format(RetrieveLogos_Url, gameId),
            SteamGridItemType.Icon => string.Format(RetrieveIcons_Url, gameId),
            SteamGridItemType.Header => string.Format(RetrieveGrids_Url, gameId),
            _ => string.Format(RetrieveGrids_Url, gameId),
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