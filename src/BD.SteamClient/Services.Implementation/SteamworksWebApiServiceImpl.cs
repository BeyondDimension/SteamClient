using System;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task<T?> GetAsync<T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        var client = CreateClient(TAG, HttpHandlerCategory.Default);
        return client.GetAsync<T>(logger, requestUri, accept,
            cancellationToken: cancellationToken, userAgent: http_helper.UserAgent, jsonSerializer: Newtonsoft.Json.JsonSerializer.CreateDefault());
    }

    public async Task<string> GetAllSteamAppsString()
    {
        var rsp = await GetAsync<string>(STEAMAPP_LIST_URL);
        return rsp ?? string.Empty;
    }

    public async Task<List<SteamApp>> GetAllSteamAppList()
    {
        var rsp = await GetAsync<SteamApps>(STEAMAPP_LIST_URL);
        return rsp?.AppList?.Apps ?? new List<SteamApp>();
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
            data.SteamID = rsp.PersonaName;
            data.AvatarFull = rsp.AvatarUrl;
            data.AvatarMedium = rsp.AvatarUrl;
        }
        return data;
    }

    public async Task<SteamMiniProfile?> GetUserMiniProfile(long steamId3)
    {
        var requestUri = string.Format(STEAM_MINIPROFILE_URL, steamId3);
        var rsp = await GetAsync<SteamMiniProfile>(requestUri);
        return rsp;
    }
}