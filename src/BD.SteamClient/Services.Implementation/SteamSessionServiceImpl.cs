using static System.Net.Http.HttpClientUseCookiesWithProxyServiceImpl;

namespace BD.SteamClient.Services.Implementation;

public class SteamSessionServiceImpl : ISteamSessionService
{
    private const string TAG = "SteamSessionS";

    private readonly ConcurrentDictionary<string, SteamSession> _sessions;

    public SteamSessionServiceImpl()
    {
        _sessions = new ConcurrentDictionary<string, SteamSession>();
    }

    public bool AddOrSetSeesion(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.SteamId))
            return false;

        var tag = SpecialTag(steamSession.SteamId);
        var handler = new HttpClientHandler()
        {
            UseCookies = true,
            CookieContainer = steamSession.CookieContainer,
            Proxy = HttpClient.DefaultProxy,
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
            await ISecureStorage.Instance.SetAsync(ISteamSessionService.CurrentSteamUserKey, temp);
            return true;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
        return false;
    }

    public async Task<SteamSession?> LoadSession()
    {
        try
        {
            var text = await ISecureStorage.Instance.GetAsync(ISteamSessionService.CurrentSteamUserKey);
            if (text != null)
            {
                var session = Serializable.DJSON<SteamSession>(text);
                if (session != null && await Ioc.Get<ISteamAccountService>().CheckAccessTokenValidation(session.AccessToken))
                {
                    session.GenerateSetCookie();
                    AddOrSetSeesion(session);
                    return session;
                }
            }
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
        return null;
    }

    public async Task UnlockParental(string steam_id, string pinCode)
    {
        var session = RentSession(steam_id).ThrowIsNull();
        foreach (var unlock_url in Unlock_urls())
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, unlock_url)
            {
                Content = new MultipartFormDataContent()
                {
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(pinCode)), "pin" },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(session.CookieContainer.GetCookies(new Uri(unlock_url))["sessionid"]?.Value ?? string.Empty)), "sessionid" },
                }
            };
            var response = await session.HttpClient!.SendAsync(request);
        }

        IEnumerable<string> Unlock_urls()
        {
            yield return SteamApiUrls.STEAM_PARENTAL_UNLOCK_STORE;
            yield return SteamApiUrls.STEAM_PARENTAL_UNLOCK_COMMUNITY;
        }
    }

    public async Task LockParental(string steam_id)
    {
        var session = RentSession(steam_id).ThrowIsNull();
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
    }

    private static string SpecialTag(string steam_id) => $"SteamSession_{steam_id}";
}
