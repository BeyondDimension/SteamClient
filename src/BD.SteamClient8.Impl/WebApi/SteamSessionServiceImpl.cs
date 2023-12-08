namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600
public class SteamSessionServiceImpl : ISteamSessionService
{
    private const string TAG = "SteamSessionS";

    private readonly ConcurrentDictionary<string, SteamSession> _sessions;

    public SteamSessionServiceImpl()
    {
        _sessions = new ConcurrentDictionary<string, SteamSession>();
    }

    public bool AddOrSetSession(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.SteamId))
            return false;

        var tag = SpecialTag(steamSession.SteamId);
        var handler = new HttpClientHandler()
        {
            UseCookies = true,
            CookieContainer = steamSession.CookieContainer,
        };
        steamSession.HttpClient = new HttpClient(handler);
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
