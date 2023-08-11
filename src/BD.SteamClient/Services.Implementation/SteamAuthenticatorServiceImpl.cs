namespace BD.SteamClient.Services.Implementation;

public class SteamAuthenticatorServiceImpl : HttpClientUseCookiesWithDynamicProxyServiceImpl, ISteamAuthenticatorService
{

    public SteamAuthenticatorServiceImpl(
        IServiceProvider s,
        ILogger<SteamTradeServiceImpl> logger) : base(
            s, logger)
    {
    }

    public SteamAuthenticatorServiceImpl(
        IServiceProvider s,
        Func<CookieContainer, HttpMessageHandler> func) : base(func, s.GetRequiredService<ILogger<SteamTradeServiceImpl>>())
    {

    }

    public async Task<string> AccountWaitingForEmailConfirmation(SteamSession steamSession)
    {
        return await RequestAsync(STEAM_AUTHENTICATOR_ACCOUNTWAITINGFOREMAILCONF.Format(steamSession.OAuthToken), HttpMethod.Post, httpClient: steamSession.HttpClient);
    }

    public async Task<string> AddAuthenticatorAsync(SteamSession steamSession, string authenticator_time, string? device_identifier, string authenticator_type = "1", string sms_phone_id = "1")
    {
        var data = new Dictionary<string, string>();
        data.Add("steamid", steamSession.SteamId);
        data.Add("authenticator_time", authenticator_time);
        data.Add("authenticator_type", authenticator_type);
        data.Add("device_identifier", device_identifier ?? "");
        data.Add("sms_phone_id", sms_phone_id);

        return await RequestAsync(STEAM_AUTHENTICATOR_ADD.Format(steamSession.OAuthToken), HttpMethod.Post, data: data, httpClient: steamSession.HttpClient);
    }

    public async Task<string> AddPhoneNumberAsync(SteamSession steamSession, string phone_number, string? contury_code)
    {
        var data = new Dictionary<string, string>();
        data.Add("phone_number", phone_number);
        data.Add("phone_country_code", contury_code ?? "");
        return await RequestAsync(STEAM_AUTHENTICATOR_ADD_PHONENUMBER.Format(steamSession.OAuthToken), HttpMethod.Post, data: data, httpClient: steamSession.HttpClient);
    }

    public async Task<string> FinalizeAddAuthenticatorAsync(SteamSession steamSession, string? activation_code, string authenticator_code, string authenticator_time, string validate_sms_code = "1")
    {
        var data = new Dictionary<string, string>();
        data.Add("steamid", steamSession.SteamId);
        data.Add("activation_code", activation_code ?? "");
        data.Add("validate_sms_code", validate_sms_code);
        data.Add("authenticator_code", authenticator_code);
        data.Add("authenticator_time", authenticator_time);
        return await RequestAsync(STEAM_AUTHENTICATOR_FINALIZEADD.Format(steamSession.OAuthToken), HttpMethod.Post, data: data, httpClient: steamSession.HttpClient);
    }

    public async Task<string> GetUserCountry(SteamSession steamSession)
    {
        var param = new Dictionary<string, string>();
        param.Add("steamid", steamSession.SteamId);
        return await RequestAsync(STEAM_AUTHENTICATOR_GET_USERCOUNTRY.Format(steamSession.OAuthToken), HttpMethod.Post, data: param, httpClient: steamSession.HttpClient);
    }

    public async Task<string> RemoveAuthenticatorAsync(SteamSession steamSession, string? revocation_code, string steamguard_scheme, string revocation_reason = "1")
    {
        var param = new Dictionary<string, string>();
        param.Add("revocation_code", revocation_code ?? throw new Exception("恢复代码为null"));
        param.Add("revocation_reason", revocation_reason);
        param.Add("steamguard_scheme", steamguard_scheme);

        var url = STEAM_AUTHENTICATOR_REMOVE.Format(steamSession.OAuthToken);
        return await RequestAsync(url, HttpMethod.Post, data: param, httpClient: steamSession.HttpClient);
    }

    public async Task<string> SendPhoneVerificationCode(SteamSession steamSession)
    {
        return await RequestAsync(STEAM_AUTHENTICATOR_SEND_PHONEVERIFICATIONCODE.Format(steamSession.OAuthToken), HttpMethod.Post, httpClient: steamSession.HttpClient);
    }

    public async Task<string> TwoFAQueryTime()
    {
        return await RequestAsync(STEAM_AUTHENTICATOR_TWOFAQUERYTIME, HttpMethod.Post);
    }

    private async Task<string> RequestAsync(string url, HttpMethod httpMethod, Dictionary<string, string>? data = null, Dictionary<string, string>? headers = null, HttpClient? httpClient = null)
    {
        using var httpRequestMessage = new HttpRequestMessage(httpMethod, url);
        httpRequestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

        if (headers != null && headers.Count > 0)
            foreach ((var name, var value) in headers)
                httpRequestMessage.Headers.TryAddWithoutValidation(name, value);

        if (data != null && data.Count > 0)
        {
            if (httpMethod == HttpMethod.Post)
                httpRequestMessage.Content = new FormUrlEncodedContent(data);

            if (httpMethod == HttpMethod.Get)
            {
                var query = string.Join("&", data.Keys
                    .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(data[key]!)}"));
                var builder = new UriBuilder(url);
                builder.Query += query;
                httpRequestMessage.RequestUri = builder.Uri;
            }
        }

        var response = await (httpClient ?? client).SendAsync(httpRequestMessage);
        return Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
    }
}
