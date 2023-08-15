using Google.Protobuf;

namespace BD.SteamClient.Services.Implementation;

public class SteamAuthenticatorServiceImpl : HttpClientUseCookiesWithDynamicProxyServiceImpl, ISteamAuthenticatorService
{
    private ISteamSessionService _sessionService;

    public SteamAuthenticatorServiceImpl(
        IServiceProvider s,
        ILogger<SteamTradeServiceImpl> logger) : base(
            s, logger)
    {
        _sessionService = s.GetRequiredService<ISteamSessionService>();
    }

    public SteamAuthenticatorServiceImpl(
        IServiceProvider s,
        Func<CookieContainer, HttpMessageHandler> func) : base(func, s.GetRequiredService<ILogger<SteamTradeServiceImpl>>())
    {
        _sessionService = s.GetRequiredService<ISteamSessionService>();
    }

    public async Task<string> AccountWaitingForEmailConfirmation(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        return await RequestAsync(STEAM_AUTHENTICATOR_ACCOUNTWAITINGFOREMAILCONF.Format(steamSession.AccessToken), HttpMethod.Post, httpClient: steamSession.HttpClient);
    }

    public async Task<string> AddAuthenticatorAsync(string steam_id, string authenticator_time, string? device_identifier, string authenticator_type = "1", string sms_phone_id = "1")
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var data = new Dictionary<string, string>();
        data.Add("steamid", steamSession.SteamId);
        data.Add("authenticator_time", authenticator_time);
        data.Add("authenticator_type", authenticator_type);
        data.Add("device_identifier", device_identifier ?? "");
        data.Add("sms_phone_id", sms_phone_id);

