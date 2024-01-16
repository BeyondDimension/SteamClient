#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[MessagePackObject(keyAsPropertyName: true)]
public sealed partial class BattleNetAuthenticator : AuthenticatorValueModel
{
    /// <summary>
    /// 正则表达式源生成
    /// </summary>
    [GeneratedRegex(".*\"country\":\"([^\"]*)\".*", RegexOptions.IgnoreCase)]
    private static partial Regex CountryRegex();

    /// <summary>
    /// 代码中的位数
    /// </summary>
    const int CODE_DIGITS = 8;

    const string BATTLENET_ISSUER = "BattleNet";

    /// <summary>
    /// 创建一个新的 <see cref="BattleNetAuthenticator"/> 对象
    /// </summary>
    [SerializationConstructor]
    public BattleNetAuthenticator() : base(CODE_DIGITS)
    {
    }

    /// <inheritdoc/>
    public override string? Issuer { get => BATTLENET_ISSUER; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.BattleNet;

    /// <summary>
    /// 获取或设置序列号
    /// </summary>
    public string? Serial { get; set; }

    /// <summary>
    /// 获取或设置恢复码是否已验证
    /// </summary>
    public bool RestoreCodeVerified { get; set; }

    /// <inheritdoc/>
    protected override bool ExplicitHasValue()
    {
        return base.ExplicitHasValue() && Serial != null;
    }

    /// <summary>
    /// 模型管柱尺寸
    /// </summary>
    const int MODEL_SIZE = 16;

    /// <summary>
    /// 我们在随机模型字符串中使用的可能字符的字符串
    /// </summary>
    const string MODEL_CHARS = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890";

    /// <summary>
    /// Http 响应中使用的缓冲区大小
    /// </summary>
    const int RESPONSE_BUFFER_SIZE = 64;

    /// <summary>
    /// 期望从登记返回数据的大小
    /// </summary>
    const int ENROLL_RESPONSE_SIZE = 45;

    /// <summary>
    /// 时间同步返回数据的预期大小
    /// </summary>
    const int SYNC_RESPONSE_SIZE = 8;

    /// <summary>
    /// 恢复调用时使用的缓冲区大小
    /// </summary>
    const int RESTOREINIT_BUFFER_SIZE = 32;

    /// <summary>
    /// 用于恢复验证调用的缓冲区大小
    /// </summary>
    const int RESTOREVALIDATE_BUFFER_SIZE = 20;

    /// <summary>
    /// 用于加密数据的公钥模数
    /// </summary>
    const string ENROLL_MODULUS =
      "955e4bd989f3917d2f15544a7e0504eb9d7bb66b6f8a2fe470e453c779200e5e" +
      "3ad2e43a02d06c4adbd8d328f1a426b83658e88bfd949b2af4eaf30054673a14" +
      "19a250fa4cc1278d12855b5b25818d162c6e6ee2ab4a350d401d78f6ddb99711" +
      "e72626b48bd8b5b0b7f3acf9ea3c9e0005fee59e19136cdb7c83f2ab8b0a2a99";

    /// <summary>
    /// 用于加密数据的公钥指数
    /// </summary>
    const string ENROLL_EXPONENT =
      "0101";

    /// <summary>
    /// 如果网络错误，忽略同步的分钟数
    /// </summary>
    const int SYNC_ERROR_MINUTES = 5;

    /// <summary>
    /// 所有移动服务的 url
    /// </summary>
    const string REGION_US = "US";
    const string REGION_EU = "EU";
    const string REGION_KR = "KR";
    const string REGION_CN = "CN";

    /// <summary>
    /// 套欧盟国家 ISO3166
    /// </summary>
    static readonly string[] EU_COUNTRIES =
    [
        "AL",
        "AD",
        "AM",
        "AT",
        "AZ",
        "BY",
        "BE",
        "BA",
        "BG",
        "HR",
        "CY",
        "CZ",
        "DK",
        "EE",
        "FI",
        "FR",
        "GE",
        "DE",
        "GR",
        "HU",
        "IS",
        "IE",
        "IT",
        "KV",
        "XK",
        "LV",
        "LI",
        "LT",
        "LU",
        "MK",
        "MT",
        "MD",
        "MC",
        "ME",
        "NL",
        "NO",
        "PL",
        "PT",
        "RO",
        "RU",
        "SM",
        "RS",
        "SK",
        "ES",
        "SE",
        "CH",
        "TR",
        "UA",
        "UK",
        "GB",
        "VA",
    ];

    /// <summary>
    /// 一套 ISO3166 KR 国家/地区
    /// </summary>
    static readonly string[] KR_COUNTRIES =
    [
        "KR",
        "KP",
        "TW",
        "HK",
        "MO",
    ];

    /// <summary>
    /// 上次同步错误的时间
    /// </summary>
    static DateTime _lastSyncError = DateTime.MinValue;

    /// <summary>
    /// 向服务器注册验证器
    /// </summary>
    public async void EnrollAsync()
    {
        // default to US
        var region = REGION_US;
        var country = REGION_US;

        // Battle.net does a GEO IP lookup anyway so there is no need to pass the region
        // however China has its own URL so we must still do our own GEO IP lookup to find the country

        string? responseString = null;
        try
        {
            var georesponse = await IAuthenticatorNetService.Instance.GEOIP();
            responseString = georesponse?.Content;
        }
        catch (Exception)
        {
        }
        if (string.IsNullOrEmpty(responseString) == false)
        {
            // not worth a full json parser, just regex it
            //var match = Regex.Match(responseString, ".*\"country\":\"([^\"]*)\".*", RegexOptions.IgnoreCase);
            var countryregex = CountryRegex();
            var match = countryregex.Match(responseString);
            if (match.Success == true)
            {
                // match the correct region
                country = match.Groups[1].Value.ToUpper();

                if (EU_COUNTRIES.Contains(country) == true)
                {
                    region = REGION_EU;
                }
                else if (KR_COUNTRIES.Contains(country) == true)
                {
                    region = REGION_KR;
                }
                else if (country == REGION_CN)
                {
                    region = REGION_CN;
                }
                else
                {
                    region = REGION_US;
                }
            }
        }

        // allow override of country for CN using US from app.config
        //System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
        //try
        //{
        //	string configcountry = config.GetValue("BattleNetAuthenticator.Country", typeof(string)) as string;
        //	if (string.IsNullOrEmpty(configcountry) == false)
        //	{
        //		country = configcountry;
        //	}
        //}
        //catch (InvalidOperationException ) { }
        //try
        //{
        //	string configregion = config.GetValue("BattleNetAuthenticator.Region", typeof(string)) as string;
        //	if (string.IsNullOrEmpty(configregion) == false)
        //	{
        //		region = configregion;
        //	}
        //}
        //catch (InvalidOperationException ) {}

        // generate byte array of data:
        //  00 byte[20] one-time key used to decrypt data when returned;
        //  20 byte[2] country code, e.g. US, GB, FR, KR, etc
        //  22 byte[16] model string for this device;
        //	38 END
        var data = new byte[38];
        var oneTimePad = CreateOneTimePad(20);
        Array.Copy(oneTimePad, data, oneTimePad.Length);
        // add country
        var countrydata = Encoding.UTF8.GetBytes(country);
        Array.Copy(countrydata, 0, data, 20, Math.Min(countrydata.Length, 2));
        // add model name
        var model = Encoding.UTF8.GetBytes(GeneralRandomModel());
        Array.Copy(model, 0, data, 22, Math.Min(model.Length, 16));

        // encrypt the data with BMA public key
        var rsa = new RsaEngine();
        rsa.Init(true, new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(ENROLL_MODULUS, 16), new Org.BouncyCastle.Math.BigInteger(ENROLL_EXPONENT, 16)));
        var encrypted = rsa.ProcessBlock(data, 0, data.Length);

        // call the enroll server
        byte[]? responseData = null;
        try
        {
            var response = await IAuthenticatorNetService.Instance.EnRoll(region, encrypted);
            responseData = response.Content;
            responseData.ThrowIsNull();

            // check it is correct size
            if (responseData.Length != ENROLL_RESPONSE_SIZE)
            {
                throw new AuthenticatorInvalidEnrollResponseException(string.Format("Invalid response data size (expected 45 got {0})", responseData.Length));
            }
        }
        catch (Exception ex)
        {
            throw new AuthenticatorInvalidEnrollResponseException("Cannot contact Battle.net servers.", ex);
        }
        // return data:
        // 00-07 server time (Big Endian)
        // 08-24 serial number (17)
        // 25-44 secret key encrpyted with our pad
        // 45 END

        // extract the server time
        var serverTime = new byte[8];
        Array.Copy(responseData, serverTime, 8);
        if (BitConverter.IsLittleEndian == true)
        {
            Array.Reverse(serverTime);
        }
        // get the difference between the server time and our current time
        ServerTimeDiff = BitConverter.ToInt64(serverTime, 0) - CurrentTime;

        // get the secret key
        var secretKey = new byte[20];
        Array.Copy(responseData, 25, secretKey, 0, 20);
        // decrypt the initdata with a simple xor with our key
        for (var i = oneTimePad.Length - 1; i >= 0; i--)
        {
            secretKey[i] ^= oneTimePad[i];
        }
        SecretKey = secretKey;

        // get the serial number
        Serial = Encoding.Default.GetString(responseData, 8, 17);
    }

#if DEBUG
    /// <summary>
    /// Debug version of enroll that just returns a known test authenticator
    /// </summary>
    /// <param name="testmode"></param>
    public void Enroll(bool testmode)
    {
        if (!testmode)
        {
            EnrollAsync();
        }
        else
        {
            ServerTimeDiff = 0;
            SecretKey = [0x7B, 0x0B, 0xFA, 0x82, 0x30, 0xE5, 0x44, 0x24, 0xAB, 0x51, 0x77, 0x7D, 0xAD, 0xBF, 0xD5, 0x37, 0x41, 0x43, 0xE3, 0xB0];
            Serial = "US-1306-2525-4376"; // Restore: CR24KPKF51
        }
    }
#endif

