namespace BD.SteamClient8.Impl.WebApi;

/// <summary>
/// <see cref="ISteamSessionService"/> Steam 登录会话信息服务实现
/// </summary>
public class SteamSessionServiceImpl : WebApiClientFactoryService, ISteamSessionService
{
    private const string TAG = "SteamSessionS";

    /// <inheritdoc/>
    protected sealed override SystemTextJsonSerializerOptions JsonSerializerOptions =>
        DefaultJsonSerializerContext_.Default.Options;

    /// <inheritdoc/>
    protected sealed override string ClientName => TAG;

    private readonly ConcurrentDictionary<string, SteamSession> _sessions;

    /// <summary>
    /// 初始化 <see cref="SteamSessionServiceImpl"/> 类的新实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    public SteamSessionServiceImpl(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(TAG),
            serviceProvider)
    {
        _sessions = new ConcurrentDictionary<string, SteamSession>();
    }

    /// <inheritdoc/>
    public bool AddOrSetSession(SteamSession steamSession)
    {
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
    public SteamSession? RentSession(string steam_id)
    {
        _ = _sessions.TryGetValue(SpecialTag(steam_id), out var steamSession);
        return steamSession;
    }

    /// <inheritdoc/>
    public bool RemoveSession(string steam_id)
    {
        if (_sessions.TryRemove(SpecialTag(steam_id), out var steamSession))
        {
            steamSession?.HttpClient?.Dispose();
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> SaveSession(SteamSession steamSession)
    {
        try
        {
            var temp = Serializable.SJSON(steamSession);
            await ISecureStorage.Instance.SetAsync("CurrentSteamUserSession", temp);
            return true;
        }
        catch (Exception ex)
        {
            ex.LogAndShow();
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<SteamSession?> LoadSession(string filePath)
    {
        try
        {
            var text = await ISecureStorage.Instance.GetAsync("CurrentSteamUserSession");
            if (text != null)
            {
                var session = Serializable.DJSON<SteamSession>(text);
                if (session != null && (await Ioc.Get<ISteamAccountService>().CheckAccessTokenValidation(session.AccessToken)).Content)
                {
                    session.GenerateSetCookie();
                    AddOrSetSession(session);
                    return session;
                }
            }
        }
        catch (Exception ex)
        {
            ex.LogAndShow();
        }
        return null;
    }

    private static string SpecialTag(string steam_id) => $"SteamSession_{steam_id}";
}
