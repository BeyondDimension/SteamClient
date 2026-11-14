using System.Text.Json.Serialization.Metadata;

namespace BD.SteamClient.Services.Implementation;

internal sealed class SteamworksWebApiServiceImpl : GeneralHttpClientFactory, ISteamworksWebApiService
{
    const string TAG = "SteamworksWebApiS";

    protected override string? DefaultClientName => TAG;

    public SteamworksWebApiServiceImpl(
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
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd(MediaTypeNames.JSON);
            if (http_helper.UserAgent is { } userAgent)
                request.Headers.UserAgent.ParseAdd(userAgent);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return default;
            if (typeof(T) == typeof(string))
                return (T)(object)await response.Content.ReadAsStringAsync(cancellationToken);
            return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "SendAsync fail, requestUri: {requestUri}", requestUri);
            return default;
        }
    }

    public async Task<string> GetAllSteamAppsString()
    {
        var rsp = await GetAsync(STEAMAPP_LIST_URL, SteamWebJsonContext.Default.String);
        return rsp ?? string.Empty;
    }

    public async Task<List<SteamApp>> GetAllSteamAppList()
    {
        var rsp = await GetAsync(STEAMAPP_LIST_URL, SteamWebJsonContext.Default.JsonObject);
        if (rsp?["applist"]?["apps"] is not JsonArray apps)
            return new List<SteamApp>();
        var result = new List<SteamApp>(apps.Count);
        foreach (var app in apps)
        {
            if (app is JsonObject item)
                result.Add(new SteamApp { AppId = item["appid"]?.GetValue<uint>() ?? 0, Name = item["name"]?.GetValue<string>() });
        }
        return result;
    }

    public async Task<SteamUser> GetUserInfo(long steamId64)
    {
        //因为某些原因放弃从社区页链接获取详细资料
        //var requestUri = string.Format(SteamApiUrls.STEAM_USERINFO_XML_URL, steamId64);
        //var rsp = await s.GetAsync<SteamUser>(requestUri);

        var data = new SteamUser() { SteamId64 = steamId64 };
        var rsp = await GetUserMiniProfile(data.SteamId32);
        if (rsp != null)
        {
            data.MiniProfile = rsp;
            data.SteamID = WebUtility.HtmlDecode(rsp.PersonaName);
            data.AvatarFull = rsp.AvatarUrl;
            data.AvatarMedium = rsp.AvatarUrl;
        }
        return data;
    }

    public async Task<SteamMiniProfile?> GetUserMiniProfile(long steamId3)
    {
        var requestUri = string.Format(STEAM_MINIPROFILE_URL, steamId3);
        var rsp = await GetAsync(requestUri, SteamWebJsonContext.Default.SteamMiniProfile);
        return rsp;
    }
}