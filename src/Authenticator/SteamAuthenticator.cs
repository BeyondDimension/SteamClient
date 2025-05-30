/*
* Copyright (C) 2015 Colin Mackie.
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
* https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/SteamAuthenticator.cs
*/

using BD.SteamClient8.WinAuth.Enums;
using BD.SteamClient8.WinAuth.Models.Abstractions;
using BD.SteamClient8.WinAuth.Services.Abstractions;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinAuth;

/// <summary>
/// Steam 身份验证令牌
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
public sealed partial class SteamAuthenticator : AuthenticatorValueModel, ISteamAuthenticator
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
    [global::MessagePack.SerializationConstructor]
    public SteamAuthenticator() : base(CODE_DIGITS)
    {
    }

    /// <inheritdoc/>
    public override string? Issuer { get => STEAM_ISSUER; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.Steam;

    /// <inheritdoc/>
    public string? Serial { get; set; }

    /// <inheritdoc/>
    public string? DeviceId { get; set; }

    /// <inheritdoc/>
    public string? SteamData
    {
        get => field;
        set => SteamDataObj = IAuthenticatorNetService.Instance.Deserialize(field = value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public ISteamConvertSteamDataJsonStruct? SteamDataObj { get; private set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string? RecoveryCode => SteamDataObj?.RevocationCode;

    /// <summary>
    /// 帐户名称
    /// </summary>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string? AccountName => SteamDataObj?.AccountName;

    /// <summary>
    /// SteamId (64 位)
    /// </summary>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public long? SteamId64 => SteamDataObj?.SteamId;

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

    /// <inheritdoc cref="ISteamAuthenticator.SYNC_ERROR_MINUTES"/>
    const int SYNC_ERROR_MINUTES = ISteamAuthenticator.SYNC_ERROR_MINUTES;

    /// <inheritdoc cref="ISteamAuthenticator.ENROLL_ACTIVATE_RETRIES"/>
    public const int ENROLL_ACTIVATE_RETRIES = ISteamAuthenticator.ENROLL_ACTIVATE_RETRIES;

    /// <inheritdoc cref="ISteamAuthenticator.INVALID_ACTIVATION_CODE"/>
    public const int INVALID_ACTIVATION_CODE = ISteamAuthenticator.INVALID_ACTIVATION_CODE;

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

    /// <inheritdoc cref="ISteamAuthenticator.IEnrollState"/>
    [global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public sealed class EnrollState : ISteamAuthenticator.IEnrollState
    {
        /// <inheritdoc/>
        public string? Language { get; set; }

        string? _Username;

        /// <inheritdoc/>
        public string? Username
        {
            get => _Username;
            set => _Username = value == null ? null :
                String2.SteamUNPWDRegex().Replace(value, string.Empty);
        }

        string? _Password;

        /// <inheritdoc/>
        public string? Password
        {
            get => _Password;
            set => _Password = value == null ? null :
                String2.SteamUNPWDRegex().Replace(value, string.Empty);
        }

        // public string? CaptchaId { get; set; }
        //
        // public string? CaptchaUrl { get; set; }
        //
        // public string? CaptchaText { get; set; }

        /// <inheritdoc/>
        public string? EmailDomain { get; set; }

        /// <inheritdoc/>
        public string? EmailAuthText { get; set; }

        /// <inheritdoc/>
        public string? ActivationCode { get; set; }

        /// <inheritdoc/>
        [global::MessagePack.MessagePackFormatter(typeof(CookieFormatter))]
        public CookieContainer? Cookies { get; set; }

        /// <inheritdoc/>
        public long SteamId { get; set; }

        /// <inheritdoc/>
        public string? PhoneNumber { get; set; }

        /// <inheritdoc/>
        public bool NoPhoneNumber { get; set; }

        /// <inheritdoc/>
        public bool ReplaceAuth { get; set; }

        //public string? OAuthToken { get; set; }

        // public bool RequiresLogin { get; set; }
        //
        // public bool RequiresCaptcha { get; set; }
        //
        // public bool Requires2FA { get; set; }
        //
        // public bool RequiresEmailAuth { get; set; }

        /// <inheritdoc/>
        public bool RequiresEmailConfirmPhone { get; set; }

        /// <inheritdoc/>
        public bool RequiresActivation { get; set; }

        /// <inheritdoc/>
        public string? RevocationCode { get; set; }

        /// <inheritdoc/>
        public string? SecretKey { get; set; }

        /// <inheritdoc/>
        public bool Success { get; set; }

        /// <inheritdoc/>
        public string? Error { get; set; }

        /// <inheritdoc/>
        public string? AccessToken { get; set; }

        /// <inheritdoc/>
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

    protected override void WriteSecretData(StringBuilder b)
    {
        base.WriteSecretData(b);
        b.Append('|');
        AppendHexStringToUTF8Bytes(b, Serial);
        b.Append('|');
        AppendHexStringToUTF8Bytes(b, DeviceId);
        b.Append('|');
        AppendHexStringToUTF8Bytes(b, SteamData);
        b.Append('|');
        if (!string.IsNullOrWhiteSpace(SessionData))
        {
            AppendHexStringToUTF8Bytes(b, SessionData);
        }
    }

    protected override void SetSecretData(ReadOnlySpan<char> s)
    {
        if (s.Length == 0)
        {
            SecretKey = null;
            Serial = null;
            DeviceId = null;
            SteamData = null;
            SessionData = null;
            return;
        }

        base.SetSecretData(s);
        var parts = s.Split('|');
        if (parts.MoveNext())
        {
            Range? r1 = null, r2 = null, r3 = null, r4 = null;
            int i = 0;
            foreach (var it in parts)
            {
                var endEach = false;
                switch (i)
                {
                    case 0:
                        r1 = it;
                        break;
                    case 1:
                        r2 = it;
                        break;
                    case 2:
                        r3 = it;
                        break;
                    case 3:
                        r4 = it;
                        break;
                    default:
                        endEach = true;
                        break;
                }
                if (endEach)
                {
                    break;
                }
                i++;
            }

            Serial = r1.HasValue ? Encoding.UTF8.GetString(Convert.FromHexString(s[r1.Value])) : null;
            DeviceId = r2.HasValue ? Encoding.UTF8.GetString(Convert.FromHexString(s[r2.Value])) : null;
            SteamData = r3.HasValue ? Encoding.UTF8.GetString(Convert.FromHexString(s[r3.Value])) : string.Empty;
            if (string.IsNullOrEmpty(SteamData) == false && !(SteamData.AsSpan().TrimStart().StartsWith('{')))
            {
                // convert old recovation code into SteamData json
                //SteamData = "{\"revocation_code\":\"" + SteamData + "\"}";
                SteamData = JsonSerializer.Serialize(new SteamDataRevocationCodeJsonModel
                {
                    RevocationCode = SteamData,
                }, SteamDataRevocationCodeJsonModelJSC.Default.SteamDataRevocationCodeJsonModel);
            }
            var session = r4.HasValue ? Encoding.UTF8.GetString(Convert.FromHexString(s[r4.Value])) : null;

            if (string.IsNullOrEmpty(session) == false)
            {
                SessionData = session;
            }
        }
    }

    //[global::MemoryPack.MemoryPackIgnore, global::Newtonsoft.Json.JsonIgnore, global::System.Text.Json.Serialization.JsonIgnore]
    //public override string? SecretData
    //{
    //    get
    //    {
    //        WriteSecretData
    //        //if (Client != null && Client.Session != null)
    //        //    SessionData = Client.Session.ToString();

    //        //if (Logger != null)
    //        //{
    //        //	Logger.Debug("Get Steam data: {0}, Session:{1}", (SteamData ?? string.Empty).Replace("\n"," ").Replace("\r",""), (SessionData ?? string.Empty).Replace("\n", " ").Replace("\r", ""));
    //        //}

    //        // this is the key |  serial | deviceid

    //        Serial.ThrowIsNull();
    //        DeviceId.ThrowIsNull();
    //        SteamData.ThrowIsNull();
    //        return base.SecretData
    //               + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(Serial))
    //               + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(DeviceId))
    //               + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(SteamData))
    //               + "|" + (string.IsNullOrEmpty(SessionData) == false
    //                   ? ByteArrayToString(Encoding.UTF8.GetBytes(SessionData))
    //                   : string.Empty);
    //    }

    //    set
    //    {
    //        // extract key + serial + deviceid
    //        if (string.IsNullOrEmpty(value) == false)
    //        {
    //            var parts = value.Split('|');
    //            base.SecretData = value;
    //            Serial = parts.Length > 1 ? Encoding.UTF8.GetString(StringToByteArray(parts[1])) : null;
    //            DeviceId = parts.Length > 2 ? Encoding.UTF8.GetString(StringToByteArray(parts[2])) : null;
    //            SteamData = parts.Length > 3 ? Encoding.UTF8.GetString(StringToByteArray(parts[3])) : string.Empty;

    //            if (string.IsNullOrEmpty(SteamData) == false && SteamData[0] != '{')
    //                // convert old recovation code into SteamData json
    //                SteamData = "{\"revocation_code\":\"" + SteamData + "\"}";
    //            var session = parts.Length > 4 ? Encoding.UTF8.GetString(StringToByteArray(parts[4])) : null;

    //            //if (Logger != null)
    //            //{
    //            //	Logger.Debug("Set Steam data: {0}, Session:{1}", (SteamData ?? string.Empty).Replace("\n", " ").Replace("\r", ""), (SessionData ?? string.Empty).Replace("\n", " ").Replace("\r", ""));
    //            //}

    //            if (string.IsNullOrEmpty(session) == false)
    //                SessionData = session;
    //        }
    //        else
    //        {
    //            SecretKey = null;
    //            Serial = null;
    //            DeviceId = null;
    //            SteamData = null;
    //            SessionData = null;
    //        }
    //    }
    //}

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
            throw new EncryptedSecretDataException();

        // don't retry for 5 minutes
        if (_lastSyncError >= DateTime.Now.AddMinutes(0 - SYNC_ERROR_MINUTES))
            return;

        try
        {
            // get servertime in ms
            var servertime = await IAuthenticatorNetService.Instance.GetSteamServerTimeAsync();
            servertime *= 1000; // 转换为毫秒

            // get the difference between the server time and our current time
            ServerTimeDiff = servertime - IAuthenticatorValueModelBase.CurrentTime;
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
                ServerTimeDiff = (interval * Period * 1000L) - IAuthenticatorValueModelBase.CurrentTime;
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
}

sealed class SteamDataRevocationCodeJsonModel
{
    [global::System.Text.Json.Serialization.JsonPropertyName("revocation_code")]
    public string? RevocationCode { get; set; }
}

[JsonSerializable(typeof(SteamDataRevocationCodeJsonModel))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
sealed partial class SteamDataRevocationCodeJsonModelJSC : JsonSerializerContext
{
    static SteamDataRevocationCodeJsonModelJSC()
    {
        // https://github.com/dotnet/runtime/issues/94135
        s_defaultOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,
        };
        Default = new SteamDataRevocationCodeJsonModelJSC(new JsonSerializerOptions(s_defaultOptions));
    }
}