        return await RequestAsync(STEAM_AUTHENTICATOR_ADD.Format(steamSession.AccessToken), HttpMethod.Post, data: data, httpClient: steamSession.HttpClient);
    }

    public async Task<string> AddPhoneNumberAsync(string steam_id, string phone_number, string? contury_code)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var data = new Dictionary<string, string>();
        data.Add("phone_number", phone_number);
        data.Add("phone_country_code", contury_code ?? "");
        return await RequestAsync(STEAM_AUTHENTICATOR_ADD_PHONENUMBER.Format(steamSession.AccessToken), HttpMethod.Post, data: data, httpClient: steamSession.HttpClient);
    }

    public async Task<string> FinalizeAddAuthenticatorAsync(string steam_id, string? activation_code, string authenticator_code, string authenticator_time, string validate_sms_code = "1")
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var data = new Dictionary<string, string>();
        data.Add("steamid", steamSession.SteamId);
        data.Add("activation_code", activation_code ?? "");
        data.Add("validate_sms_code", validate_sms_code);
        data.Add("authenticator_code", authenticator_code);
        data.Add("authenticator_time", authenticator_time);
        return await RequestAsync(STEAM_AUTHENTICATOR_FINALIZEADD.Format(steamSession.AccessToken), HttpMethod.Post, data: data, httpClient: steamSession.HttpClient);
    }

    public async Task<string> GetUserCountry(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var param = new Dictionary<string, string>();
        param.Add("steamid", steamSession.SteamId);
        return await RequestAsync(STEAM_AUTHENTICATOR_GET_USERCOUNTRY.Format(steamSession.AccessToken), HttpMethod.Post, data: param, httpClient: steamSession.HttpClient);
    }

    public async Task<string> RemoveAuthenticatorAsync(string steam_id, string? revocation_code, string steamguard_scheme, string revocation_reason = "1")
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var param = new Dictionary<string, string>();
        param.Add("revocation_code", revocation_code ?? throw new Exception("恢复代码为null"));
        param.Add("revocation_reason", revocation_reason);
        param.Add("steamguard_scheme", steamguard_scheme);

        var url = STEAM_AUTHENTICATOR_REMOVE.Format(steamSession.AccessToken);
        return await RequestAsync(url, HttpMethod.Post, data: param, httpClient: steamSession.HttpClient);
    }

    public async Task<CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response?> RemoveAuthenticatorViaChallengeStartSync(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var base64string = UrlEncoder.Default.Encode(new CTwoFactor_RemoveAuthenticatorViaChallengeStart_Request().ToByteString().ToBase64());
        var data = new Dictionary<string, string>()
        {
            { "input_protobuf_encoded", base64string }
        };
        var url = STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGESTARTSYNC.Format(steamSession.AccessToken);
        var response = await RequestAsync(url, HttpMethod.Post, data, httpClient: steamSession.HttpClient);
        return CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response.Parser.ParseJson(response);
    }

    public async Task<CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response?> RemoveAuthenticatorViaChallengeContinueSync(string steam_id, string? sms_code, bool generate_new_token = true)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var base64string = UrlEncoder.Default.Encode(new CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Request
        {
            SmsCode = sms_code,
            GenerateNewToken = generate_new_token
        }.ToByteString().ToBase64());

        var data = new Dictionary<string, string>()
        {
            { "input_protobuf_encoded", base64string }
        };
        var url = STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGECONTINUESYNC.Format(steamSession.AccessToken);
        var response = await RequestAsync(url, HttpMethod.Post, data, httpClient: steamSession.HttpClient);
        return CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response.Parser.ParseJson(response);
    }

    public async Task<string> SendPhoneVerificationCode(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            return string.Empty;

        return await RequestAsync(STEAM_AUTHENTICATOR_SEND_PHONEVERIFICATIONCODE.Format(steamSession.AccessToken), HttpMethod.Post, httpClient: steamSession.HttpClient);
    }

    public async Task<string> TwoFAQueryTime()
    {
        return await RequestAsync(STEAM_AUTHENTICATOR_TWOFAQUERYTIME, HttpMethod.Post);
    }

    public async Task<string> RefreshAccessToken(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            return string.Empty;

        if (string.IsNullOrEmpty(steamSession.RefreshToken))
            throw new Exception("Refresh token is empty");

        if (IsTokenExpired(steamSession.RefreshToken))
            throw new Exception("Refresh token is expired");

        string responseStr;
        try
        {
            var postData = new Dictionary<string, string>
            {
                { "refresh_token", steamSession.RefreshToken },
                { "steamid", steamSession.SteamId.ToString() }
            };
            responseStr = await RequestAsync(STEAM_AUTHENTICATOR_REFRESHACCESSTOKEN, HttpMethod.Post, postData, httpClient: steamSession.HttpClient);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to refresh token: " + ex.Message);
        }

        var response = JsonSerializer.Deserialize<GenerateAccessTokenForAppResponse>(responseStr);
        if (response != null && !string.IsNullOrEmpty(response.Response.AccessToken))
        {
            steamSession.AccessToken = response.Response.AccessToken;
            return response.Response.AccessToken;
        }
        return string.Empty;

    }

    #region Private

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
        return await response.Content.ReadAsStringAsync();
    }

    private bool IsTokenExpired(string token)
    {
        var tokenComponents = token.Split('.');
        // Fix up base64url to normal base64
        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0)
        {
            base64 += new string('=', 4 - base64.Length % 4);
        }

        var payloadBytes = Convert.FromBase64String(base64);
        var jwt = JsonSerializer.Deserialize<SteamAccessToken>(System.Text.Encoding.UTF8.GetString(payloadBytes));

        // Compare expire time of the token to the current time
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > jwt.Exp;
    }

    private class SteamAccessToken
    {
        [JsonPropertyName("exp")]
        public long Exp { get; set; }
    }

    private class GenerateAccessTokenForAppResponse
    {
        [JsonPropertyName("response")]
        public GenerateAccessTokenForAppResponseResponse? Response;
    }

    private class GenerateAccessTokenForAppResponseResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
    #endregion
}