    /// <summary>
    /// Synchronise this authenticator's time with server time. We update our data record with the difference from our UTC time.
    /// </summary>
    public override async void Sync()
    {
        // check if data is protected
        if (SecretKey == null && EncryptedData != null)
        {
            throw new AuthenticatorEncryptedSecretDataException();
        }

        // don't retry for 5 minutes
        if (_lastSyncError >= DateTime.Now.AddMinutes(0 - SYNC_ERROR_MINUTES))
        {
            return;
        }

        try
        {
            // create a connection to time sync server
            // get response
            byte[]? responseData = null;
            var response = await IAuthenticatorNetService.Instance.Sync(Region);
            responseData = response.Content;
            responseData.ThrowIsNull();

            // check it is correct size
            if (responseData.Length != SYNC_RESPONSE_SIZE)
            {
                throw new AuthenticatorInvalidSyncResponseException(string.Format("Invalid response data size (expected " + SYNC_RESPONSE_SIZE + " got {0}", responseData.Length));
            }

            // return data:
            // 00-07 server time (Big Endian)

            // extract the server time
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(responseData);
            }
            // get the difference between the server time and our current time
            var serverTimeDiff = BitConverter.ToInt64(responseData, 0) - CurrentTime;

            // update the Data object
            ServerTimeDiff = serverTimeDiff;
            LastServerTime = DateTime.Now.Ticks;

            // clear any sync error
            _lastSyncError = DateTime.MinValue;
        }
        catch (WebException)
        {
            // don't retry for a while after error
            _lastSyncError = DateTime.Now;

            // set to zero to force reset
            ServerTimeDiff = 0;
        }
    }

    /// <summary>
    /// 根据序列号和还原代码还原验证器
    /// </summary>
    /// <param name="serial">序列码，例如 US-1234-5678-1234</param>
    /// <param name="restoreCode">恢复注册时给定的代码，10个字符</param>
    public async void RestoreAsync(string serial, string restoreCode)
    {
        // get the serial data
        var serialBytes = Encoding.UTF8.GetBytes(serial.ToUpper().Replace("-", string.Empty));

        // send the request to the server to get our challenge

        byte[]? challenge = null;
        {
            var response = await IAuthenticatorNetService.Instance.ReStore(serial, serialBytes);
            // OK?
            //if (!response.IsSuccessStatusCode)
            //{
            //    if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
            //    {
            //        throw new AuthenticatorInvalidRestoreResponseException(string.Format("No response from server ({0}). Perhaps maintainence?", (int)response.StatusCode));
            //    }
            //    else
            //    {
            //        throw new AuthenticatorInvalidRestoreResponseException(string.Format("Error communicating with server: {0} - {1}", (int)response.StatusCode, response.StatusCode));
            //    }
            //}
            challenge = response.Content;
            challenge.ThrowIsNull();

            // check it is correct size
            if (challenge.Length != RESTOREINIT_BUFFER_SIZE)
            {
                throw new AuthenticatorInvalidRestoreResponseException(string.Format("Invalid response data size (expected 32 got {0})", challenge.Length));
            }
        }

        // only take the first 10 bytes of the restore code and encode to byte taking count of the missing chars
        var restoreCodeBytes = new byte[10];
        var arrayOfChar = restoreCode.ToUpper().ToCharArray();
        for (var i = 0; i < 10; i++)
        {
            restoreCodeBytes[i] = ConvertRestoreCodeCharToByte(arrayOfChar[i]);
        }

        // build the response to the challenge
        var hmac = new HMac(new Sha1Digest());
        hmac.Init(new KeyParameter(restoreCodeBytes));
        var hashdata = new byte[serialBytes.Length + challenge.Length];
        Array.Copy(serialBytes, 0, hashdata, 0, serialBytes.Length);
        Array.Copy(challenge, 0, hashdata, serialBytes.Length, challenge.Length);
        hmac.BlockUpdate(hashdata, 0, hashdata.Length);
        var hash = new byte[hmac.GetMacSize()];
        hmac.DoFinal(hash, 0);

        // create a random key
        var oneTimePad = CreateOneTimePad(20);

        // concatanate the hash and key
        var hashkey = new byte[hash.Length + oneTimePad.Length];
        Array.Copy(hash, 0, hashkey, 0, hash.Length);
        Array.Copy(oneTimePad, 0, hashkey, hash.Length, oneTimePad.Length);

        // encrypt the data with BMA public key
        var rsa = new RsaEngine();
        rsa.Init(true, new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(ENROLL_MODULUS, 16), new Org.BouncyCastle.Math.BigInteger(ENROLL_EXPONENT, 16)));
        var encrypted = rsa.ProcessBlock(hashkey, 0, hashkey.Length);

        // prepend the serial to the encrypted data
        var postbytes = new byte[serialBytes.Length + encrypted.Length];
        Array.Copy(serialBytes, 0, postbytes, 0, serialBytes.Length);
        Array.Copy(encrypted, 0, postbytes, serialBytes.Length, encrypted.Length);

        // send the challenge response back to the server

        byte[]? secretKey = null;
        {
            var response = await IAuthenticatorNetService.Instance.ReStoreValidate(serial, postbytes);
            // OK?
            //if (!response.IsSuccessStatusCode)
            //{
            //    if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
            //    {
            //        throw new AuthenticatorInvalidRestoreResponseException(string.Format("No response from server ({0}). Perhaps maintainence?", (int)response.StatusCode));
            //    }
            //    else if ((int)response.StatusCode >= 600 && (int)response.StatusCode < 700)
            //    {
            //        throw new AuthenticatorInvalidRestoreCodeException("Invalid serial number or restore code.");
            //    }
            //    else
            //    {
            //        throw new AuthenticatorInvalidRestoreResponseException(string.Format("Error communicating with server: {0} - {1}", (int)response.StatusCode, response.StatusCode));
            //    }
            //}
            secretKey = response.Content;
            secretKey.ThrowIsNull();

            // check it is correct size
            if (secretKey.Length != RESTOREVALIDATE_BUFFER_SIZE)
            {
                throw new AuthenticatorInvalidRestoreResponseException(string.Format("Invalid response data size (expected " + RESTOREVALIDATE_BUFFER_SIZE + " got {0})", secretKey.Length));
            }
        }

        // xor the returned data key with our pad to get the actual secret key
        for (var i = oneTimePad.Length - 1; i >= 0; i--)
        {
            secretKey[i] ^= oneTimePad[i];
        }

        // set the authenticator data
        SecretKey = secretKey;
        if (serial.Length == 14)
        {
            Serial = serial[..2].ToUpper() + "-" + serial.Substring(2, 4) + "-" + serial.Substring(6, 4) + "-" + serial.Substring(10, 4);
        }
        else
        {
            Serial = serial.ToUpper();
        }
        // restore code is ok
        RestoreCodeVerified = true;
        // sync the time
        ServerTimeDiff = 0L;
        Sync();
    }

    /// <summary>
    /// 从 Xml 中读取任何额外的标记
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="name">标签的名称</param>
    /// <returns>如果读取并处理了标记，则为<see langword="true"/></returns>
    public override bool ReadExtraXml(XmlReader reader, string name)
    {
        switch (name)
        {
            case "restorecodeverified":
                RestoreCodeVerified = reader.ReadElementContentAsBoolean();
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 向 XmlWriter 中添加额外的标记
    /// </summary>
    /// <param name="writer">XmlWriter 写入数据</param>
    protected override void WriteExtraXml(XmlWriter writer)
    {
        if (RestoreCodeVerified)
        {
            writer.WriteStartElement("restorecodeverified");
            writer.WriteString(bool.TrueString.ToLower());
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// 计算验证器的还原代码。这是从序列号和密钥摘要的最后10个字节中提取的，然后将其特别编码为字母数字
    /// </summary>
    /// <returns>还原验证器代码(总是10个字符)</returns>
    string BuildRestoreCode()
    {
        // return if not set
        if (string.IsNullOrEmpty(Serial) == true || SecretKey == null)
        {
            return string.Empty;
        }

        // get byte array of serial
        var serialdata = Encoding.UTF8.GetBytes(Serial.ToUpper().Replace("-", string.Empty));
        var secretdata = SecretKey;

        // combine serial data and secret data
        var combined = new byte[serialdata.Length + secretdata.Length];
        Array.Copy(serialdata, 0, combined, 0, serialdata.Length);
        Array.Copy(secretdata, 0, combined, serialdata.Length, secretdata.Length);

        // create digest of combined data
        IDigest digest = new Sha1Digest();
        digest.BlockUpdate(combined, 0, combined.Length);
        var digestdata = new byte[digest.GetDigestSize()];
        digest.DoFinal(digestdata, 0);

        // take last 10 chars of hash and convert each byte to our encoded string that doesn't use I,L,O,S
        var code = new StringBuilder();
        var startpos = digestdata.Length - 10;
        for (var i = 0; i < 10; i++)
        {
            code.Append(ConvertRestoreCodeByteToChar(digestdata[startpos + i]));
        }

        return code.ToString();
    }

    /// <summary>
    /// 创建一个随机的Model字符串用于初始化，以保护通过网络发送的init字符串
    /// </summary>
    /// <returns>随机模型串</returns>
    private static string GeneralRandomModel()
    {
        // seed a new RNG
        var randomSeedGenerator = RandomNumberGenerator.Create();
        var seedBuffer = new byte[4];
        randomSeedGenerator.GetBytes(seedBuffer);
        var random = new Random(BitConverter.ToInt32(seedBuffer, 0));

        // create a model string with available characters
        var model = new StringBuilder(MODEL_SIZE);
        for (var i = MODEL_SIZE; i > 0; i--)
        {
            model.Append(MODEL_CHARS[random.Next(MODEL_CHARS.Length)]);
        }

        return model.ToString();
    }

    #region Utility functions

    /// <summary>
    /// 将一个字符转换为字节，但使用适当的映射来排除 I,L,O 和 s，例如 a =10 但 J=18 而不是 19 (因为缺少 I)
    /// </summary>
    /// <param name="c">要转换的字符</param>
    /// <returns>还原码字符的字节值</returns>
    private static byte ConvertRestoreCodeCharToByte(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return (byte)(c - '0');
        }
        else
        {
            var index = (byte)(c + 10 - 65);
            if (c >= 'I')
            {
                index--;
            }
            if (c >= 'L')
            {
                index--;
            }
            if (c >= 'O')
            {
                index--;
            }
            if (c >= 'S')
            {
                index--;
            }

            return index;
        }
    }

    /// <summary>
    /// 将字节转换为字符，但使用适当的映射来排除 I,L,O 和 S
    /// </summary>
    /// <param name="b">要转换的字节</param>
    /// <returns>恢复码值的字符值</returns>
    static char ConvertRestoreCodeByteToChar(byte b)
    {
        var index = b & 0x1f;
        if (index <= 9)
        {
            return (char)(index + 48);
        }
        else
        {
            index = (index + 65) - 10;
            if (index >= 73)
            {
                index++;
            }
            if (index >= 76)
            {
                index++;
            }
            if (index >= 79)
            {
                index++;
            }
            if (index >= 83)
            {
                index++;
            }
            return (char)index;
        }
    }

    #endregion

    /// <summary>
    /// 从序列号的前 2 个字符中获取验证者的区域
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public string Region
    {
        get
        {
            return string.IsNullOrEmpty(Serial) == false ? Serial[..2] : string.Empty;
        }
    }

    /// <summary>
    /// 获取/设置组合的秘密数据值
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public override string? SecretData
    {
        get
        {
            // for Battle.net, this is the key + serial
            return base.SecretData + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(Serial.ThrowIsNull(nameof(Serial))));
        }

        set
        {
            // for Battle.net, extract key + serial
            if (!string.IsNullOrEmpty(value))
            {
                var parts = value.Split('|');
                if (parts.Length <= 1)
                {
                    // old Authenticator2 version with secretdata + serial
                    SecretKey = StringToByteArray(value[..40]);
                    Serial = Encoding.UTF8.GetString(StringToByteArray(value[40..]));
                }
                else if (parts.Length == 3) // alpha 3.0.6
                {
                    // secret|script|serial
                    base.SecretData = value;
                    Serial = parts.Length > 2 ? Encoding.UTF8.GetString(StringToByteArray(parts[2])) : null;
                }
                else
                {
                    // secret|serial
                    base.SecretData = value;
                    Serial = parts.Length > 1 ? Encoding.UTF8.GetString(StringToByteArray(parts[1])) : null;
                }
            }
            else
            {
                SecretKey = null;
                Serial = null;
            }
        }
    }

    /// <summary>
    /// 获取用于恢复丢失的身份验证器的身份验证器的还原代码以及序列号
    /// </summary>
    /// <returns>restore code (10 chars)</returns>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public string RestoreCode
    {
        get
        {
            return BuildRestoreCode();
        }
    }
}
