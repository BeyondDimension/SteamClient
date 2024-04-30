using Strings = BD.SteamClient8.Resources.Strings;

namespace BD.SteamClient8.Extensions;

public static partial class WinAuthExtensions
{
    /// <summary>
    /// 调用 Steam 添加令牌接口
    /// </summary>
    /// <returns>调用成功返回 <see langword="true"/></returns>
    public static async Task<ApiRspImpl<bool>> AddAuthenticatorAsync(
        this SteamAuthenticator @this,
        SteamAuthenticator.EnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken))
            return Strings.Error_InvalidLoginInfo;

        state.Error = null;
        if (@this.ServerTimeDiff == default)
            await Task.Run(@this.Sync);
        var deviceId = SteamAuthenticator.BuildRandomId();

        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var response = await steamAuthenticatorService.AddAuthenticatorAsync(state.SteamId.ToString(), @this.ServerTime.ToString(), deviceId);

        var tfaResponse = response.Content;

        if (tfaResponse?.Response == null)
        {
            state.AccessToken = null;
            state.Cookies = null;
            return state.Error = Strings.Error_InvalidResponseFromSteam.Format(response.GetMessage());
        }

        if (tfaResponse.Response.Status == default || tfaResponse.Response.Status == 84)
        {
            // invalid response
            return state.Error = Strings.Error_ITwoFactorService_AddAuthenticator_v0001;
        }

        // if (!response.Contains("shared_secret", StringComparison.CurrentCulture))
        // {
        //     // invalid response
        //    return state.Error = Strings.error_invalid_response_from_steam_.Format(response);
        // }

        //账号没有绑定手机号
        switch (tfaResponse.Response.Status)
        {
            case 2:
                //state.Error = Strings.error_steamguard_phoneajax_.Format(Environment.NewLine);
                state.Error = Strings.Error_AccountNotBindTel;
                state.NoPhoneNumber = true;
                return state.Error;

            case 29:
                state.Error = Strings.Error_HasAuthenticator;
                return state.Error;
        }

        state.NoPhoneNumber = false;

        // save data into this authenticator
        var secret = tfaResponse.Response.SharedSecret;
        //SecretKey = Convert.FromBase64String(secret);
        @this.SecretKey = Base64Extensions.Base64DecodeToByteArray_Nullable(secret);
        @this.Serial = tfaResponse.Response.SerialNumber;
        @this.DeviceId = deviceId;
        state.RevocationCode = tfaResponse.Response.RevocationCode;

        // add the steamid into the data
        var steamdata = tfaResponse.Response;
        if (steamdata.SteamId != 0 && state.SteamId != 0)
            steamdata.SteamId = state.SteamId;
        if (steamdata.SteamGuardScheme == string.Empty)
            steamdata.SteamGuardScheme = "2";
        @this.SteamData = SystemTextJsonSerializer.Serialize(steamdata,
            DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);

        // calculate server drift
        var servertime = tfaResponse.Response.ServerTime * 1000;
        @this.ServerTimeDiff = servertime - AuthenticatorValueModel.CurrentTime;
        @this.LastServerTime = DateTime.Now.Ticks;

        state.RequiresActivation = true;

        state.Error = Strings.RequiresActivation;
        return true;
    }

    /// <summary>
    /// 完成添加认证器
    /// </summary>
    public static async Task<ApiRspImpl<bool>> FinalizeAddAuthenticatorAsync(
        this SteamAuthenticator @this,
        SteamAuthenticator.EnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken))
            return Strings.Error_InvalidLoginInfo;

        state.Error = null;

        if (@this.ServerTimeDiff == default)
            await Task.Run(@this.Sync);
        // finalize adding the authenticator

        // try and authorise
        var retries = 0;
        while (state.RequiresActivation == true && retries < SteamAuthenticator.ENROLL_ACTIVATE_RETRIES)
        {
            var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
            var response = await steamAuthenticatorService.FinalizeAddAuthenticatorAsync(
                    steam_id: state.SteamId.ToString(),
                    activation_code: state.ActivationCode,
                    authenticator_code: @this.CalculateCode(false),
                    authenticator_time: @this.ServerTime.ToString()
                );
            var finalizeResponse = response.Content;
            finalizeResponse.ThrowIsNull();
            if (finalizeResponse.Response == null)
            {
                return state.Error = Strings.Error_InvalidResponseFromSteam.Format(response);
            }

            if (finalizeResponse.Response.Status != default && finalizeResponse.Response.Status == SteamAuthenticator.INVALID_ACTIVATION_CODE)
            {
                return state.Error = Strings.Error_InvalidActivationCode;
            }

            // reset our time
            if (string.IsNullOrEmpty(finalizeResponse.Response.ServerTime))
            {
                var servertime = long.Parse(finalizeResponse.Response.ServerTime) * 1000;
                @this.ServerTimeDiff = servertime - AuthenticatorValueModel.CurrentTime;
                @this.LastServerTime = DateTime.Now.Ticks;
            }

            // check success
            if (finalizeResponse.Response.Success == true)
            {
                if (finalizeResponse.Response.WantMore == true)
                {
                    @this.ServerTimeDiff += @this.Period * 1000L;
                    retries++;
                    continue;
                }

                state.RequiresActivation = false;
                break;
            }

            @this.ServerTimeDiff += @this.Period * 1000L;
            retries++;
        }

        if (state.RequiresActivation == true)
        {
            return state.Error = Strings.Error_OnActivating;
        }

        // mark and successful and return key
        state.Success = true;
        state.SecretKey = AuthenticatorValueModel.ByteArrayToString(@this.SecretKey.ThrowIsNull());

        // // send confirmation email
        // data.Clear();
        // data.Add("access_token", state.AccessToken);
        // data.Add("steamid", state.SteamId);
        // data.Add("email_type", "2");
        //
        // _ = await RequestAsync(WEBAPI_BASE + "/ITwoFactorService/SendEmail/v0001", "POST", data);

        return true;
    }

    /// <summary>
    /// 获取 Rsa 密钥和加密密码
    /// </summary>
    public static async Task<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>> GetRsaKeyAndEncryptedPasswordAsync(
        this SteamAuthenticator @this,
        SteamAuthenticator.EnrollState state)
    {
        state.Username.ThrowIsNull();
        state.Password.ThrowIsNull();

        // get the user's RSA key

        var steamAccountService = Ioc.Get<ISteamAccountService>();
        var rsaResponse = await steamAccountService.GetRSAkeyV2Async(state.Username, state.Password);

        return rsaResponse;
    }

    /// <summary>
    /// Steam 账户添加绑定手机号
    /// </summary>
    /// <returns>返回错误信息，isOK 标识执行成功，infoMessage 为是否需要显示提示信息</returns>
    public static async Task<ApiRspImpl<(bool isOK, string? infoMessage)>> AddPhoneNumberAsync(
        this ISteamAuthenticatorService steamAuthenticatorService,
        SteamAuthenticator.EnrollState state,
        string phoneNumber,
        string? countryCode = null)
    {
        state.AccessToken.ThrowIsNull();

        if (!state.RequiresEmailConfirmPhone)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                var userCountryOrRegion = await steamAuthenticatorService.GetUserCountryOrRegion(state.SteamId.ToString());
                if (string.IsNullOrWhiteSpace(userCountryOrRegion.Content?.Response?.CountryOrRegion))
                {
                    return userCountryOrRegion.GetMessage();
                }
                countryCode = userCountryOrRegion.Content.Response.CountryOrRegion;
            }

            var steamAddPhoneNumberResponse = (await steamAuthenticatorService.AddPhoneNumberAsync(state.SteamId.ToString(), phoneNumber, countryCode)).Content;
            steamAddPhoneNumberResponse.ThrowIsNull();
            steamAddPhoneNumberResponse.Response.ThrowIsNull();

            if (steamAddPhoneNumberResponse.Response.ConfirmationEmailAddress == null)
                return Strings.AccountNotBindEmail; // Error 级别提示消息
            state.EmailDomain = steamAddPhoneNumberResponse.Response.ConfirmationEmailAddress;
            state.PhoneNumber = steamAddPhoneNumberResponse.Response.PhoneNumberFormatted;
            state.RequiresEmailConfirmPhone = true;
        }

        var waitingForEmailConfirmationResponse = (await steamAuthenticatorService.AccountWaitingForEmailConfirmation(state.SteamId.ToString())).Content;

        waitingForEmailConfirmationResponse.ThrowIsNull();
        waitingForEmailConfirmationResponse.Response.ThrowIsNull();

        if (!waitingForEmailConfirmationResponse.Response.AwaitingEmailConfirmation)
        {
            await steamAuthenticatorService.SendPhoneVerificationCode(state.SteamId.ToString());
            state.RequiresEmailConfirmPhone = false;
            return (true, null);
        }

        // Info 级别 Toast
        return (true, Strings.ConfirmLinkInEmail);
    }

    /// <summary>
    /// Steam 移除安全防护
    /// </summary>
    /// <param name="this"></param>
    /// <param name="steam_id"></param>
    /// <param name="scheme">1 = 移除令牌验证器但保留邮箱验证，2 = 移除所有防护</param>
    /// <returns></returns>
    public static async Task<ApiRspImpl<bool>> RemoveAuthenticatorAsync(
        this SteamAuthenticator @this,
        string steam_id,
        int scheme = 1)
    {
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var result = await steamAuthenticatorService.RemoveAuthenticatorAsync(steam_id, @this.RecoveryCode, scheme.ToString());
        if (!result.IsSuccess)
            return ApiRspHelper.Create<bool>(result);
        if (result.Content == null)
            return ApiRspCode.NoResponseContent;
        if (result.Content.Response?.Success != true)
            return Strings.RemoveAuthenticatorFail_.Format(result.Content.Response?.RevocationAttemptsRemaining);
        return true;
    }

    /// <summary>
    /// 申请 Steam 替换安全防护令牌验证码
    /// </summary>
    /// <param name="steamAuthenticatorService"></param>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    public static async Task<ApiRspImpl<bool>> RemoveAuthenticatorViaChallengeStartSync2(
        this ISteamAuthenticatorService steamAuthenticatorService,
        string steam_id)
    {
        var result = await steamAuthenticatorService.RemoveAuthenticatorViaChallengeStartSync(steam_id);

        if (!result.IsSuccess)
            return ApiRspHelper.Create<bool>(result);
        if (result.Content == null)
            return ApiRspCode.NoResponseContent;

        // 返回内容正常序列化即表示成功
        return result.Content != null;
    }

    /// <summary>
    /// Steam 替换安全防护令牌
    /// </summary>
    public static async Task<ApiRspImpl<bool>> RemoveAuthenticatorViaChallengeContinueSync(
        this SteamAuthenticator @this,
        string steam_id,
        string? sms_code,
        bool generate_new_token = true)
    {
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var result = await steamAuthenticatorService.RemoveAuthenticatorViaChallengeContinueSync(steam_id, sms_code, generate_new_token);
        if (!result.IsSuccess)
            return ApiRspHelper.Create<bool>(result);
        if (result.Content == null)
            return ApiRspCode.NoResponseContent;

        var response = result.Content;
        if (!response.Success || response.ReplacementToken == null)
        {
            return Strings.Error_InvalidResponseFromSteam.Format("ReplacementToken is null.");
        }

        // save data into this authenticator
        @this.SecretKey = response.ReplacementToken.SharedSecret.ToByteArray();
        @this.Serial = response.ReplacementToken.SerialNumber.ToString();
        @this.DeviceId = SteamAuthenticator.BuildRandomId();
        @this.SteamData = SystemTextJsonSerializer.Serialize(new SteamConvertSteamDataJsonStruct
        {
            Secret_1 = response.ReplacementToken.Secret1.ToBase64().Base64Encode(),
            Status = response.ReplacementToken.Status,
            ServerTime = (long)response.ReplacementToken.ServerTime,
            AccountName = response.ReplacementToken.AccountName,
            TokenGid = response.ReplacementToken.TokenGid,
            IdentitySecret = response.ReplacementToken.IdentitySecret.ToBase64().Base64Encode(),
            RevocationCode = response.ReplacementToken.RevocationCode,
            Uri = response.ReplacementToken.Uri,
            SteamId = (long)response.ReplacementToken.Steamid,
            SerialNumber = response.ReplacementToken.SerialNumber.ToString(),
            SharedSecret = response.ReplacementToken.SharedSecret.ToBase64().Base64Encode(),
            SteamGuardScheme = response.ReplacementToken.SteamguardScheme.ToString(),
        }, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);

        // calculate server drift
        var servertime = (long)response.ReplacementToken.ServerTime * 1000;
        @this.ServerTimeDiff = servertime - SteamAuthenticator.CurrentTime;
        @this.LastServerTime = DateTime.Now.Ticks;

        return true;
    }
}
