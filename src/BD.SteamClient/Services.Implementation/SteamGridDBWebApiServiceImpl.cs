using System.Text.Json.Serialization.Metadata;

namespace BD.SteamClient.Services.Implementation;

internal sealed class SteamGridDBWebApiServiceImpl : GeneralHttpClientFactory, ISteamGridDBWebApiServiceImpl
{
    const string TAG = "SteamGridDBWebApiS";

    protected override string? DefaultClientName => TAG;

    public SteamGridDBWebApiServiceImpl(
        IHttpClientFactory clientFactory,
        ILoggerFactory loggerFactory,
        IHttpPlatformHelperService http_helper) : base(
            loggerFactory.CreateLogger(TAG),
            http_helper, clientFactory)
    {

    }

    async Task<T?> GetAsync<T>(string requestUri, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(TAG, HttpHandlerCategory.Default);
            var apiKeySteamGridDB = ISteamGridDBWebApiServiceImpl.ApiKey;
            apiKeySteamGridDB.ThrowIsNull();
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKeySteamGridDB);
            request.Headers.Accept.ParseAdd(MediaTypeNames.JSON);
            if (http_helper.UserAgent is { } userAgent)
                request.Headers.UserAgent.ParseAdd(userAgent);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return default;
            return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "SendAsync fail, requestUri: {requestUri}", requestUri);
            return default;
        }
    }

    public async Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId)
    {
        var url = string.Format(RetrieveGameBySteamAppId_Url, appId);
        var rsp = await GetAsync(url, SteamWebJsonContext.Default.SteamGridAppData);

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

        var rsp = await GetAsync(url, SteamWebJsonContext.Default.SteamGridItemData);

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