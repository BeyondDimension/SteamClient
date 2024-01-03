using Strings = BD.SteamClient8.Resources.Strings;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 身份验证令牌
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[MessagePackObject(keyAsPropertyName: true)]
public sealed partial class SteamAuthenticator : AuthenticatorValueModel
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
    }

    /// <inheritdoc/>
    public override string? Issuer { get => STEAM_ISSUER; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
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
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public string? RecoveryCode => string.IsNullOrEmpty(SteamData)
        ? null
        : SystemTextJsonSerializer.Deserialize(SteamData, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct)
            ?.RevocationCode;

    /// <summary>
    /// 帐户名称
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public string? AccountName => string.IsNullOrEmpty(SteamData)
        ? null
        : SystemTextJsonSerializer.Deserialize(SteamData, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct)?.AccountName;

    /// <summary>
    /// steamId (64 位)
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
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
    static readonly char[] STEAMCHARS =
    [
        '2', '3', '4', '5', '6', '7', '8', '9', 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R',
        'T', 'V', 'W', 'X', 'Y',
    ];

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

        string? _Username;

        /// <summary>
        /// 用户名
        /// </summary>
        public string? Username
        {
            get => _Username;
            set => _Username = value == null ? null :
                SteamLoginState.SteamUNPWDRegex().Replace(value, string.Empty);
        }

        string? _Password;

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password
        {
            get => _Password;
            set => _Password = value == null ? null :
                SteamLoginState.SteamUNPWDRegex().Replace(value, string.Empty);
        }

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

    #endregion

    /// <summary>
    /// 扩展偏移量以在创建第一个代码时重试
    /// </summary>
    readonly int[] ENROLL_OFFSETS = [0, -30, 30, -60, 60, -90, 90, -120, 120];

    /// <summary>
    /// 获取/设置组合的秘密数据值
    /// </summary>
    [MPIgnore, NewtonsoftJsonIgnore, SystemTextJsonIgnore]
    public override string? SecretData
    {
        get
        {
            //if (Client != null && Client.Session != null)
            //    SessionData = Client.Session.ToString();

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

    ///// <summary>
    ///// 获取(或创建)此认证器的当前 Steam 客户端
    ///// </summary>
    ///// <returns>当前或新的 SteamClient</returns>
    //public SteamClient GetClient(string? language = null)
    //{
    //    lock (this)
    //    {
    //        Client ??= new SteamClient(this);
    //        Client.SessionSet(SessionData, language);
    //        return Client;
    //    }
    //}

    /// <summary>
    /// 获取 Rsa 密钥和加密密码
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public async Task<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>> GetRsaKeyAndEncryptedPasswordAsync(
        EnrollState state)
    {
        state.Username.ThrowIsNull();
        state.Password.ThrowIsNull();

        // get the user's RSA key

        var steamAccountService = Ioc.Get<ISteamAccountService>();
        var rsaResponse = await steamAccountService.GetRSAkeyV2Async(state.Username, state.Password);

        return rsaResponse;
    }

    /// <summary>
    /// 调用 Steam 添加令牌接口
    /// </summary>
    /// <param name="state"></param>
    /// <returns>调用成功返回true</returns>
    public async Task<ApiRspImpl<bool>> AddAuthenticatorAsync(EnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken))
            return Strings.Error_InvalidLoginInfo;

        state.Error = null;
        if (ServerTimeDiff == default)
            await Task.Run(Sync);
        var deviceId = BuildRandomId();

        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var response = await steamAuthenticatorService.AddAuthenticatorAsync(state.SteamId.ToString(), ServerTime.ToString(), deviceId);

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
    public async Task<ApiRspImpl<bool>> FinalizeAddAuthenticatorAsync(EnrollState state)
    {
        if (string.IsNullOrEmpty(state.AccessToken))
            return Strings.Error_InvalidLoginInfo;

        state.Error = null;

        if (ServerTimeDiff == default)
            await Task.Run(Sync);
        // finalize adding the authenticator

        // try and authorise
        var retries = 0;
        while (state.RequiresActivation == true && retries < ENROLL_ACTIVATE_RETRIES)
        {
            var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
            var response = await steamAuthenticatorService.FinalizeAddAuthenticatorAsync(state.SteamId.ToString(), state.ActivationCode, CalculateCode(false), ServerTime.ToString(), state.AccessToken);
            var finalizeResponse = response.Content;
            finalizeResponse.ThrowIsNull();
            if (finalizeResponse.Response == null)
            {
                return state.Error = Strings.Error_InvalidResponseFromSteam.Format(response);
            }

            if (finalizeResponse.Response.Status != default && finalizeResponse.Response.Status == INVALID_ACTIVATION_CODE)
            {
                return state.Error = Strings.Error_InvalidActivationCode;
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
            return state.Error = Strings.Error_OnActivating;
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
    /// 获取用户所在国家或地区
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    public static async Task<ApiRspImpl<GetUserCountryOrRegionResponse?>> GetUserCountryOrRegion(string steam_id)
    {
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var result = await steamAuthenticatorService.GetUserCountryOrRegion(steam_id);
        return result;
    }

    /// <summary>
    /// Steam 账户添加绑定手机号
    /// </summary>
    /// <param name="state"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="countryCode"></param>
    /// <returns>返回错误信息，isOK 标识执行成功，infoMessage 为是否需要显示提示信息</returns>
    public static async Task<ApiRspImpl<(bool isOK, string? infoMessage)>> AddPhoneNumberAsync(EnrollState state, string phoneNumber, string? countryCode = null)
    {
        state.AccessToken.ThrowIsNull();

        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        if (!state.RequiresEmailConfirmPhone)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                var userCountryOrRegion = await GetUserCountryOrRegion(state.SteamId.ToString());
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
    /// <param name="steam_id"></param>
    /// <param name="scheme">1 = 移除令牌验证器但保留邮箱验证，2 = 移除所有防护</param>
    /// <returns></returns>
    public async Task<ApiRspImpl<bool>> RemoveAuthenticatorAsync(string steam_id, int scheme = 1)
    {
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        var result = await steamAuthenticatorService.RemoveAuthenticatorAsync(steam_id, RecoveryCode, scheme.ToString());
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
    /// <param name="steam_id"></param>
    /// <returns></returns>
    public static async Task<ApiRspImpl<bool>> RemoveAuthenticatorViaChallengeStartSync(string steam_id)
    {
        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
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
    /// <param name="steam_id"></param>
    /// <param name="sms_code"></param>
    /// <param name="generate_new_token"></param>
    /// <returns></returns>
    public async Task<ApiRspImpl<bool>> RemoveAuthenticatorViaChallengeContinueSync(string steam_id, string? sms_code, bool generate_new_token = true)
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
    /// 与 Steam 同步验证者的时间
    /// </summary>
    public override async void Sync()
    {
        // check if data is protected
        if (SecretKey == null && EncryptedData != null)
            throw new AuthenticatorEncryptedSecretDataException();

        // don't retry for 5 minutes
        if (_lastSyncError >= DateTime.Now.AddMinutes(0 - SYNC_ERROR_MINUTES))
            return;

        var steamAuthenticatorService = Ioc.Get<ISteamAuthenticatorService>();
        try
        {
            var json = (await steamAuthenticatorService.TwoFAQueryTime()).Content;
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
}