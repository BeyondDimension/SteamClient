#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Impl;

/// <summary>
/// <see cref="ISteamworksWebApiService"/> Steamworks WebApi 服务实现
/// </summary>
internal sealed class SteamworksWebApiServiceImpl : WebApiClientFactoryService, ISteamworksWebApiService
{
    const string TAG = "SteamworksWebApiS";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override SystemTextJsonSerializerOptions JsonSerializerOptions =>
    DefaultJsonSerializerContext_.Default.Options;

    /// <summary>
    /// 初始化 <see cref="SteamworksWebApiServiceImpl"/> 类的新实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    public SteamworksWebApiServiceImpl(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : base(
            loggerFactory.CreateLogger(TAG),
            serviceProvider)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        try
        {
            using var sendArgs = new WebApiClientSendArgs(requestUri)
            {
                ConfigureRequestMessage = (req, args, token) =>
                {
                    req.Headers.TryAddWithoutValidation("accept", accept);
                },
            };
            sendArgs.SetHttpClient(CreateClient());
            return await SendAsync<T>(sendArgs, cancellationToken);
        }
        catch
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string>> GetAllSteamAppsString(CancellationToken cancellationToken = default)
    {
        var rsp = await GetAsync<string>(SteamApiUrls.STEAMAPP_LIST_URL, cancellationToken: cancellationToken);
        return ApiRspHelper.Ok(rsp ?? string.Empty)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<List<SteamApp>>> GetAllSteamAppList(CancellationToken cancellationToken = default)
    {
        var rsp = await GetAsync<SteamApps>(SteamApiUrls.STEAMAPP_LIST_URL, cancellationToken: cancellationToken);
        return (rsp?.AppList?.Apps ?? [])!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamUser>> GetUserInfo(long steamId64, CancellationToken cancellationToken = default)
    {
        //因为某些原因放弃从社区页链接获取详细资料
        //var requestUri = string.Format(SteamApiUrls.STEAM_USERINFO_XML_URL, steamId64);
        //var rsp = await s.GetAsync<SteamUser>(requestUri);

        var data = new SteamUser() { SteamId64 = steamId64 };
        var rsp = (await GetUserMiniProfile(data.SteamId32, cancellationToken: cancellationToken)).Content;
        if (rsp != null)
        {
            data.MiniProfile = rsp;
            data.SteamID = WebUtility.HtmlDecode(rsp.PersonaName);
            data.AvatarFull = rsp.AvatarUrl;
            data.AvatarMedium = rsp.AvatarUrl;
        }
        return data!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamMiniProfile?>> GetUserMiniProfile(long steamId3, CancellationToken cancellationToken = default)
    {
        var requestUri = string.Format(SteamApiUrls.STEAM_MINIPROFILE_URL, steamId3);
        var rsp = await GetAsync<SteamMiniProfile>(requestUri, cancellationToken: cancellationToken);
        return rsp;
    }
}