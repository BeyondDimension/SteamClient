using BD.Common8.Enums;
using BD.Common8.Extensions;
using BD.Common8.Helpers;
using BD.Common8.Models;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.WebApi.Authenticators;
using BD.SteamClient8.Services.Abstractions.WebApi;
using BD.SteamClient8.WinAuth.Models.Abstractions;
using System.Extensions;
using System.Text;
using System.Text.Json;
using Strings = BD.SteamClient8.Resources.Strings;

namespace BD.SteamClient8.Extensions;

public static partial class WinAuthExtensions
{
    /// <summary>
    /// 调用 Steam 添加令牌接口
    /// </summary>
    /// <returns>调用成功返回 <see langword="true"/></returns>
    public static async Task<ApiRspImpl<bool>> AddAuthenticatorAsync(
        this ISteamAuthenticator @this,
        ISteamAuthenticator.IEnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken))
            return Strings.Error_InvalidLoginInfo;

        state.Error = null;
        if (@this.ServerTimeDiff == default)
            await Task.Run(@this.Sync);
        var deviceId = ISteamAuthenticator.BuildRandomId();

        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var response = await steamAuthenticatorService.AddAuthenticatorAsync(state.SteamId.ToString(), @this.ServerTime.ToString(), deviceId);

        var tfaResponse = response;

        if (tfaResponse?.Response == null)
        {
            state.AccessToken = null;
            state.Cookies = null;
            return state.Error = Strings.Error_InvalidResponseFromSteam.Format("tfaResponse?.Response = null");
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

        // 账号没有绑定手机号
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
        @this.SteamData = JsonSerializer.Serialize(steamdata,
            DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);

        // calculate server drift
        var servertime = tfaResponse.Response.ServerTime * 1000;
        @this.ServerTimeDiff = servertime - IAuthenticatorValueModelBase.CurrentTime;
        @this.LastServerTime = DateTime.Now.Ticks;

        state.RequiresActivation = true;

        state.Error = Strings.RequiresActivation;
        return true;
    }

    /// <summary>
    /// 完成添加认证器
    /// </summary>
    public static async Task<ApiRspImpl<bool>> FinalizeAddAuthenticatorAsync(
        this ISteamAuthenticator @this,
        ISteamAuthenticator.IEnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken))
            return Strings.Error_InvalidLoginInfo;

        state.Error = null;

        if (@this.ServerTimeDiff == default)
            await Task.Run(@this.Sync);
        // finalize adding the authenticator

        // try and authorise
        var retries = 0;
        while (state.RequiresActivation == true && retries < ISteamAuthenticator.ENROLL_ACTIVATE_RETRIES)
        {
            var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
            var response = await steamAuthenticatorService.FinalizeAddAuthenticatorAsync(
                    steam_id: state.SteamId.ToString(),
                    activation_code: state.ActivationCode,
                    authenticator_code: @this.CalculateCode(false),
                    authenticator_time: @this.ServerTime.ToString()
                );
            var finalizeResponse = response;
            if (finalizeResponse?.Response == null)
            {
                return state.Error = Strings.Error_InvalidResponseFromSteam.Format("finalizeResponse?.Response = null");
            }

            if (finalizeResponse.Response.Status != default && finalizeResponse.Response.Status == ISteamAuthenticator.INVALID_ACTIVATION_CODE)
            {
                return state.Error = Strings.Error_InvalidActivationCode;
            }

            // reset our time
            if (finalizeResponse.Response.ServerTime != default)
            {
                var servertime = finalizeResponse.Response.ServerTime * 1000;
                @this.ServerTimeDiff = servertime - IAuthenticatorValueModelBase.CurrentTime;
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
        state.SecretKey = Convert.ToHexString(@this.SecretKey.ThrowIsNull());

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
    public static async Task<(string encryptedPassword64, ulong timestamp)> GetRsaKeyAndEncryptedPasswordAsync(
        this ISteamAuthenticator @this,
        ISteamAuthenticator.IEnrollState state)
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
        ISteamAuthenticator.IEnrollState state,
        string phoneNumber,
        string? countryCode = null)
    {
        state.AccessToken.ThrowIsNull();

        if (!state.RequiresEmailConfirmPhone)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                var userCountryOrRegion = await steamAuthenticatorService.GetUserCountryOrRegion(state.SteamId.ToString());
                countryCode = userCountryOrRegion?.Response?.CountryOrRegion.ThrowIsNull();
            }

            var steamAddPhoneNumberResponse = await steamAuthenticatorService.AddPhoneNumberAsync(state.SteamId.ToString(), phoneNumber, countryCode);
            steamAddPhoneNumberResponse.ThrowIsNull();
            steamAddPhoneNumberResponse.Response.ThrowIsNull();

            if (steamAddPhoneNumberResponse.Response.ConfirmationEmailAddress == null)
            {
                // 账号未绑定邮箱 📫
                return Strings.AccountNotBindEmail; // Error 级别提示消息
            }
            state.EmailDomain = steamAddPhoneNumberResponse.Response.ConfirmationEmailAddress;
            state.PhoneNumber = steamAddPhoneNumberResponse.Response.PhoneNumberFormatted;
            state.RequiresEmailConfirmPhone = true;
        }

        var waitingForEmailConfirmationResponse = await steamAuthenticatorService.AccountWaitingForEmailConfirmation(state.SteamId.ToString());

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
    /// Steam 验证手机号码
    /// </summary>
    /// <param name="steamAuthenticatorService"></param>
    /// <param name="state"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="sms_code"></param>
    /// <returns></returns>
    public static async Task<ApiRspImpl<bool>> VerifyPhoneNumberAsync(
        this ISteamAuthenticatorService steamAuthenticatorService,
        ISteamAuthenticator.IEnrollState state,
        string phoneNumber,
        string? sms_code)
    {
        state.AccessToken.ThrowIsNull();

        var result = await steamAuthenticatorService.VerifyPhoneNumberAsync(state.SteamId.ToString(), phoneNumber, sms_code);

        return result;
    }

    /// <summary>
    /// Steam 移除安全防护
    /// </summary>
    /// <param name="this"></param>
    /// <param name="steam_id"></param>
    /// <param name="scheme">1 = 移除令牌验证器但保留邮箱验证，2 = 移除所有防护</param>
    /// <returns></returns>
    public static async Task<ApiRspImpl<bool>> RemoveAuthenticatorAsync(
        this ISteamAuthenticator @this,
        string steam_id,
        int scheme = 1)
    {
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var result = await steamAuthenticatorService.RemoveAuthenticatorAsync(steam_id, @this.RecoveryCode, scheme.ToString());
        if (result != null && result.Response != null && result.Response.Success)
        {
            // 成功移除安全防护令牌
            return ApiRspHelper.Ok(true);
        }
        else
        {
            // 返回剩余尝试次数提示
            return Strings.RemoveAuthenticatorFail_.Format(result?.Response?.RevocationAttemptsRemaining);
        }
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
        if (result != null)
        {
            // 返回内容正常序列化即表示成功
            return ApiRspHelper.Ok(true);
        }
        else
        {
            return ApiRspCode.NoResponseContent;
        }
    }

    /// <summary>
    /// Steam 替换安全防护令牌
    /// </summary>
    public static async Task<ApiRspImpl<bool>> RemoveAuthenticatorViaChallengeContinueSync(
        this ISteamAuthenticator @this,
        string steam_id,
        string? sms_code,
        bool generate_new_token = true)
    {
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var result = await steamAuthenticatorService.RemoveAuthenticatorViaChallengeContinueSync(steam_id, sms_code, generate_new_token);
        if (result == null)
        {
            return ApiRspCode.NoResponseContent;
        }

        var response = result;
        if (!response.Success || response.ReplacementToken == null)
        {
            return Strings.Error_InvalidResponseFromSteam.Format("ReplacementToken is null.");
        }

        // save data into this authenticator
        @this.SecretKey = response.ReplacementToken.SharedSecret.ToByteArray();
        @this.Serial = response.ReplacementToken.SerialNumber.ToString();
        @this.DeviceId = ISteamAuthenticator.BuildRandomId();
        @this.SteamData = JsonSerializer.Serialize(new SteamConvertSteamDataJsonStruct
        {
            Secret_1 = response.ReplacementToken.Secret1.ToBase64().Base64Encode(),
            Status = response.ReplacementToken.Status,
            ServerTime = unchecked((long)response.ReplacementToken.ServerTime),
            AccountName = response.ReplacementToken.AccountName,
            TokenGid = response.ReplacementToken.TokenGid,
            IdentitySecret = response.ReplacementToken.IdentitySecret.ToBase64().Base64Encode(),
            RevocationCode = response.ReplacementToken.RevocationCode,
            Uri = response.ReplacementToken.Uri,
            SteamId = unchecked((long)response.ReplacementToken.Steamid),
            SerialNumber = response.ReplacementToken.SerialNumber.ToString(),
            SharedSecret = response.ReplacementToken.SharedSecret.ToBase64().Base64Encode(),
            SteamGuardScheme = response.ReplacementToken.SteamguardScheme.ToString(),
        }, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);

        // calculate server drift
        var servertime = unchecked((long)response.ReplacementToken.ServerTime) * 1000;
        @this.ServerTimeDiff = servertime - IAuthenticatorValueModelBase.CurrentTime;
        @this.LastServerTime = DateTime.Now.Ticks;

        return true;
    }
}