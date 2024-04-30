using static BD.SteamClient8.Models.WinAuth.Abstractions.IAuthenticatorValueModel;

namespace BD.SteamClient8.Models.WinAuth;

/// <inheritdoc cref="IAuthenticatorValueModel"/>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public partial class AuthenticatorValueModel : IAuthenticatorValueModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorValueModel"/> class.
    /// </summary>
    public AuthenticatorValueModel()
    {
    }

    /// <summary>
    /// 判断 <see cref="IAuthenticatorValueModel"/> 实例是否具有值
    /// </summary>
    protected virtual bool ExplicitHasValue()
    {
        return SecretKey != null && CodeDigits > 0 && HMACType.IsDefined() && Period > 0;
    }

    /// <summary>
    /// 判断 <see cref="IExplicitHasValue"/> 实例是否具有值
    /// </summary>
    bool IExplicitHasValue.ExplicitHasValue() => ExplicitHasValue();

    /// <summary>
    /// 代码中的默认位数
    /// </summary>
    public const int DEFAULT_CODE_DIGITS = 6;

    /// <summary>
    /// 默认期限为 30s
    /// </summary>
    public const int DEFAULT_PERIOD = 30;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorValueModel"/> class.
    /// </summary>
    /// <param name="codeDigits"></param>
    /// <param name="hmacType"></param>
    /// <param name="period"></param>
    public AuthenticatorValueModel(
        int codeDigits = DEFAULT_CODE_DIGITS,
        HMACTypes hmacType = HMACTypes.SHA1,
        int period = DEFAULT_PERIOD) : this()
    {
        CodeDigits = codeDigits;
        HMACType = hmacType;
        Period = period;
    }

    /// <inheritdoc cref="AuthenticatorPlatform"/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public virtual AuthenticatorPlatform Platform { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public virtual string? Issuer { get; }

    /// <inheritdoc/>
    public long ServerTimeDiff { get; set; }

    /// <inheritdoc/>
    public long LastServerTime { get; set; }

    /// <inheritdoc/>
    public byte[]? SecretKey { get; set; }

    /// <inheritdoc/>
    public int CodeDigits { get; set; }

    /// <inheritdoc/>
    public HMACTypes HMACType { get; set; }

    /// <inheritdoc/>
    public int Period { get; set; }

    /// <summary>
    /// 盐值所占的字节数
    /// </summary>
    const int SALT_LENGTH = 8;

    /// <summary>
    /// PBKDF2 密钥生成的迭代次数
    /// </summary>
    const int PBKDF2_ITERATIONS = 2000;

    /// <summary>
    /// 衍生的 PBKDF2 密钥大小
    /// </summary>
    const int PBKDF2_KEYSIZE = 256;

    /// <summary>
    /// 加密变更的版本号
    /// </summary>
    static readonly string ENCRYPTION_HEADER
        = "57494E4155544833";
    //= ByteArrayToString(Encoding.UTF8.GetBytes("WINAUTH3"));

    /// <inheritdoc cref="HMACTypes"/>
    public const HMACTypes DEFAULT_HMAC_TYPE = HMACTypes.SHA1;

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public string? EncryptedData { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public virtual string? SecretData
    {
        get
        {
            // this is the secretkey
            return ByteArrayToString(SecretKey.ThrowIsNull(nameof(SecretKey))) + "\t" + CodeDigits.ToString() + "\t" + HMACType.ToString() + "\t" + Period.ToString();
        }

        set
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                var parts = value.Split('|')[0].Split('\t');
                SecretKey = StringToByteArray(parts[0]);
                if (parts.Length > 1)
                {
                    if (int.TryParse(parts[1], out var digits) == true)
                    {
                        CodeDigits = digits;
                    }
                }
                if (parts.Length > 2)
                {
                    HMACType = (HMACTypes)Enum.Parse(typeof(HMACTypes), parts[2]);
                }
                if (parts.Length > 3)
                {
                    if (int.TryParse(parts[3], out var period) == true)
                    {
                        Period = period;
                    }
                }
            }
            else
            {
                SecretKey = null;
            }
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public PasswordTypes PasswordType { get; set; }

    /// <summary>
    /// 用于加密 secretdata 的密码（如果 PasswordType==Explict）
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    protected string? Password { get; set; }

    /// <summary>
    /// 哈希秘密数据以检测更改
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    protected byte[]? SecretHash { get; private set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public bool RequiresPassword { get; private set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public long ServerTime
    {
        get
        {
            return CurrentTime + ServerTimeDiff;
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public long CodeInterval
    {
        get
        {
            // calculate the code interval; the server's time div 30,000
            return (CurrentTime + ServerTimeDiff) / (Period * 1000L);
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public string CurrentCode
    {
        get
        {
            if (SecretKey == null && EncryptedData != null)
            {
                return string.Empty;
            }

            return CalculateCode(false);
        }
    }

    /// <summary>
    /// 计算验证器的当前代码
    /// </summary>
    /// <param name="resync"></param>
    /// <param name="interval"></param>
    /// <returns>authenticator code</returns>
    public virtual string CalculateCode(bool resync = false, long interval = -1)
    {
        // sync time if required
        if (resync || ServerTimeDiff == 0)
        {
            if (interval > 0)
            {
                ServerTimeDiff = (interval * Period * 1000L) - CurrentTime;
            }
            else
            {
                Sync();
            }
        }

        var hmac = HMACType switch
        {
            HMACTypes.SHA1 => new HMac(new Sha1Digest()),
            HMACTypes.SHA256 => new HMac(new Sha256Digest()),
            HMACTypes.SHA512 => new HMac(new Sha512Digest()),
            _ => throw ThrowHelper.GetArgumentOutOfRangeException(HMACType),
        };
        hmac.Init(new KeyParameter(SecretKey));

        var codeIntervalArray = BitConverter.GetBytes(CodeInterval);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(codeIntervalArray);
        }
        hmac.BlockUpdate(codeIntervalArray, 0, codeIntervalArray.Length);

        var mac = new byte[hmac.GetMacSize()];
        hmac.DoFinal(mac, 0);

        // the last 4 bits of the mac say where the code starts (e.g. if last 4 bit are 1100, we start at byte 12)
        var start = mac.Last() & 0x0f;

        // extract those 4 bytes
        var bytes = new byte[4];
        Array.Copy(mac, start, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        var fullcode = BitConverter.ToUInt32(bytes, 0) & 0x7fffffff;

        // we use the last 8 digits of this code in radix 10
        var codemask = (uint)Math.Pow(10, CodeDigits);
        var format = new string('0', CodeDigits);
        var code = (fullcode % codemask).ToString(format);

        return code;
    }

    /// <inheritdoc/>
    public virtual void Sync()
    {
    }

    #region Load / Save

    //    /// <summary>
    //    /// 从 <see cref="XmlReader"/> 中读取 <see cref="AuthenticatorValueModel"/> 对象
    //    /// </summary>
    //    /// <param name="reader"></param>
    //    /// <param name="password"></param>
    //    /// <returns></returns>
    //    [Obsolete("命名空间，类型名的改动导致不兼容，需要之前的数据比较修复", true)]
    //    public static AuthenticatorValueModel? ReadXmlv2(XmlReader reader, string? password = null)
    //    {
    //        AuthenticatorValueModel? authenticator = null;
    //        var authenticatorType = reader.GetAttribute("type");
    //        if (!string.IsNullOrEmpty(authenticatorType))
    //        {
    // 改成 switch type ！！
    //            authenticatorType = authenticatorType.Replace(
    //                "WindowsAuthenticator.",
    //                typeof(AuthenticatorValueModel).FullName + ".");
    //            var authenticatorType_ = Assembly.GetExecutingAssembly().GetType(authenticatorType, false, true);
    //            authenticatorType_ = authenticatorType_.ThrowIsNull();
    //#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
    //            authenticator = (AuthenticatorValueModel?)Activator.CreateInstance(authenticatorType_);
    //#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
    //        }
    //        if (authenticator == null)
    //        {
    //            return null;
    //        }

    //        reader.MoveToContent();
    //        if (reader.IsEmptyElement)
    //        {
    //            reader.Read();
    //            return null;
    //        }

    //        reader.Read();
    //        while (reader.EOF == false)
    //        {
    //            if (reader.IsStartElement())
    //            {
    //                switch (reader.Name)
    //                {
    //                    case "servertimediff":
    //                        authenticator.ServerTimeDiff = reader.ReadElementContentAsLong();
    //                        break;

    //                    //case "restorecodeverified":
    //                    //	authenticator.RestoreCodeVerified = reader.ReadElementContentAsBoolean();
    //                    //	break;

    //                    case "secretdata":
    //                        var encrypted = reader.GetAttribute("encrypted");
    //                        var data = reader.ReadElementContentAsString();

    //                        var passwordType = DecodePasswordTypes(encrypted);

    //                        if (passwordType != PasswordTypes.None)
    //                        {
    //                            // this is an old version so there is no hash
    //                            data = DecryptSequence(data, passwordType, password);
    //                        }

    //                        authenticator.PasswordType = PasswordTypes.None;
    //                        authenticator.SecretData = data;

    //                        break;

    //                    default:
    //                        if (authenticator.ReadExtraXml(reader, reader.Name) == false)
    //                        {
    //                            reader.Skip();
    //                        }
    //                        break;
    //                }
    //            }
    //            else
    //            {
    //                reader.Read();
    //                break;
    //            }
    //        }

    //        return authenticator;
    //    }

    /// <summary>
    /// 读取额外的 Xml 内容
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual bool ReadExtraXml(XmlReader reader, string name)
    {
        return false;
    }

    /// <summary>
    /// 将字符串密码类型转换为 <see cref="PasswordTypes"/> 类型
    /// </summary>
    /// <param name="passwordTypes">密码类型的字符串版本</param>
    /// <returns></returns>
    public static PasswordTypes DecodePasswordTypes(string? passwordTypes)
    {
        var passwordType = PasswordTypes.None;
        if (string.IsNullOrEmpty(passwordTypes))
        {
            return passwordType;
        }

        var types = passwordTypes.ToCharArray();
        for (var i = types.Length - 1; i >= 0; i--)
        {
            var type = types[i];
            switch (type)
            {
                case 'u':
                    passwordType |= PasswordTypes.User;
                    break;
                case 'm':
                    passwordType |= PasswordTypes.Machine;
                    break;
                case 'y':
                    passwordType |= PasswordTypes.Explicit;
                    break;
                case 'a':
                    passwordType |= PasswordTypes.YubiKeySlot1;
                    break;
                case 'b':
                    passwordType |= PasswordTypes.YubiKeySlot2;
                    break;
                default:
                    break;
            }
        }

        return passwordType;
    }

    /// <summary>
    /// 将 PasswordTypes 类型编码为字符串以存储在配置中
    /// </summary>
    /// <param name="passwordType">PasswordTypes value</param>
    /// <returns>string version</returns>
    public static string EncodePasswordTypes(PasswordTypes passwordType)
    {
        var encryptedTypes = new StringBuilder();
        if ((passwordType & PasswordTypes.Explicit) != 0)
        {
            encryptedTypes.Append('y');
        }
        if ((passwordType & PasswordTypes.User) != 0)
        {
            encryptedTypes.Append('u');
        }
        if ((passwordType & PasswordTypes.Machine) != 0)
        {
            encryptedTypes.Append('m');
        }

        return encryptedTypes.ToString();
    }

    /// <summary>
    /// 设置加密方式和密码
    /// </summary>
    /// <param name="passwordType">密码类型</param>
    /// <param name="password">密码</param>
    public void SetEncryption(PasswordTypes passwordType, string? password = null)
    {
        // check if still encrpyted
        if (RequiresPassword == true)
        {
            // have to decrypt to be able to re-encrypt
            throw new AuthenticatorEncryptedSecretDataException();
        }

        if (passwordType == PasswordTypes.None)
        {
            RequiresPassword = false;
            EncryptedData = null;
            PasswordType = passwordType;
        }
        else
        {
            using var ms = new MemoryStream();
            // get the plain version
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
            };
            using (var encryptedwriter = XmlWriter.Create(ms, settings))
            {
                var encrpytedData = EncryptedData;
                var savedpasswordType = PasswordType;
                try
                {
                    PasswordType = PasswordTypes.None;
                    EncryptedData = null;
                    WriteToWriter(encryptedwriter);
                }
                finally
                {
                    PasswordType = savedpasswordType;
                    EncryptedData = encrpytedData;
                }
            }
            var data = ByteArrayToString(ms.ToArray());

            // update secret hash
            SecretHash = SHA1.HashData(Encoding.UTF8.GetBytes(SecretData.ThrowIsNull(nameof(SecretData))));

            // encrypt
            EncryptedData = EncryptSequence(data, passwordType, password);
            PasswordType = passwordType;
            if (PasswordType == PasswordTypes.Explicit)
            {
                SecretData = null;
                RequiresPassword = true;
            }
        }
    }

    /// <inheritdoc/>
    public void Protect()
    {
        if (PasswordType != PasswordTypes.None)
        {
            // check if the data has changed
            //if (this.SecretData != null)
            //{
            //	using (SHA1 sha1 = SHA1.Create())
            //	{
            //		byte[] secretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(this.SecretData));
            //		if (this.SecretHash == null || secretHash.SequenceEqual(this.SecretHash) == false)
            //		{
            //			// we need to encrypt changed secret data
            //			SetEncryption(this.PasswordType, this.Password);
            //		}
            //	}
            //}

            SecretData = null;
            RequiresPassword = true;
            Password = null;
        }
    }

    /// <inheritdoc/>
    public bool Unprotect(string? password)
    {
        var passwordType = PasswordType;
        if (passwordType == PasswordTypes.None)
        {
            throw new InvalidOperationException("Cannot Unprotect a non-encrypted authenticator");
        }

        // decrypt
        var changed = false;
        try
        {
            var data = DecryptSequence(
                EncryptedData.ThrowIsNull(nameof(EncryptedData)),
                PasswordType, password);
            using (var ms = new MemoryStream(StringToByteArray(data)))
            {
                var reader = XmlReader.Create(ms);
                changed = ReadXml(reader, password) || changed;
            }
            RequiresPassword = false;
            // calculate hash of current secretdata
            SecretHash = SHA1.HashData(Encoding.UTF8.GetBytes(SecretData.ThrowIsNull(nameof(SecretData))));
            // keep the password until we reprotect in case data changes
            Password = password;

            if (changed == true)
            {
                // we need to encrypt changed secret data
                using var ms = new MemoryStream();
                // get the plain version
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                };
                using (var encryptedwriter = XmlWriter.Create(ms, settings))
                {
                    WriteToWriter(encryptedwriter);
                }
                var encrypteddata = ByteArrayToString(ms.ToArray());

                // update secret hash
                SecretHash = SHA1.HashData(Encoding.UTF8.GetBytes(SecretData.ThrowIsNull(nameof(SecretData))));

                // encrypt
                EncryptedData = EncryptSequence(encrypteddata, passwordType, password);
            }

            return changed;
        }
        catch (AuthenticatorEncryptedSecretDataException)
        {
            RequiresPassword = true;
            throw;
        }
        finally
        {
            PasswordType = passwordType;
        }
    }

    /// <inheritdoc/>
    public bool ReadXml(XmlReader reader, string? password = null)
    {
        // decode the password type
        var encrypted = reader.GetAttribute("encrypted");
        var passwordType = DecodePasswordTypes(encrypted);
        PasswordType = passwordType;

        if (passwordType != PasswordTypes.None)
        {
            // read the encrypted text from the node
            EncryptedData = reader.ReadElementContentAsString();
            return Unprotect(password);

            //// decrypt
            //try
            //{
            //	string data = Authenticator.DecryptSequence(this.EncryptedData, passwordType, password);
            //	using (MemoryStream ms = new MemoryStream(Authenticator.StringToByteArray(data)))
            //	{
            //		reader = XmlReader.Create(ms);
            //		this.ReadXml(reader, password);
            //	}
            //}
            //catch (EncryptedSecretDataException)
            //{
            //	this.RequiresPassword = true;
            //	throw;
            //}
            //finally
            //{
            //	this.PasswordType = passwordType;
            //}
        }

        reader.MoveToContent();
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return false;
        }

        reader.Read();
        while (reader.EOF == false)
        {
            if (reader.IsStartElement())
            {
                switch (reader.Name)
                {
                    case "lastservertime":
                        LastServerTime = reader.ReadElementContentAsLong();
                        break;

                    case "servertimediff":
                        ServerTimeDiff = reader.ReadElementContentAsLong();
                        break;

                    case "secretdata":
                        SecretData = reader.ReadElementContentAsString();
                        break;

                    default:
                        if (ReadExtraXml(reader, reader.Name) == false)
                        {
                            reader.Skip();
                        }
                        break;
                }
            }
            else
            {
                reader.Read();
                break;
            }
        }

        // check if we need to sync, or if it's been a day
        if (this is HOTPAuthenticator)
        {
            // no time sync
            return true;
        }
        else if (ServerTimeDiff == 0 || LastServerTime == 0 || LastServerTime < DateTime.Now.AddHours(-24).Ticks)
        {
            Task.Run(Sync).ForgetAndDispose();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 将此验证器写入 XmlWriter
    /// </summary>
    /// <param name="writer">XmlWriter to receive authenticator</param>
    public void WriteToWriter(XmlWriter writer)
    {
        writer.WriteStartElement("authenticatordata");
        //writer.WriteAttributeString("type", this.GetType().FullName);
        var encrypted = EncodePasswordTypes(PasswordType);
        if (string.IsNullOrEmpty(encrypted) == false)
        {
            writer.WriteAttributeString("encrypted", encrypted);
        }

        if (PasswordType != PasswordTypes.None)
        {
            writer.WriteRaw(EncryptedData.ThrowIsNull(nameof(EncryptedData)));
        }
        else
        {
            writer.WriteStartElement("servertimediff");
            writer.WriteString(ServerTimeDiff.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("lastservertime");
            writer.WriteString(LastServerTime.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("secretdata");
            writer.WriteString(SecretData);
            writer.WriteEndElement();

            WriteExtraXml(writer);
        }

        /*
              if (passwordType != Authenticator.PasswordTypes.None)
              {
                //string data = this.EncryptedData;
                //if (data == null)
                //{
                //	using (MemoryStream ms = new MemoryStream())
                //	{
                //		XmlWriterSettings settings = new XmlWriterSettings();
                //		settings.Indent = true;
                //		settings.Encoding = Encoding.UTF8;
                //		using (XmlWriter encryptedwriter = XmlWriter.Create(ms, settings))
                //		{
                //			Authenticator.PasswordTypes savedpasswordType = PasswordType;
                //			PasswordType = Authenticator.PasswordTypes.None;
                //			WriteToWriter(encryptedwriter);
                //			PasswordType = savedpasswordType;
                //		}
                //		data = Authenticator.ByteArrayToString(ms.ToArray());
                //	}

                //	data = Authenticator.EncryptSequence(data, PasswordType, Password);
                //}

                writer.WriteString(this.EncryptedData);
                writer.WriteEndElement();

                return;
              }

              //
              writer.WriteStartElement("servertimediff");
              writer.WriteString(ServerTimeDiff.ToString());
              writer.WriteEndElement();
              //
              writer.WriteStartElement("secretdata");
              writer.WriteString(SecretData);
              writer.WriteEndElement();

              WriteExtraXml(writer);
        */

        writer.WriteEndElement();
    }

    /*
        /// <summary>
        /// Write this authenticator into an XmlWriter
        /// </summary>
        /// <param name="writer">XmlWriter to receive authenticator</param>
        protected void WriteToWriter(XmlWriter writer, PasswordTypes passwordType)
        {
          if (passwordType != Authenticator.PasswordTypes.None)
          {
            writer.WriteStartElement("authenticatordata");
            writer.WriteAttributeString("encrypted", EncodePasswordTypes(this.PasswordType));
            writer.WriteString(this.EncryptedData);
            writer.WriteEndElement();
          }
          else
          {
            writer.WriteStartElement("servertimediff");
            writer.WriteString(ServerTimeDiff.ToString());
            writer.WriteEndElement();
            //
            writer.WriteStartElement("secretdata");
            writer.WriteString(SecretData);
            writer.WriteEndElement();

            WriteExtraXml(writer);
          }
        }
    */

    /// <summary>
    /// 将任何特定于类的 xml 节点写入编写器的虚拟函数
    /// </summary>
    /// <param name="writer">XmlWriter 写入数据</param>
    protected virtual void WriteExtraXml(XmlWriter writer)
    {
    }

    #endregion

    #region Utility functions

    /// <summary>
    /// 通过生成一个随机块，然后根据需要多次对该块进行哈希，创建一个一次性填充
    /// </summary>
    /// <param name="length">所需长度</param>
    /// <returns>包含随机数据的字节数组</returns>
    protected internal static byte[] CreateOneTimePad(int length)
    {
        // There is a MITM vulnerability from using the standard Random call
        // see https://docs.google.com/document/edit?id=1pf-YCgUnxR4duE8tr-xulE3rJ1Hw-Bm5aMk5tNOGU3E&hl=en
        // in http://code.google.com/p/winauth/issues/detail?id=2
        // so we switch out to use RNGCryptoServiceProvider instead of Random

        var random = RandomNumberGenerator.Create();

        var randomblock = new byte[length];
        var i = 0;
        do
        {
            var hashBlock = new byte[128];
            random.GetBytes(hashBlock);

            var key = SHA1.HashData(hashBlock.AsSpan(0, hashBlock.Length));
            if (key.Length >= randomblock.Length)
            {
                Array.Copy(key, 0, randomblock, i, randomblock.Length);
                break;
            }
            Array.Copy(key, 0, randomblock, i, key.Length);
            i += key.Length;
        }
        while (true);

        return randomblock;
    }

    /// <summary>
    /// 获取自 1/1/70 以来的毫秒数（与 Java currentTimeMillis 相同）
    /// </summary>
    public static long CurrentTime => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 将十六进制字符串转换为字节数组。例如“001f406a”->byte[]｛0x00，0x1f，0x40，0x6a｝
    /// </summary>
    /// <param name="hex">要转换的十六进制字符串</param>
    /// <returns>十六进制字符串的 byte[]</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] StringToByteArray(string hex) => Convert2.FromHexString(hex);

    /// <summary>
    /// 将字节数组转换为 ascii 十六进制字符串，例如 byte[]｛0x00,0x1f，0x40，ox6a｝->“001f406a”
    /// </summary>
    /// <param name="bytes">要转换的字节数组</param>
    /// <returns>字节数组的字符串版本</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ByteArrayToString(byte[] bytes) => bytes.ToHexString();

    /// <summary>
    /// 使用选定的加密类型解密字符串序列
    /// </summary>
    /// <param name="data">要解密的十六进制编码字符串序列</param>
    /// <param name="encryptedTypes">加密类型</param>
    /// <param name="password">可选密码</param>
    /// <param name="decode"></param>
    /// <returns>解密字符串序列</returns>
    public static string DecryptSequence(string data, PasswordTypes encryptedTypes, string? password, bool decode = false)
    {
        // check for encrpytion header
        if (data.Length < ENCRYPTION_HEADER.Length || !data.StartsWith(ENCRYPTION_HEADER))
        {
            return DecryptSequenceNoHash(data, encryptedTypes, password, decode);
        }

        // extract salt and hash
        //using (var sha = new SHA256Managed())
        using (var sha = SafeHasher("SHA256"))
        {
            // jump header
            var datastart = ENCRYPTION_HEADER.Length;
            var salt = data.Substring(datastart, Math.Min(SALT_LENGTH * 2, data.Length - datastart));
            datastart += salt.Length;
            var hash = data.Substring(datastart, Math.Min(sha.HashSize / 8 * 2, data.Length - datastart));
            datastart += hash.Length;
            data = data[datastart..];

            data = DecryptSequenceNoHash(data, encryptedTypes, password);

            // check the hash
            var compareplain = StringToByteArray(salt + data);
            var comparehash = ByteArrayToString(sha.ComputeHash(compareplain));
            if (string.Compare(comparehash, hash) != 0)
            {
                throw new AuthenticatorBadPasswordException();
            }
        }

        return data;
    }

    /// <summary>
    /// 使用选定的加密类型解密字符串序列
    /// </summary>
    /// <param name="data">要解密的十六进制编码字符串序列</param>
    /// <param name="encryptedTypes">加密类型</param>
    /// <param name="password">可选密码</param>
    /// <param name="decode"></param>
    /// <returns>decrypted string sequence</returns>
    private static string DecryptSequenceNoHash(string data, PasswordTypes encryptedTypes, string? password, bool decode = false)
    {
        try
        {
            // reverse order they were encrypted
            if ((encryptedTypes & PasswordTypes.Machine) != 0)
            {
                // we are going to decrypt with the Windows local machine key
                var cipher = StringToByteArray(data);
                var plain = Unprotect(cipher, null, DataProtectionScope.LocalMachine);
                if (decode == true)
                {
                    data = Encoding.UTF8.GetString(plain, 0, plain.Length);
                }
                else
                {
                    data = ByteArrayToString(plain);
                }
            }
            if ((encryptedTypes & PasswordTypes.User) != 0)
            {
                // we are going to decrypt with the Windows User account key
                var cipher = StringToByteArray(data);
                var plain = Unprotect(cipher, null, DataProtectionScope.CurrentUser);
                if (decode == true)
                {
                    data = Encoding.UTF8.GetString(plain, 0, plain.Length);
                }
                else
                {
                    data = ByteArrayToString(plain);
                }
            }
            if ((encryptedTypes & PasswordTypes.Explicit) != 0)
            {
                // we use an explicit password to encrypt data
                if (string.IsNullOrEmpty(password) == true)
                {
                    throw new AuthenticatorEncryptedSecretDataException();
                }
                data = Decrypt(data, password, true);
                if (decode == true)
                {
                    var plain = StringToByteArray(data);
                    data = Encoding.UTF8.GetString(plain, 0, plain.Length);
                }
            }
            if ((encryptedTypes & PasswordTypes.YubiKeySlot1) != 0 || (encryptedTypes & PasswordTypes.YubiKeySlot2) != 0)
            {
                throw new NotSupportedException();
            }
        }
        catch (AuthenticatorEncryptedSecretDataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AuthenticatorBadPasswordException(ex);
        }

        return data;
    }

    /// <summary>
    /// Downgrade SHA256 or MD5 to SHA1 to be FIPS compliant
    /// </summary>
    public static HashAlgorithm SafeHasher(string name)
    {
        try
        {
            if (name == "SHA512")
            {
                return SHA512.Create();
            }
            if (name == "SHA256")
            {
                return SHA256.Create();
            }
            if (name == "MD5")
            {
                return MD5.Create();
            }

            return SHA1.Create();
        }
        catch (Exception)
        {
            // FIPS only allows SHA1 before Windows 10
            return SHA1.Create();
        }
    }

    /// <summary>
    /// 将数据进行加密处理，并返回加密后的结果字符串
    /// </summary>
    /// <param name="data">需要加密的原始数据</param>
    /// <param name="passwordType">密码类型枚举</param>
    /// <param name="password">加密所需的密码</param>
    /// <returns>加密后的结果字符串</returns>
    public static string EncryptSequence(string data, PasswordTypes passwordType, string? password)
    {
        // get hash of original
        var random = RandomNumberGenerator.Create();
        var saltbytes = new byte[SALT_LENGTH];
        random.GetBytes(saltbytes);
        var salt = ByteArrayToString(saltbytes);

        string hash;
        //using (var sha = new SHA256Managed())
        using (var sha = SafeHasher("SHA256"))
        {
            var plain = StringToByteArray(salt + data);
            hash = ByteArrayToString(sha.ComputeHash(plain));
        }

        if ((passwordType & PasswordTypes.YubiKeySlot1) != 0 || (passwordType & PasswordTypes.YubiKeySlot2) != 0)
        {
            throw new NotSupportedException();
        }
        if ((passwordType & PasswordTypes.Explicit) != 0)
        {
            var encrypted = Encrypt(data, password);

            // test the encryption
            var decrypted = Decrypt(encrypted, password, true);
            if (string.Compare(data, decrypted) != 0)
            {
                throw new AuthenticatorInvalidEncryptionException(data, password, encrypted, decrypted);
            }
            data = encrypted;
        }
        if ((passwordType & PasswordTypes.User) != 0)
        {
            // we encrypt the data using the Windows User account key
            var plain = StringToByteArray(data);
            var cipher = Protect(plain, null, DataProtectionScope.CurrentUser);
            data = ByteArrayToString(cipher);
        }
        if ((passwordType & PasswordTypes.Machine) != 0)
        {
            // we encrypt the data using the Local Machine account key
            var plain = StringToByteArray(data);
            var cipher = Protect(plain, null, DataProtectionScope.LocalMachine);
            data = ByteArrayToString(cipher);
        }

        // prepend the salt + hash
        return ENCRYPTION_HEADER + salt + hash + data;
    }

    enum DataProtectionScope
    {
        CurrentUser = 0,
        LocalMachine = 1,
    }

    static byte[] Protect(byte[] userData, byte[]? optionalEntropy, DataProtectionScope scope)
    {
        byte[] result;
#if WINDOWS
        result = ProtectedData.Protect(userData, optionalEntropy, (System.Security.Cryptography.DataProtectionScope)scope);
#else

        var localDataProtectionProviderService = Ioc.Get<ILocalDataProtectionProvider.IProtectedData>();
        result = localDataProtectionProviderService.Protect(userData);
#endif
        return result;
    }

    static byte[] Unprotect(byte[] encryptedData, byte[]? optionalEntropy, DataProtectionScope scope)
    {
        byte[] result;
#if WINDOWS
        result = ProtectedData.Unprotect(encryptedData, optionalEntropy, (System.Security.Cryptography.DataProtectionScope)scope);
#else
        var localDataProtectionProviderService = Ioc.Get<ILocalDataProtectionProvider.IProtectedData>();
        result = localDataProtectionProviderService.Unprotect(encryptedData);
#endif
        return result;
    }

    /// <summary>
    /// 用给定的密钥加密字符串
    /// </summary>
    /// <param name="plain">要加密的数据-字节数组的十六进制表示</param>
    /// <param name="password">用于加密的密钥</param>
    /// <returns>十六进制编码加密字符串</returns>
    public static string Encrypt(string plain, string? password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password.ThrowIsNull(nameof(password)));

        // build a new salt
        var rg = RandomNumberGenerator.Create();
        var saltbytes = new byte[SALT_LENGTH];
        rg.GetBytes(saltbytes);
        var salt = ByteArrayToString(saltbytes);

        // build our PBKDF2 key
        Rfc2898DeriveBytes kg = new(passwordBytes, saltbytes, PBKDF2_ITERATIONS, HashAlgorithmName.SHA1);
        var key = kg.GetBytes(PBKDF2_KEYSIZE);

        return salt + Encrypt(plain, key);
    }

    /// <summary>
    /// 使用字节数组密钥加密字符串
    /// </summary>
    /// <param name="plain">要加密的数据-字节数组的十六进制表示</param>
    /// <param name="key">用于加密的密钥</param>
    /// <returns>十六进制编码加密字符串</returns>
    public static string Encrypt(string plain, byte[] key)
    {
        var inBytes = StringToByteArray(plain);

        // get our cipher
        BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
        cipher.Init(true, new KeyParameter(key));

        // encrypt data
        var osize = cipher.GetOutputSize(inBytes.Length);
        var outBytes = new byte[osize];
        var olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
        olen += cipher.DoFinal(outBytes, olen);
        if (olen < osize)
        {
            var t = new byte[olen];
            Array.Copy(outBytes, 0, t, 0, olen);
            outBytes = t;
        }

        // return encoded byte->hex string
        return ByteArrayToString(outBytes);
    }

    /// <summary>
    /// 使用 MD5 或 PBKDF2 生成的密钥解密十六进制编码的字符串
    /// </summary>
    /// <param name="data">要解密的数据字符串</param>
    /// <param name="password"></param>
    /// <param name="PBKDF2">标志，指示我们正在使用PBKDF2生成派生密钥</param>
    /// <returns>十六进制编码解密字符串</returns>
    public static string Decrypt(
        string data,
        string? password,
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        bool PBKDF2)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
    {
        byte[] key;
        var saltBytes = StringToByteArray(data[..(SALT_LENGTH * 2)]);

        if (PBKDF2 == true)
        {
            // extract the salt from the data
            var passwordBytes = Encoding.UTF8.GetBytes(password.ThrowIsNull());

            // build our PBKDF2 key
#if NETCF
			PBKDF2 kg = new PBKDF2(passwordBytes, saltbytes, 2000);
#else
            var kg = new Rfc2898DeriveBytes(passwordBytes, saltBytes, PBKDF2_ITERATIONS, HashAlgorithmName.SHA1);
#endif
            key = kg.GetBytes(PBKDF2_KEYSIZE);
        }
        else
        {
            // extract the salt from the data
            var passwordBytes = Encoding.Default.GetBytes(password.ThrowIsNull(nameof(password)));
            key = new byte[saltBytes.Length + passwordBytes.Length];
            Array.Copy(saltBytes, key, saltBytes.Length);
            Array.Copy(passwordBytes, 0, key, saltBytes.Length, passwordBytes.Length);
            // build out combined key
            key = MD5.HashData(key);
        }

        // extract the actual data to be decrypted
        var inBytes = StringToByteArray(data[(SALT_LENGTH * 2)..]);

        // get cipher
        BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
        cipher.Init(false, new KeyParameter(key));

        // decrypt the data
        var osize = cipher.GetOutputSize(inBytes.Length);
        var outBytes = new byte[osize];
        try
        {
            var olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
            olen += cipher.DoFinal(outBytes, olen);
            if (olen < osize)
            {
                var t = new byte[olen];
                Array.Copy(outBytes, 0, t, 0, olen);
                outBytes = t;
            }
        }
        catch (Exception ex)
        {
            // an exception is due to bad password
            throw new AuthenticatorBadPasswordException(ex);
        }

        // return encoded string
        return ByteArrayToString(outBytes);
    }

    /// <summary>
    /// 使用字节数组密钥解密十六进制编码的字符串
    /// </summary>
    /// <param name="data">十六进制编码字符串</param>
    /// <param name="key">解密密钥</param>
    /// <returns>十六进制编码的纯文本</returns>
    public static string Decrypt(string data, byte[] key)
    {
        // the actual data to be decrypted
        var inBytes = StringToByteArray(data);

        // get cipher
        BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
        cipher.Init(false, new KeyParameter(key));

        // decrypt the data
        var osize = cipher.GetOutputSize(inBytes.Length);
        var outBytes = new byte[osize];
        try
        {
            var olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
            olen += cipher.DoFinal(outBytes, olen);
            if (olen < osize)
            {
                var t = new byte[olen];
                Array.Copy(outBytes, 0, t, 0, olen);
                outBytes = t;
            }
        }
        catch (Exception ex)
        {
            // an exception is due to bad password
            throw new AuthenticatorBadPasswordException(ex);
        }

        // return encoded string
        return ByteArrayToString(outBytes);
    }

    /// <summary>
    /// 为了与 NETCF35 兼容，包装了 TryParse 来转换 long
    /// </summary>
    /// <param name="s">要分析的值字符串</param>
    /// <param name="val">返回值</param>
    /// <returns>如果值已解析，则为 <see langword="true"/></returns>
    protected internal static bool LongTryParse(string s, out long val)
    {
        return long.TryParse(s, out val);
    }

    #endregion
}
