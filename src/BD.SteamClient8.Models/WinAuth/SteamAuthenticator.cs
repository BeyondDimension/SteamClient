namespace BD.SteamClient8.Models.WinAuth;

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
    public const int ENROLL_ACTIVATE_RETRIES = 30;

    /// <summary>
    /// 激活码不正确
    /// </summary>
    public const int INVALID_ACTIVATION_CODE = 89;

    /// <summary>
    /// 验证器代码的字符集
    /// </summary>
    static readonly char[] STEAMCHARS =
    [
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        'B',
        'C',
        'D',
        'F',
        'G',
        'H',
        'J',
        'K',
        'M',
        'N',
        'P',
        'Q',
        'R',
        'T',
        'V',
        'W',
        'X',
        'Y',
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

    #endregion Authenticator data

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

        var steamAuthenticatorService = Ioc.Get<IAuthenticatorNetService>();
        try
        {
            var steamSyncStruct = (await steamAuthenticatorService.TwoFAQueryTime()).Content;
            steamSyncStruct.ThrowIsNull();
            steamSyncStruct.Response.ThrowIsNull();

            // get servertime in ms
            long servertime = long.Parse(steamSyncStruct.Response.ServerTime) * 1000;

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
    public override string CalculateCode(bool resyncTime = false, long interval = -1)
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
    public static string BuildRandomId() => "android:" + Guid.NewGuid().ToString();

    SteamConvertSteamDataJsonStruct? SteamDataDeserialize()
    {
        if (string.IsNullOrEmpty(SteamData)) return null;
        var data = SystemTextJsonSerializer.Deserialize(SteamData, DefaultJsonSerializerContext_.Default.SteamConvertSteamDataJsonStruct);
        return data;
    }
}