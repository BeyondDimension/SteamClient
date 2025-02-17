namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamSessionService"/> Steam 登录会话信息服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamSessionServiceImpl"/> 类的新实例
/// </remarks>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
public sealed class SteamSessionServiceImpl(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamSessionService
{
    const string TAG = "SteamSessionS";

    /// <inheritdoc/>
    protected sealed override SystemTextJsonSerializerOptions JsonSerializerOptions =>
        DefaultJsonSerializerContext_.Default.Options;

    /// <inheritdoc/>
    protected sealed override string ClientName => TAG;

    readonly ConcurrentDictionary<string, SteamSession> _sessions = new();

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> AddOrSetSession(SteamSession steamSession, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (string.IsNullOrEmpty(steamSession.SteamId))
            return false;

        var tag = SpecialTag(steamSession.SteamId);
        steamSession.HttpClient = CreateClient(tag);
        var container = GetCookieContainer(tag);
        container.Add(steamSession.Cookies);
        _sessions[tag] = steamSession;
        return true;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamSession?>> RentSession(string steam_id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        _ = _sessions.TryGetValue(SpecialTag(steam_id), out var steamSession);
        return steamSession;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> RemoveSession(string steam_id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (_sessions.TryRemove(SpecialTag(steam_id), out var steamSession))
        {
            steamSession?.HttpClient?.Dispose();
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> SaveSession(SteamSession steamSession, CancellationToken cancellationToken = default)
    {
        try
        {
            var temp = SystemTextJsonSerializer.Serialize(steamSession,
                SteamSessionServiceImpl_SteamSession_JsonSerializerContext_.Default.SteamSession);
            await ISecureStorage.Instance.SetAsync(ISteamSessionService.CurrentSteamUserKey, temp);
            return true;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamSession?>> LoadSession(CancellationToken cancellationToken = default)
    {
        try
        {
            var text = await ISecureStorage.Instance.GetAsync(ISteamSessionService.CurrentSteamUserKey);
            if (text != null)
            {
                var session = SystemTextJsonSerializer.Deserialize(text,
                    SteamSessionServiceImpl_SteamSession_JsonSerializerContext_.Default.SteamSession);
                if (session != null)
                {
                    var steamAccountService = Ioc.Get<ISteamAccountService>();
                    var checkAccessTokenValidResult = await steamAccountService.CheckAccessTokenValidation(session.AccessToken, cancellationToken);
                    if (checkAccessTokenValidResult.IsSuccess && checkAccessTokenValidResult.Content)
                    {
                        session.GenerateSetCookie();
                        await AddOrSetSession(session, cancellationToken);
                        return session;
                    }
                }
            }
            return ApiRspHelper.Ok<SteamSession>(null);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public async Task<ApiRspImpl> UnlockParental(string steam_id, string pinCode, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiRspHelper.Fail("取消操作");

        var session = (await RentSession(steam_id)).Content.ThrowIsNull();
        var tag = SpecialTag(steam_id);
        var container = GetCookieContainer(tag);
        foreach (var unlock_url in Unlock_urls())
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, unlock_url)
            {
                Content = new MultipartFormDataContent()
                {
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(pinCode)), "pin" },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(container.GetCookies(new Uri(unlock_url))["sessionid"]?.Value ?? string.Empty)), "sessionid" },
                }
            };
            var response = await session.HttpClient!.SendAsync(request);
        }

        IEnumerable<string> Unlock_urls()
        {
            yield return SteamApiUrls.STEAM_PARENTAL_UNLOCK_STORE;
            yield return SteamApiUrls.STEAM_PARENTAL_UNLOCK_COMMUNITY;
        }

        return ApiRspHelper.Ok();
    }

    public async Task<ApiRspImpl> LockParental(string steam_id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ApiRspHelper.Fail("取消操作");

        var session = (await RentSession(steam_id)).Content.ThrowIsNull();
        foreach (var lock_url in lock_urls())
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, lock_url);
            await session.HttpClient!.SendAsync(request);
        }
        IEnumerable<string> lock_urls()
        {
            yield return SteamApiUrls.STEAM_PARENTAL_LOCK_STORE;
            yield return SteamApiUrls.STEAM_PARENTAL_LOCK_COMMUNITY;
        }

        return ApiRspHelper.Ok();
    }

    static string SpecialTag(string steam_id) => $"SteamSession_{steam_id}";
}

[SystemTextJsonSerializable(typeof(SteamSession))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
sealed partial class SteamSessionServiceImpl_SteamSession_JsonSerializerContext_ : SystemTextJsonSerializerContext
{
}