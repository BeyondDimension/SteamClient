namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600
public class SteamSessionServiceImpl : WebApiClientFactoryService, ISteamSessionService
{
    private const string TAG = "SteamSessionS";

    protected override SystemTextJsonSerializerContext? JsonSerializerContext => DefaultJsonSerializerContext_.Default;

    protected sealed override string ClientName => TAG;

    private readonly ConcurrentDictionary<string, SteamSession> _sessions;

    public SteamSessionServiceImpl(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger(TAG),
            serviceProvider)
    {
        _sessions = new ConcurrentDictionary<string, SteamSession>();
    }

    public bool AddOrSetSession(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.SteamId))
            return false;

        var tag = SpecialTag(steamSession.SteamId);
        steamSession.HttpClient = CreateClient(tag);
        steamSession.CookieContainer = GetCookieContainer(tag);
        _sessions[tag] = steamSession;
        return true;
    }

    public SteamSession? RentSession(string steam_id)
    {
        _ = _sessions.TryGetValue(SpecialTag(steam_id), out var steamSession);
        return steamSession;
    }

    public bool RemoveSession(string steam_id)
    {
        if (_sessions.TryRemove(SpecialTag(steam_id), out var steamSession))
        {
            steamSession?.HttpClient?.Dispose();
            return true;
        }
        return false;
    }

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
