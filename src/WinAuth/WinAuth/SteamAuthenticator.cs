/*
 * Copyright (C) 2011 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Exception = System.Exception;

namespace WinAuth.WinAuth;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
[MPObj(keyAsPropertyName: true)]
public sealed partial class SteamAuthenticator : AuthenticatorValueDTO
{
    /// <summary>
    /// 代码中的字符数
    /// </summary>
    const int CODE_DIGITS = 5;

    /// <summary>
    /// KeyUri 的 Steam 发行者
    /// </summary>
    const string STEAM_ISSUER = "Steam";

    /// <summary>
    /// 创建一个新的 Authenticator 对象
    /// </summary>
    [MPConstructor]
    public SteamAuthenticator() : base(CODE_DIGITS)
    {
        Issuer = STEAM_ISSUER;
        Client = GetClient();
        AuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        AccountService = Ioc.Get<ISteamAccountService>();
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.Steam;

    /// <summary>
    /// 返回验证器的序列号
    /// </summary>
    public string? Serial { get; set; }

    /// <summary>
    /// 创建并注册的随机设备 ID
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Steam Json 数据
    /// </summary>
    public string? SteamData { get; set; }

    /// <summary>
    /// 撤销代码
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public string? RecoveryCode => string.IsNullOrEmpty(SteamData)
        ? null
        : SystemTextJsonSerializer.Deserialize(SteamData, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct)
            ?.RevocationCode;

    /// <summary>
    /// 帐户名称
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public string? AccountName => string.IsNullOrEmpty(SteamData)
        ? null
        : SystemTextJsonSerializer.Deserialize(SteamData, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct)?.AccountName;

    /// <summary>
    /// steamId (64 位)
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public long? SteamId64 => string.IsNullOrEmpty(SteamData)
        ? null
        : SystemTextJsonSerializer.Deserialize(SteamData, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct)?.SteamId ?? 0;

    /// <summary>
    /// JSON 会话数据
    /// </summary>
    public string? SessionData { get; set; }

    /// <inheritdoc/>
    protected override bool ExplicitHasValue()
    {
        return base.ExplicitHasValue() && Serial != null && DeviceId != null && SteamData != null &&
               SessionData != null;
    }

    /// <summary>
    /// 如果网络错误，忽略同步的分钟数
    /// </summary>
    const int SYNC_ERROR_MINUTES = 60;

    /// <summary>
    /// 尝试激活的次数
    /// </summary>
    const int ENROLL_ACTIVATE_RETRIES = 30;

    /// <summary>
    /// 激活码不正确
    /// </summary>
    const int INVALID_ACTIVATION_CODE = 89;

    /// <summary>
    /// 验证器代码的字符集
    /// </summary>
    static readonly char[] STEAMCHARS = new char[]
    {
        '2', '3', '4', '5', '6', '7', '8', '9', 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R',
        'T', 'V', 'W', 'X', 'Y',
    };

    /// <summary>
    /// 注册状态
    /// </summary>
    [MPObj(keyAsPropertyName: true)]
    public sealed class EnrollState
    {
        /// <summary>
        /// 语言
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        // public string? CaptchaId { get; set; }
        //
        // public string? CaptchaUrl { get; set; }
        //
        // public string? CaptchaText { get; set; }

        /// <summary>
        /// 电子邮件域
        /// </summary>
        public string? EmailDomain { get; set; }

        /// <summary>
        /// 电子邮件认证文本
        /// </summary>
        public string? EmailAuthText { get; set; }

        /// <summary>
        /// 激活代码
        /// </summary>
        public string? ActivationCode { get; set; }

        /// <summary>
        /// Cookies
        /// </summary>
        [MessagePackFormatter(typeof(CookieFormatter))]
        public CookieContainer? Cookies { get; set; }

        /// <summary>
        /// SteamId
        /// </summary>
        public long SteamId { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 电话号码是否存在
        /// </summary>
        public bool NoPhoneNumber { get; set; }

        /// <summary>
        /// 是否更换身份验证方式
        /// </summary>
        public bool ReplaceAuth { get; set; }

        //public string? OAuthToken { get; set; }

        // public bool RequiresLogin { get; set; }
        //
        // public bool RequiresCaptcha { get; set; }
        //
        // public bool Requires2FA { get; set; }
        //
        // public bool RequiresEmailAuth { get; set; }

        /// <summary>
        /// 需要邮箱确认电话
        /// </summary>
        public bool RequiresEmailConfirmPhone { get; set; }

        /// <summary>
        /// 需要激活
        /// </summary>
        public bool RequiresActivation { get; set; }

        /// <summary>
        /// 撤销代码
        /// </summary>
        public string? RevocationCode { get; set; }

        /// <summary>
        /// 秘密密钥
        /// </summary>
        public string? SecretKey { get; set; }

        /// <summary>
        /// Success
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// 访问令牌
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string? RefreshToken { get; set; }
    }

    #region Authenticator data

    /// <summary>
    /// 上次同步时间错误
    /// </summary>
    static DateTime _lastSyncError = DateTime.MinValue;

    /// <summary>
    /// 当前 Steam 客户端实例
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    [Obsolete("Use AuthenticatorService or AccountService")]
    public SteamClient Client { get; private set; }

    /// <summary>
    /// 当前的 AuthenticatorService 实例
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public ISteamAuthenticatorService AuthenticatorService { get; private set; }

    /// <summary>
    /// 当前 AccountService 实例
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public ISteamAccountService AccountService { get; private set; }

    #endregion

    /// <summary>
    /// 扩展偏移量以在创建第一个代码时重试
    /// </summary>
    readonly int[] ENROLL_OFFSETS = new int[] { 0, -30, 30, -60, 60, -90, 90, -120, 120 };

    /// <summary>
    /// 获取/设置组合的秘密数据值
    /// </summary>
    [MPIgnore, NewtonsoftJsonIgnore, SystemTextJsonIgnore]
    public override string? SecretData
    {
        get
        {
            if (Client != null && Client.Session != null)
                SessionData = Client.Session.ToString();

            //if (Logger != null)
            //{
            //	Logger.Debug("Get Steam data: {0}, Session:{1}", (SteamData ?? string.Empty).Replace("\n"," ").Replace("\r",""), (SessionData ?? string.Empty).Replace("\n", " ").Replace("\r", ""));
            //}

            // this is the key |  serial | deviceid
            Serial.ThrowIsNull();
            DeviceId.ThrowIsNull();
            SteamData.ThrowIsNull();
            return base.SecretData
                   + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(Serial))
                   + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(DeviceId))
                   + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(SteamData))
                   + "|" + (string.IsNullOrEmpty(SessionData) == false
                       ? ByteArrayToString(Encoding.UTF8.GetBytes(SessionData))
                       : string.Empty);
        }

        set
        {
            // extract key + serial + deviceid
            if (string.IsNullOrEmpty(value) == false)
            {
                var parts = value.Split('|');
                base.SecretData = value;
                Serial = parts.Length > 1 ? Encoding.UTF8.GetString(StringToByteArray(parts[1])) : null;
                DeviceId = parts.Length > 2 ? Encoding.UTF8.GetString(StringToByteArray(parts[2])) : null;
                SteamData = parts.Length > 3 ? Encoding.UTF8.GetString(StringToByteArray(parts[3])) : string.Empty;

                if (string.IsNullOrEmpty(SteamData) == false && SteamData[0] != '{')
                    // convert old recovation code into SteamData json
                    SteamData = "{\"revocation_code\":\"" + SteamData + "\"}";
                var session = parts.Length > 4 ? Encoding.UTF8.GetString(StringToByteArray(parts[4])) : null;

                //if (Logger != null)
                //{
                //	Logger.Debug("Set Steam data: {0}, Session:{1}", (SteamData ?? string.Empty).Replace("\n", " ").Replace("\r", ""), (SessionData ?? string.Empty).Replace("\n", " ").Replace("\r", ""));
                //}

                if (string.IsNullOrEmpty(session) == false)
                    SessionData = session;
            }
            else
            {
                SecretKey = null;
                Serial = null;
                DeviceId = null;
                SteamData = null;
                SessionData = null;
            }
        }
    }

    /// <summary>
    /// 获取(或创建)此认证器的当前 Steam 客户端
    /// </summary>
    /// <returns>当前或新的 SteamClient</returns>
    public SteamClient GetClient(string? language = null)
    {
        lock (this)
        {
            Client ??= new SteamClient(this);
            Client.SessionSet(SessionData, language);
            return Client;
        }
    }

    /// <summary>
    /// 获取 Rsa 密钥和加密密码
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="WinAuthInvalidEnrollResponseException"></exception>
    public async Task<(string encryptedPassword, ulong timestamp)> GetRsaKeyAndEncryptedPasswordAsync(
        EnrollState state)
    {
        state.Username.ThrowIsNull();
        state.Password.ThrowIsNull();
        // Steam strips any non-ascii chars from username and password
        state.Username = U0000_U007F_Regex().Replace(state.Username, string.Empty);
        state.Password = U0000_U007F_Regex().Replace(state.Password, string.Empty);

        // get the user's RSA key

        var rsaResponse = await AccountService.GetRSAkeyV2Async(state.Username, state.Password);
        if (rsaResponse?.IsSuccess != true)
            throw new WinAuthInvalidEnrollResponseException(
                $"Cannot get steam information for user: {state.Username}, response: {rsaResponse}");

        return rsaResponse.Content;
    }

    /// <summary>
    /// 调用 Steam 添加令牌接口
    /// </summary>
    /// <param name="state"></param>
    /// <returns>调用成功返回true</returns>
    public async Task<bool> AddAuthenticatorAsync(EnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken)) throw new Exception(Strings.Error_InvalidLoginInfo);
        state.Error = null;
        if (ServerTimeDiff == default)
            await Task.Run(Sync);
        var deviceId = BuildRandomId();

        var response = await AuthenticatorService.AddAuthenticatorAsync(state.SteamId.ToString(), ServerTime.ToString(), deviceId);
        var tfaResponse = response.Content;

        if (tfaResponse?.Response == null)
        {
            state.AccessToken = null;
            state.Cookies = null;
            state.Error = Strings.Error_InvalidResponseFromSteam.Format(tfaResponse);
            return false;
        }

        if (tfaResponse.Response.Status == default || tfaResponse.Response.Status == 84)
        {
            // invalid response
            state.Error = Strings.Error_ITwoFactorService_AddAuthenticator_v0001;
            return false;
        }

        // if (!response.Contains("shared_secret", StringComparison.CurrentCulture))
        // {
        //     // invalid response
        //     state.Error = Strings.error_invalid_response_from_steam_.Format(response);
        //     return false;
        // }

        //账号没有绑定手机号
        switch (tfaResponse.Response.Status)
        {
            case 2:
                //state.Error = Strings.error_steamguard_phoneajax_.Format(Environment.NewLine);
                state.Error = Strings.Error_AccountNotBindTel;
                state.NoPhoneNumber = true;
                return false;
            case 29:
                state.Error = Strings.Error_HasAuthenticator;
                return false;
        }

        state.NoPhoneNumber = false;

        // save data into this authenticator
        var secret = tfaResponse.Response.SharedSecret;
        //SecretKey = Convert.FromBase64String(secret);
        SecretKey = Base64Extensions.Base64DecodeToByteArray_Nullable(secret);
        Serial = tfaResponse.Response.SerialNumber;
        DeviceId = deviceId;
        state.RevocationCode = tfaResponse.Response.RevocationCode;

        // add the steamid into the data
        var steamdata = tfaResponse.Response;
        if (steamdata.SteamId != 0 && state.SteamId != 0)
            steamdata.SteamId = state.SteamId;
        if (steamdata.SteamGuardScheme == string.Empty)
            steamdata.SteamGuardScheme = "2";
        SteamData = SystemTextJsonSerializer.Serialize(steamdata,
            DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);

        // calculate server drift
        var servertime = tfaResponse.Response.ServerTime * 1000;
        ServerTimeDiff = servertime - CurrentTime;
        LastServerTime = DateTime.Now.Ticks;

        state.RequiresActivation = true;

        state.Error = Strings.RequiresActivation;
        return true;
    }

    /// <summary>
    /// 完成添加认证器
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> FinalizeAddAuthenticatorAsync(EnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken)) throw new Exception(Strings.Error_InvalidLoginInfo);
        state.Error = null;

        if (ServerTimeDiff == default)
            await Task.Run(Sync);
        // finalize adding the authenticator

        // try and authorise
        var retries = 0;
        while (state.RequiresActivation == true && retries < ENROLL_ACTIVATE_RETRIES)
        {
            var response = await AuthenticatorService.FinalizeAddAuthenticatorAsync(state.SteamId.ToString(), state.ActivationCode, CalculateCode(false), ServerTime.ToString(), state.AccessToken);
            var finalizeResponse = response.Content;
            finalizeResponse.ThrowIsNull();
            if (finalizeResponse.Response == null)
            {
                state.Error = Strings.Error_InvalidResponseFromSteam.Format(response);
                return false;
            }

            if (finalizeResponse.Response.Status != default && finalizeResponse.Response.Status == INVALID_ACTIVATION_CODE)
            {
                state.Error = Strings.Error_InvalidActivationCode;
                return false;
            }

            // reset our time
            if (string.IsNullOrEmpty(finalizeResponse.Response.ServerTime))
            {
                var servertime = long.Parse(finalizeResponse.Response.ServerTime) * 1000;
                ServerTimeDiff = servertime - CurrentTime;
                LastServerTime = DateTime.Now.Ticks;
            }

            // check success
            if (finalizeResponse.Response.Success == true)
            {
                if (finalizeResponse.Response.WantMore == true)
                {
                    ServerTimeDiff += Period * 1000L;
                    retries++;
                    continue;
                }

                state.RequiresActivation = false;
                break;
            }

            ServerTimeDiff += Period * 1000L;
            retries++;
        }

        if (state.RequiresActivation == true)
        {
            state.Error = Strings.Error_OnActivating;
            return false;
        }

        // mark and successful and return key
        state.Success = true;
        state.SecretKey = ByteArrayToString(SecretKey.ThrowIsNull());

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
    /// 获取用户所在国家
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    public async Task<string?> GetUserCountry(string steam_id)
    {
        var jsonObj = (await AuthenticatorService.GetUserCountry(steam_id)).Content;
        return jsonObj?.Response?.Country;
    }

    /// <summary>
    /// Steam 账户添加绑定手机号
    /// </summary>
    /// <param name="state"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="countryCode"></param>
    /// <returns>返回错误信息，返回为null则执行成功</returns>
    public async Task<string?> AddPhoneNumberAsync(EnrollState state, string phoneNumber, string? countryCode = null)
    {
        state.AccessToken.ThrowIsNull();

        if (!state.RequiresEmailConfirmPhone)
        {
            if (string.IsNullOrEmpty(countryCode))
                countryCode = await GetUserCountry(state.SteamId.ToString());

            var steamAddPhoneNumberResponse = (await AuthenticatorService.AddPhoneNumberAsync(state.SteamId.ToString(), phoneNumber, countryCode)).Content;
            steamAddPhoneNumberResponse.ThrowIsNull();
            steamAddPhoneNumberResponse.Response.ThrowIsNull();

            if (steamAddPhoneNumberResponse.Response.ConfirmationEmailAddress == null)
                return Strings.AccountNotBindEmail;
            state.EmailDomain = steamAddPhoneNumberResponse.Response.ConfirmationEmailAddress;
            state.PhoneNumber = steamAddPhoneNumberResponse.Response.PhoneNumberFormatted;
            state.RequiresEmailConfirmPhone = true;
        }

        var waitingForEmailConfirmationResponse = (await AuthenticatorService.AccountWaitingForEmailConfirmation(state.SteamId.ToString())).Content;

        waitingForEmailConfirmationResponse.ThrowIsNull();
        waitingForEmailConfirmationResponse.Response.ThrowIsNull();

        if (!waitingForEmailConfirmationResponse.Response.AwaitingEmailConfirmation)
        {
            await AuthenticatorService.SendPhoneVerificationCode(state.SteamId.ToString());
            state.RequiresEmailConfirmPhone = false;
            return null;
        }

        return Strings.ConfirmLinkInEmail;
    }

    /// <summary>
    /// Steam 移除安全防护
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="scheme">1 = 移除令牌验证器但保留邮箱验证，2 = 移除所有防护</param>
    /// <returns></returns>
    public async Task<bool> RemoveAuthenticatorAsync(string steam_id, int scheme = 1)
    {
        var jsonObj = (await AuthenticatorService.RemoveAuthenticatorAsync(steam_id, RecoveryCode, scheme.ToString())).Content;
        return jsonObj is { Response.Success: true };
    }

    /// <summary>
    /// 申请 Steam 替换安全防护令牌验证码
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    public async Task<bool> RemoveAuthenticatorViaChallengeStartSync(string steam_id)
    {
        var response = (await AuthenticatorService.RemoveAuthenticatorViaChallengeStartSync(steam_id)).Content;
        return response != null;
    }

    /// <summary>
    /// Steam 替换安全防护令牌
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="sms_code"></param>
    /// <param name="generate_new_token"></param>
    /// <returns></returns>
    public async Task<bool> RemoveAuthenticatorViaChallengeContinueSync(string steam_id, string? sms_code, bool generate_new_token = true)
    {
        var response = (await AuthenticatorService.RemoveAuthenticatorViaChallengeContinueSync(steam_id, sms_code, generate_new_token)).Content.ThrowIsNull();

        if (!response.Success || response.ReplacementToken == null)
        {
            Strings.Error_InvalidResponseFromSteam.Format(response);
            return false;
        }

        // save data into this authenticator
        SecretKey = response.ReplacementToken.SharedSecret.ToByteArray();
        Serial = response.ReplacementToken.SerialNumber.ToString();
        DeviceId = BuildRandomId();
        SteamData = SystemTextJsonSerializer.Serialize(new SteamConvertSteamDataJsonStruct
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
        ServerTimeDiff = servertime - CurrentTime;
        LastServerTime = DateTime.Now.Ticks;

        return true;
    }

    /// <summary>
    /// 向服务器注册身份验证器
    /// </summary>
    [Obsolete("use AddAuthenticatorAsync")]
    public async Task EnrollAsync(EnrollState state)
    {
        // clear error
        state.Error = null;
        try
        {
            var data = new NameValueCollection();
            //await CheckCookiesAsync(state);
            //string response;

            // if (string.IsNullOrEmpty(state.OAuthToken) == true)
            // {
            //     var (rsaResponse, encryptedPassword) = await GetRsaKeyAndEncryptedPasswordAsync(state);
            //
            //     // var nameValueCollection = new NameValueCollection();
            //     // nameValueCollection.Add("account_name", state.Username);
            //     // nameValueCollection.Add("persistence", "0");
            //     // nameValueCollection.Add("website_id", "Client");
            //     // nameValueCollection.Add("guard_data", null);
            //     // nameValueCollection.Add("language", null);
            //     // nameValueCollection.Add("encrypted_password", encryptedPassword);
            //     // nameValueCollection.Add("encryption_timestamp", rsaResponse.TimeStamp);
            //     //
            //     // SteamLoginDeviceDetails steamLoginDeviceDetails = new SteamLoginDeviceDetails()
            //     // {
            //     //     device_friendly_name = $"{Environment.MachineName} (WinAuth)",
            //     //     platform_type = EAuthTokenPlatformType.k_EAuthTokenPlatformType_MobileApp,
            //     //     os_type = (int)EOSType.Android9,
            //     // };
            //     // using (MemoryStream stream = new MemoryStream())
            //     // {
            //     //     ProtoBuf.Serializer.Serialize(stream, steamLoginDeviceDetails);
            //     //     var device = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
            //     //     nameValueCollection.Add("device_details", device);
            //     // }
            //
            //     // login request
            //     data = new NameValueCollection
            //     {
            //         { "password", encryptedPassword },
            //         { "username", state.Username },
            //         { "twofactorcode", "" },
            //         { "emailauth", state.EmailAuthText ?? string.Empty },
            //         { "loginfriendlyname", "" },
            //         { "captchagid", state.CaptchaId ?? "-1" },
            //         { "captcha_text", state.CaptchaText ?? "" },
            //         { "emailsteamid", state.EmailAuthText != null ? state.SteamId ?? string.Empty : string.Empty },
            //         { "rsatimestamp", rsaResponse.TimeStamp },
            //         { "remember_login", "false" },
            //         { "oauth_client_id", "DE45CD61" },
            //         { "oauth_scope", "read_profile write_profile read_client write_client" },
            //         { "donotache", donotache_value },
            //     };
            //     const string url_login_dologin = "/login/dologin/";
            //     response = await RequestAsync(COMMUNITY_BASE + url_login_dologin, "POST", data, state.Cookies);
            //     // response = await RequestAsync(
            //     //     "https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaCredentials/v1/", "POST",
            //     //     nameValueCollection, state.Cookies);
            //
            //     // nameValueCollection.Clear();
            //     // nameValueCollection.Add("client_id", "10078727937720195440");
            //     // nameValueCollection.Add("steamid", "76561198387775567");
            //     // nameValueCollection.Add("code", "NP6YM");
            //     // nameValueCollection.Add("code_type", "2");
            //
            //     // response = await RequestAsync(
            //     //     "https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1/",
            //     //     "POST", nameValueCollection, state.Cookies);
            //
            //     if (response.Contains("\"captcha_gid\":-1"))
            //     {
            //         state.Error = Strings.error_password;
            //         return false;
            //     }
            //     if (response.Contains("transfer"))
            //     {
            //         var jsonObj = SystemTextJsonSerializer.Deserialize(response,
            //             DefaultJsonSerializerContext_.Default.SteamMobileDologinJsonStruct);
            //         jsonObj.ThrowIsNull();
            //         jsonObj.TransferParameters.ThrowIsNull();
            //         state.Requires2FA = jsonObj.RequiresTwofactor;
            //         if (!jsonObj.LoginComplete) throw new WinAuthInvalidEnrollResponseException("Login Failed");
            //         state.SteamId = jsonObj.TransferParameters.Steamid;
            //         state.OAuthToken = jsonObj.TransferParameters.Auth;
            //     }
            //     else
            //     {
            //         var loginResponse = SystemTextJsonSerializer.Deserialize<SteamDoLoginJsonStruct>(response,
            //             DefaultJsonSerializerContext_.Default.SteamDoLoginJsonStruct);
            //         loginResponse.ThrowIsNull();
            //
            //         //if (loginresponse == null)
            //         //    throw GetWinAuthInvalidEnrollResponseException(response, url_login_dologin, new ArgumentNullException(nameof(loginresponse)));
            //
            //         if (loginResponse.EmailSteamId != string.Empty)
            //             state.SteamId = loginResponse.EmailSteamId;
            //
            //         // require captcha
            //         if (loginResponse.CaptchaNeeded == true)
            //         {
            //             state.RequiresCaptcha = true;
            //             state.CaptchaId = loginResponse.CaptchaGId;
            //             state.CaptchaUrl = COMMUNITY_BASE + "/public/captcha.php?gid=" + state.CaptchaId;
            //
            //             state.Error = Strings.CaptchaNeeded;
            //             return false;
            //         }
            //         else
            //         {
            //             state.RequiresCaptcha = false;
            //             state.CaptchaId = null;
            //             state.CaptchaUrl = null;
            //             state.CaptchaText = null;
            //         }
            //
            //         // require email auth
            //         if (loginResponse.EmailAuthNeeded == true)
            //         {
            //             if (!string.IsNullOrEmpty(loginResponse.EmailDomain))
            //             {
            //                 var emaildomain = loginResponse.EmailDomain;
            //                 if (string.IsNullOrEmpty(emaildomain) == false)
            //                     state.EmailDomain = emaildomain;
            //             }
            //
            //             state.RequiresEmailAuth = true;
            //
            //             state.Error = Strings.EmailAuthNeeded;
            //             return false;
            //         }
            //         else
            //         {
            //             state.EmailDomain = null;
            //             state.RequiresEmailAuth = false;
            //         }
            //
            //         // require 2fa auth
            //         if (loginResponse.RequiresTwoFactor == true)
            //             state.Requires2FA = true;
            //         else
            //             state.Requires2FA = false;
            //
            //         // if we didn't login, return the result
            //         if (loginResponse.LoginComplete == false || loginResponse.OAuth == null)
            //         {
            //             if (loginResponse.OAuth == null)
            //                 state.Error = Strings.error_NoOAuth;
            //             if (!string.IsNullOrEmpty(loginResponse.Message))
            //                 state.Error = loginResponse.Message;
            //             return false;
            //         }
            //
            //         // get the OAuth token - is stringified json
            //         //loginresponse.Oauth.Steamid.ThrowIsNull();
            //         var oauthjson = SystemTextJsonSerializer.Deserialize<SteamDoLoginOauthJsonStruct>(loginResponse.OAuth,
            //             DefaultJsonSerializerContext_.Default.SteamDoLoginOauthJsonStruct);
            //         oauthjson.ThrowIsNull();
            //         state.OAuthToken = oauthjson.OAuthToken;
            //         if (oauthjson.SteamId != string.Empty)
            //         {
            //             state.SteamId = oauthjson.SteamId;
            //         }
            //     }
            // }

            //// login to webapi
            //data.Clear();
            //data.Add("access_token", state.OAuthToken);
            //response = await RequestAsync(WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logon/v0001", "POST", data);
            //var sessionid = cookies.GetCookies(new Uri(COMMUNITY_BASE + "/"))?["sessionid"]?.Value;

            // 获取Sessionid
            // var readableCookies = cookies.GetCookies(new Uri("https://steamcommunity.com"));
            // var sessionid = readableCookies["sessionid"]?.Value;

            if (state.RequiresActivation == false)
            {
                // data.Clear();
                // data.Add("op", "has_phone");
                // data.Add("arg", "null");
                // data.Add("sessionid", sessionid);
                //
                // response = await RequestAsync(COMMUNITY_BASE + "/steamguard/phoneajax", "POST", data, cookies);
                // var jsonresponse = SystemTextJsonSerializer.Deserialize<SteamDoLoginHasPhoneJsonStruct>(response, DefaultJsonSerializerContext_.Default.SteamDoLoginHasPhoneJsonStruct);
                // jsonresponse.ThrowIsNull();
                // var hasPhone = jsonresponse.HasPhone;
                // if (hasPhone == false)
                // {
                //     state.OAuthToken = null; // force new login
                //     state.RequiresLogin = true;
                //     state.Cookies = null;
                //     state.Error = Strings.error_steamguard_phoneajax_.Format(Environment.NewLine);
                //     return false;
                // }

                //response = Request(COMMUNITY_BASE + "/steamguard/phone_checksms?bForTwoFactor=1&bRevoke2fOnCancel=", "GET", null, cookies);
                // add a new authenticator
            }
        }
        catch (WinAuthUnauthorisedRequestException ex)
        {
            throw new WinAuthInvalidEnrollResponseException(
                "You are not allowed to add an authenticator. Have you enabled 'community-generated content' in Family View?",
                ex);
        }
        catch (WinAuthInvalidRequestException ex)
        {
            throw new WinAuthInvalidEnrollResponseException("Error enrolling new authenticator", ex);
        }

        //static JsonNode SelectTokenNotNull(string response, JsonNode token, string path, string? msg = null) =>
        //    SteamClient.Utils.SelectTokenNotNull(response, token, path, msg,
        //        GetWinAuthInvalidEnrollResponseException);

        //static string SelectTokenValueNotNull(string response, JsonNode token, string path, string? msg = null) =>
        //    SteamClient.Utils.SelectTokenValueNotNull(response, token, path, msg,
        //        GetWinAuthInvalidEnrollResponseException);
    }

    /// <summary>
    /// 与 Steam 同步验证者的时间
    /// </summary>
    public override async void Sync()
    {
        // check if data is protected
        if (SecretKey == null && EncryptedData != null)
            throw new WinAuthEncryptedSecretDataException();

        // don't retry for 5 minutes
        if (_lastSyncError >= DateTime.Now.AddMinutes(0 - SYNC_ERROR_MINUTES))
            return;

        try
        {
            var json = (await AuthenticatorService.TwoFAQueryTime()).Content;
            json.ThrowIsNull();
            json.Response.ThrowIsNull();

            // get servertime in ms
            long servertime = long.Parse(json.Response.ServerTime) * 1000;

            // get the difference between the server time and our current time
            ServerTimeDiff = servertime - CurrentTime;
            LastServerTime = DateTime.Now.Ticks;

            // clear any sync error
            _lastSyncError = DateTime.MinValue;
        }
        catch
        {
            // don't retry for a while after error
            _lastSyncError = DateTime.Now;
            //throw;
            // set to zero to force reset
            //ServerTimeDiff = 0;
        }
    }

    /// <summary>
    /// 计算验证器的当前代码
    /// </summary>
    /// <param name="resyncTime">重新同步时间标志</param>
    /// <param name="interval"></param>
    /// <returns>身份验证代码</returns>
    protected override string CalculateCode(bool resyncTime = false, long interval = -1)
    {
        // sync time if required
        if (resyncTime || ServerTimeDiff == 0)
            if (interval > 0)
                ServerTimeDiff = (interval * Period * 1000L) - CurrentTime;
            else
                Task.Run(Sync);

        var hmac = new HMac(new Sha1Digest());
        hmac.Init(new KeyParameter(SecretKey));

        var codeIntervalArray = BitConverter.GetBytes(CodeInterval);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(codeIntervalArray);
        hmac.BlockUpdate(codeIntervalArray, 0, codeIntervalArray.Length);

        var mac = new byte[hmac.GetMacSize()];
        hmac.DoFinal(mac, 0);

        // the last 4 bits of the mac say where the code starts (e.g. if last 4 bit are 1100, we start at byte 12)
        var start = mac[19] & 0x0f;

        // extract those 4 bytes
        var bytes = new byte[4];
        Array.Copy(mac, start, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        var fullcode = BitConverter.ToUInt32(bytes, 0) & 0x7fffffff;

        // build the alphanumeric code
        var code = new StringBuilder();
        for (var i = 0; i < CODE_DIGITS; i++)
        {
            code.Append(STEAMCHARS[fullcode % STEAMCHARS.Length]);
            fullcode /= (uint)STEAMCHARS.Length;
        }

        return code.ToString();
    }

    /// <summary>
    /// 为注册创建一个随机的设备 ID 字符串
    /// </summary>
    /// <returns>随机字符串</returns>
    static string BuildRandomId() => "android:" + Guid.NewGuid().ToString();

    SteamConvertSteamDataJsonStruct? SteamDataDeserialize()
    {
        if (string.IsNullOrEmpty(SteamData)) return null;
        var data = SystemTextJsonSerializer.Deserialize(SteamData, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);
        return data;
    }

    /// <summary>
    /// 记录来自请求的异常
    /// </summary>
    /// <param name="method">Get 或 POST</param>
    /// <param name="url">请求 URL</param>
    /// <param name="cookies">cookie 容器</param>
    /// <param name="request">请求数据</param>
    /// <param name="ex">抛出异常</param>
    [Conditional("DEBUG")]
    static void LogException(string? method, string url, CookieContainer? cookies, NameValueCollection? request,
        Exception ex)
    {
        var data = new StringBuilder();
        if (cookies != null)
        {
            IEnumerable<Cookie> cookies_ = cookies.GetCookies(new Uri(url));
            foreach (Cookie cookie in cookies_)
            {
                if (data.Length == 0)
                    data.Append("Cookies:");
                else
                    data.Append('&');
                data.Append(cookie.Name + "=" + cookie.Value);
            }

            data.Append(' ');
        }

        if (request != null)
        {
            foreach (var key in request.AllKeys)
            {
                if (data.Length == 0)
                    data.Append("Req:");
                else
                    data.Append('&');
                data.Append(key + "=" + request[key]);
            }

            data.Append(' ');
        }

        Log.Error(nameof(WinAuth), ex, data.ToString());
    }

    /// <summary>
    /// 记录一个正常的响应
    /// </summary>
    /// <param name="method">Get 或 POST</param>
    /// <param name="url">请求 URL</param>
    /// <param name="cookies">cookie 容器</param>
    /// <param name="request">请求数据</param>
    /// <param name="response">response body</param>
    [Conditional("DEBUG")]
    static void LogRequest(string? method, string url, CookieContainer? cookies, NameValueCollection? request,
        string? response)
    {
        var data = new StringBuilder();
        if (cookies != null)
        {
            IEnumerable<Cookie> cookies_ = cookies.GetCookies(new Uri(url));
            foreach (Cookie cookie in cookies_)
            {
                if (data.Length == 0)
                    data.Append("Cookies:");
                else
                    data.Append('&');
                data.Append(cookie.Name + "=" + cookie.Value);
            }

            data.Append(' ');
        }

        if (request != null)
        {
            foreach (var key in request.AllKeys)
            {
                if (data.Length == 0)
                    data.Append("Req:");
                else
                    data.Append('&');
                data.Append(key + "=" + request[key]);
            }

            data.Append(' ');
        }

        if (response != null)
        {
            data.AppendLine();
            data.Append(response);
        }

        Log.Info(nameof(WinAuth), data.ToString());
    }

    [GeneratedRegex("[^\\u0000-\\u007F]")]
    internal static partial Regex U0000_U007F_Regex();
}