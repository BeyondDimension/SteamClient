namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600 // Elements should be documented

public sealed class SteamAuthenticatorServiceImpl : WebApiClientFactoryService, ISteamAuthenticatorService
{
    protected sealed override string ClientName => TAG;

    protected sealed override SystemTextJsonSerializerContext? JsonSerializerContext => DefaultJsonSerializerContext_.Default;

    public const string TAG = "SteamAuthenticatorWebApiS";

    readonly ISteamSessionService _sessionService;

    public SteamAuthenticatorServiceImpl(
        ISteamSessionService steamSessionService,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider) : base(
            loggerFactory.CreateLogger(TAG),
            serviceProvider)
    {
        _sessionService = steamSessionService;
    }

    const string ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

    public async Task<ApiRspImpl<IsAccountWaitingForEmailConfirmationResponse?>> AccountWaitingForEmailConfirmation(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelease login first");

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_ACCOUNTWAITINGFOREMAILCONF.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        return await SendAsync<IsAccountWaitingForEmailConfirmationResponse>(sendArgs);
    }

    public async Task<ApiRspImpl<SteamDoLoginTfaJsonStruct?>> AddAuthenticatorAsync(string steam_id, string authenticator_time, string? device_identifier, string authenticator_type = "1", string sms_phone_id = "1")
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelease login first");

        var data = new Dictionary<string, string>
        {
            { "steamid", steamSession.SteamId },
            { "authenticator_time", authenticator_time },
            { "authenticator_type", authenticator_type },
            { "device_identifier", device_identifier ?? "" },
            { "sms_phone_id", sms_phone_id }
        };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_ADD.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        return await SendAsync<SteamDoLoginTfaJsonStruct, Dictionary<string, string>>(sendArgs, data);
    }

    public async Task<ApiRspImpl<SteamAddPhoneNumberResponse?>> AddPhoneNumberAsync(string steam_id, string phone_number, string? contury_code)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var data = new Dictionary<string, string>
        {
            { "phone_number", phone_number },
            { "phone_country_code", contury_code ?? "" }
        };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_ADD_PHONENUMBER.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        return await SendAsync<SteamAddPhoneNumberResponse, Dictionary<string, string>>(sendArgs, data);
    }

    public async Task<ApiRspImpl<SteamDoLoginFinalizeJsonStruct?>> FinalizeAddAuthenticatorAsync(string steam_id, string? activation_code, string authenticator_code, string authenticator_time, string validate_sms_code = "1")
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var data = new Dictionary<string, string>
        {
            { "steamid", steamSession.SteamId },
            { "activation_code", activation_code ?? "" },
            { "validate_sms_code", validate_sms_code },
            { "authenticator_code", authenticator_code },
            { "authenticator_time", authenticator_time }
        };
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_FINALIZEADD.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        return await SendAsync<SteamDoLoginFinalizeJsonStruct, Dictionary<string, string>>(sendArgs, data);
    }

    public async Task<ApiRspImpl<GetUserCountryResponse?>> GetUserCountry(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var param = new Dictionary<string, string>
        {
            { "steamid", steamSession.SteamId }
        };
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_GET_USERCOUNTRY.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        return await SendAsync<GetUserCountryResponse, Dictionary<string, string>>(sendArgs, param);
    }

    public async Task<ApiRspImpl<RemoveAuthenticatorResponse?>> RemoveAuthenticatorAsync(string steam_id, string? revocation_code, string steamguard_scheme, string revocation_reason = "1")
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var param = new Dictionary<string, string>
        {
            { "revocation_code", revocation_code ?? throw new Exception("恢复代码为null") },
            { "revocation_reason", revocation_reason },
            { "steamguard_scheme", steamguard_scheme }
        };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_REMOVE.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        return await SendAsync<RemoveAuthenticatorResponse, Dictionary<string, string>>(sendArgs, param);
    }

    public async Task<ApiRspImpl<CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response?>> RemoveAuthenticatorViaChallengeStartSync(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var base64string = UrlEncoder.Default.Encode(new CTwoFactor_RemoveAuthenticatorViaChallengeStart_Request().ToByteString().ToBase64());
        var data = new Dictionary<string, string>()
        {
            { "input_protobuf_encoded", base64string }
        };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGESTARTSYNC.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<string, Dictionary<string, string>>(sendArgs, data);
        return CTwoFactor_RemoveAuthenticatorViaChallengeStart_Response.Parser.ParseJson(response);
    }

    public async Task<ApiRspImpl<CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response?>> RemoveAuthenticatorViaChallengeContinueSync(string steam_id, string? sms_code, bool generate_new_token = true)
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

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGECONTINUESYNC.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<string, Dictionary<string, string>>(sendArgs, data);
        return CTwoFactor_RemoveAuthenticatorViaChallengeContinue_Response.Parser.ParseJson(response);
    }

    public async Task<ApiRspImpl<bool>> SendPhoneVerificationCode(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            return string.Empty;

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_SEND_PHONEVERIFICATIONCODE.Format(steamSession.AccessToken))
        {
            Method = HttpMethod.Post,
            ContentType = ContentType
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        return (await SendAsync<HttpResponseMessage>(sendArgs)).ThrowIsNull().IsSuccessStatusCode;
    }

    public async Task<ApiRspImpl<SteamSyncStruct?>> TwoFAQueryTime()
    {
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_TWOFAQUERYTIME) { Method = HttpMethod.Post };
        sendArgs.SetHttpClient(CreateClient());
        return await SendAsync<SteamSyncStruct>(sendArgs);
    }

    public async Task<ApiRspImpl<string>> RefreshAccessToken(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        if (string.IsNullOrEmpty(steamSession.RefreshToken))
            throw new Exception("Refresh token is empty");

        if (IsTokenExpired(steamSession.RefreshToken))
            throw new Exception("Refresh token is expired");

        GenerateAccessTokenForAppResponse? response;
        try
        {
            var postData = new Dictionary<string, string>
            {
                { "refresh_token", steamSession.RefreshToken },
                { "steamid", steamSession.SteamId.ToString() }
            };

            using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_AUTHENTICATOR_REFRESHACCESSTOKEN) { Method = HttpMethod.Post, ContentType = ContentType };
            sendArgs.SetHttpClient(steamSession.HttpClient!);

            response = await SendAsync<GenerateAccessTokenForAppResponse, Dictionary<string, string>>(sendArgs, postData);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to refresh token: " + ex.Message);
        }

        return ApiRspHelper.Ok(response.ThrowIsNull()?.Response?.AccessToken ?? string.Empty)!;
    }

    #region Private

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    private bool IsTokenExpired(string token)
    {
        var tokenComponents = token.Split('.');
        // Fix up base64url to normal base64
        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0)
        {
            base64 += new string('=', 4 - (base64.Length % 4));
        }

        var payloadBytes = Convert.FromBase64String(base64);
        var jwt = SystemTextJsonSerializer.Deserialize<SteamAccessToken>(System.Text.Encoding.UTF8.GetString(payloadBytes));

        // Compare expire time of the token to the current time
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > jwt.Exp;
    }

    private class SteamAccessToken
    {
        [JsonPropertyName("exp")]
        public long Exp { get; set; }
    }
    #endregion
}
