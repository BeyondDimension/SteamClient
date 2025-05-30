/*
* Copyright (C) 2013 Colin Mackie.
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
* https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs
*/

using BD.SteamClient8.WinAuth.Enums;
using BD.SteamClient8.WinAuth.Models.Abstractions;
using BD.SteamClient8.WinAuth.Services.Abstractions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace WinAuth;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
public sealed partial class BattleNetAuthenticator : AuthenticatorValueModel
{
    /// <summary>
    /// 代码中的位数
    /// </summary>
    const int CODE_DIGITS = 8;

    const string BATTLENET_ISSUER = "BattleNet";

    /// <summary>
    /// 创建一个新的 <see cref="BattleNetAuthenticator"/> 对象
    /// </summary>
    [global::MessagePack.SerializationConstructor]
    public BattleNetAuthenticator() : base(CODE_DIGITS)
    {
    }

    /// <inheritdoc/>
    public override string? Issuer { get => BATTLENET_ISSUER; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.BattleNet;

    /// <summary>
    /// 获取或设置序列号
    /// </summary>
    public string? Serial
    {
        get => field;
        set
        {
            field = value;
            if (!string.IsNullOrWhiteSpace(value) && value.Length >= 2)
            {
                Span<char> temp = [char.ToUpperInvariant(value[0]), char.ToUpperInvariant(value[1])];
                Region = new(temp);
            }
        }
    }

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
    public const string REGION_US = "US";
    public const string REGION_EU = "EU";
    public const string REGION_KR = "KR";
    public const string REGION_CN = "CN";

    const string ENROLL_PATH = "/enrollment/enroll2.htm";
    const string SYNC_PATH = "/enrollment/time.htm";
    const string RESTORE_PATH = "/enrollment/initiatePaperRestore.htm";
    const string RESTOREVALIDATE_PATH = "/enrollment/validatePaperRestore.htm";

    /// <summary>
    /// 获取基于区域的基本移动端 url
    /// </summary>
    static string GetMobileUrl(string region, string relativeUrl)
    {
        static string GetMobileUrl(string baseUrl, string relativeUrl)
        {
            if (relativeUrl.StartsWith('/'))
            {
                return $"{baseUrl}{relativeUrl}";
            }
            return $"{baseUrl}/{relativeUrl}";
        }

        if (region != null && region.Length >= 2)
        {
            switch (char.ToUpperInvariant(region[0]))
            {
                case 'C':
                    switch (char.ToUpperInvariant(region[1]))
                    {
                        case 'N':
                            return GetMobileUrl("https://mobile-service.battlenet.com.cn", relativeUrl);
                    }
                    break;
            }
        }

        return GetMobileUrl("https://mobile-service.blizzard.com", relativeUrl);
    }

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
    public async void Enroll()
    {
        // default to US
        var region = Region;
        var country = RegionInfo.CurrentRegion.TwoLetterISORegionName;
        if (country == "CN")
        {
            if (region != REGION_CN)
            {
                country = region;
            }
        }
        else if (EU_COUNTRIES.Contains(country))
        {
            region = REGION_EU;
        }
        else if (KR_COUNTRIES.Contains(country))
        {
            region = REGION_KR;
        }

        // Battle.net does a GEO IP lookup anyway so there is no need to pass the region
        // however China has its own URL so we must still do our own GEO IP lookup to find the country

        // 截至 2016 年，geoiplookup 服务已关闭。
        //string? responseString = null;
        //try
        //{
        //    var georesponse = await IAuthenticatorNetService.Instance.GEOIP();
        //    responseString = georesponse?.Content;
        //}
        //catch (Exception)
        //{
        //}
        //if (string.IsNullOrEmpty(responseString) == false)
        //{
        //    // not worth a full json parser, just regex it
        //    //var match = Regex.Match(responseString, ".*\"country\":\"([^\"]*)\".*", RegexOptions.IgnoreCase);
        //    var countryregex = CountryRegex();
        //    var match = countryregex.Match(responseString);
        //    if (match.Success == true)
        //    {
        //        // match the correct region
        //        country = match.Groups[1].Value.ToUpperInvariant();

        //        if (EU_COUNTRIES.Contains(country) == true)
        //        {
        //            region = REGION_EU;
        //        }
        //        else if (KR_COUNTRIES.Contains(country) == true)
        //        {
        //            region = REGION_KR;
        //        }
        //        else if (country == REGION_CN)
        //        {
        //            region = REGION_CN;
        //        }
        //        else
        //        {
        //            region = REGION_US;
        //        }
        //    }
        //}

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
        rsa.Init(true, new RsaKeyParameters(false, new global::Org.BouncyCastle.Math.BigInteger(ENROLL_MODULUS, 16), new global::Org.BouncyCastle.Math.BigInteger(ENROLL_EXPONENT, 16)));
        var encrypted = rsa.ProcessBlock(data, 0, data.Length);

        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs#L360-L405
        // call the enroll server
        byte[]? responseData;
        try
        {
            var requestUrl = GetMobileUrl(region, ENROLL_PATH);
            (var statusCode, responseData) = await IAuthenticatorNetService.Instance.PostByteArrayAsync(requestUrl, encrypted, ENROLL_RESPONSE_SIZE);

            var statusCodeInt32 = (int)statusCode;
            var isSuccessStatusCode = (statusCodeInt32 >= 200) && (statusCodeInt32 <= 299);
            // OK?
            if (!isSuccessStatusCode)
            {
                throw new InvalidEnrollResponseException(string.Format("{0}: {1}", statusCodeInt32, statusCode));
            }

            // check it is correct size
            if (responseData == null || responseData.Length != ENROLL_RESPONSE_SIZE)
            {
                throw new InvalidEnrollResponseException(string.Format("Invalid response data size (expected 45 got {0})", responseData?.Length));
            }
        }
        catch (Exception ex)
        {
            throw new InvalidEnrollResponseException(
                "Cannot contact Battle.net servers: " + ex.Message, ex);
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
        ServerTimeDiff = BitConverter.ToInt64(serverTime, 0) - IAuthenticatorValueModelBase.CurrentTime;

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
            Enroll();
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
            throw new EncryptedSecretDataException();
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
            var requestUrl = GetMobileUrl(Region, SYNC_PATH);
            byte[]? responseData = null;
            (var statusCode, responseData) = await IAuthenticatorNetService.Instance.GetByteArrayAsync(requestUrl, RESPONSE_BUFFER_SIZE);

            var statusCodeInt32 = (int)statusCode;
            var isSuccessStatusCode = (statusCodeInt32 >= 200) && (statusCodeInt32 <= 299);
            // OK?
            if (!isSuccessStatusCode)
            {
                throw new InvalidEnrollResponseException(string.Format("{0}: {1}", statusCodeInt32, statusCode));
            }

            // check it is correct size
            if (responseData == null || responseData.Length != SYNC_RESPONSE_SIZE)
            {
                throw new InvalidSyncResponseException(string.Format("Invalid response data size (expected " + SYNC_RESPONSE_SIZE + " got {0}", responseData?.Length));
            }

            // return data:
            // 00-07 server time (Big Endian)

            // extract the server time
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(responseData);
            }
            // get the difference between the server time and our current time
            var serverTimeDiff = BitConverter.ToInt64(responseData, 0) - IAuthenticatorValueModelBase.CurrentTime;

            // update the Data object
            ServerTimeDiff = serverTimeDiff;
            LastServerTime = DateTime.Now.Ticks;

            // clear any sync error
            _lastSyncError = DateTime.MinValue;
        }
        catch (Exception)
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
        byte[] serialBytes;
        var serial2 = ArrayPool<char>.Shared.Rent(serial.Length);
        try
        {
            var i2 = 0;
            for (int i = 0; i < serial.Length; i++)
            {
                var it = serial[i];
                if (it == '-')
                {
                    continue;
                }
                serial2[i2] = char.ToUpperInvariant(it);
                i2++;
            }
            serialBytes = Encoding.UTF8.GetBytes(serial2, 0, i2 + 1);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(serial2);
        }

        // send the request to the server to get our challenge
        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs#L553-L604
        byte[]? challenge = null;
        try
        {
            var requestUrl = GetMobileUrl(serial, RESTORE_PATH);
            (var statusCode, challenge) = await IAuthenticatorNetService.Instance.PostByteArrayAsync(requestUrl, serialBytes, RESTOREINIT_BUFFER_SIZE);

            var statusCodeInt32 = (int)statusCode;
            var isSuccessStatusCode = (statusCodeInt32 >= 200) && (statusCodeInt32 <= 299);
            // OK?
            if (!isSuccessStatusCode)
            {
                if (statusCodeInt32 >= 500 && statusCodeInt32 < 600)
                {
                    throw new InvalidRestoreResponseException(
                        string.Format(
                            "No response from server ({0}). Perhaps maintainence?", statusCodeInt32));
                }
                else
                {
                    throw new InvalidRestoreResponseException(
                        string.Format(
                            "Error communicating with server: {0} - {1}", statusCodeInt32, statusCode));
                }
            }

            // check it is correct size
            if (challenge == null || challenge.Length != RESTOREINIT_BUFFER_SIZE)
            {
                throw new InvalidRestoreResponseException(string.Format("Invalid response data size (expected 32 got {0})", challenge?.Length));
            }
        }
#if DEBUG
#pragma warning disable CS0168 // 声明了变量，但从未使用过
        catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量，但从未使用过
#else
        catch
#endif
        {
            throw;
        }

        // only take the first 10 bytes of the restore code and encode to byte taking count of the missing chars
        var restoreCodeBytes = new byte[10];
        var arrayOfChar = restoreCode.AsSpan();
        for (var i = 0; i < 10; i++)
        {
            restoreCodeBytes[i] = ConvertRestoreCodeCharToByte(char.ToUpperInvariant(arrayOfChar[i]));
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
        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/BattleNetAuthenticator.cs#L642-L697
        byte[]? secretKey = null;
        try
        {
            var requestUrl = GetMobileUrl(serial, RESTOREVALIDATE_PATH);
            (var statusCode, secretKey) = await IAuthenticatorNetService.Instance.PostByteArrayAsync(requestUrl, postbytes, RESTOREVALIDATE_BUFFER_SIZE);

            var statusCodeInt32 = (int)statusCode;
            var isSuccessStatusCode = (statusCodeInt32 >= 200) && (statusCodeInt32 <= 299);
            // OK?
            if (!isSuccessStatusCode)
            {
                if (statusCodeInt32 >= 500 && statusCodeInt32 < 600)
                {
                    throw new InvalidRestoreResponseException(string.Format("No response from server ({0}). Perhaps maintainence?", statusCodeInt32));
                }
                else if (statusCodeInt32 >= 600 && statusCodeInt32 < 700)
                {
                    throw new InvalidRestoreCodeException("Invalid serial number or restore code.");
                }
                else
                {
                    throw new InvalidRestoreResponseException(string.Format("{0}: {1}", statusCodeInt32, statusCode));
                }
            }

            // check it is correct size
            if (secretKey == null || secretKey.Length != RESTOREVALIDATE_BUFFER_SIZE)
            {
                throw new InvalidRestoreResponseException(string.Format("Invalid response data size (expected " + RESTOREVALIDATE_BUFFER_SIZE + " got {0})", secretKey?.Length));
            }
        }
#if DEBUG
#pragma warning disable CS0168 // 声明了变量，但从未使用过
        catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量，但从未使用过
#else
        catch
#endif
        {
            throw;
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
            StringBuilder b = new();
            b.Append(serial[..2].ToUpperInvariant());
            b.Append('-');
            b.Append(serial.AsSpan(2, 4));
            b.Append('-');
            b.Append(serial.AsSpan(6, 4));
            b.Append('-');
            b.Append(serial.AsSpan(10, 4));
            //Serial = serial[..2].ToUpperInvariant() + "-" + serial.Substring(2, 4) + "-" + serial.Substring(6, 4) + "-" + serial.Substring(10, 4);
        }
        else
        {
            Serial = serial.ToUpperInvariant();
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
            writer.WriteString(/*bool.TrueString.ToLower()*/"true");
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// 计算验证器的还原代码，这是从序列号和密钥摘要的最后 10 个字节中提取的，然后将其特别编码为字母数字
    /// </summary>
    /// <returns>还原验证器代码(总是10个字符)</returns>
    string BuildRestoreCode()
    {
        // return if not set
        var serial = Serial;
        if (string.IsNullOrEmpty(serial) == true || SecretKey == null)
        {
            return string.Empty;
        }

        // get byte array of serial
        byte[] serialdata;
        var serial2 = ArrayPool<char>.Shared.Rent(serial.Length);
        try
        {
            var i2 = 0;
            for (int i = 0; i < serial.Length; i++)
            {
                var it = serial[i];
                if (it == '-')
                {
                    continue;
                }
                serial2[i2] = char.ToUpperInvariant(it);
                i2++;
            }
            serialdata = Encoding.UTF8.GetBytes(serial2, 0, i2 + 1);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(serial2);
        }
        //var serialdata = Encoding.UTF8.GetBytes(Serial.ToUpperInvariant().Replace("-", string.Empty));
        var secretdata = SecretKey;

        // combine serial data and secret data
        var combined = new byte[serialdata.Length + secretdata.Length];
        Array.Copy(serialdata, 0, combined, 0, serialdata.Length);
        Array.Copy(secretdata, 0, combined, serialdata.Length, secretdata.Length);

        // create digest of combined data
        var digest = new Sha1Digest();
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
    /// 创建一个随机的 Model 字符串用于初始化，以保护通过网络发送的 init 字符串
    /// </summary>
    /// <returns>随机模型串</returns>
    static string GeneralRandomModel()
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
    static byte ConvertRestoreCodeCharToByte(char c)
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
    /// 设置区域为国服
    /// </summary>
    public void SetRegionCN()
    {
        Region = REGION_CN;
    }

    /// <summary>
    /// 从序列号的前 2 个字符中获取验证者的区域，如果创建国服令牌，则需要指定值为 CN，否则使用默认值 US
    /// </summary>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string Region
    {
        get
        {
            if (field != null && field.Length == 2)
            {
                return field;
            }
            return REGION_US;
        }
        private set
        {
            // TODO: CN 区域应序列化保存值、且兼容旧数据
            field = value;
        }
    }

    protected override void WriteSecretData(StringBuilder b)
    {
        base.WriteSecretData(b);

        // for Battle.net, this is the key + serial
        b.Append('|');
        var serial = Serial.ThrowIsNull();
        b.Append(Convert.ToHexString(Encoding.UTF8.GetBytes(serial)));
    }

    protected override void SetSecretData(ReadOnlySpan<char> s)
    {
        if (s.Length == 0)
        {
            SecretKey = null;
            Serial = null;
            return;
        }

        var parts = s.Split('|');
        Range? r1 = null, r2 = null, r3 = null;
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
        //if (parts.Length <= 1)
        if (r1.HasValue && !r2.HasValue)
        {
            // old Authenticator2 version with secretdata + serial
            SecretKey = Convert.FromHexString(s[..40]);
            Serial = Encoding.UTF8.GetString(Convert.FromHexString(s[40..]));
        }
        //else if (parts.Length == 3) // alpha 3.0.6
        else if (r3.HasValue)
        {
            // secret|script|serial
            base.SetSecretData(s);
            //Serial = parts.Length > 2 ? Encoding.UTF8.GetString(StringToByteArray(parts[2])) : null;
            Serial = Encoding.UTF8.GetString(Convert.FromHexString(s[r2!.Value]));
        }
        else
        {
            // secret|serial
            base.SetSecretData(s);
            Serial = r1.HasValue ? Encoding.UTF8.GetString(Convert.FromHexString(s[r1.Value])) : null;
        }
    }

    ///// <inheritdoc/>
    //[IgnoreDataMember]
    //[global::MessagePack.IgnoreMember]
    //[global::Newtonsoft.Json.JsonIgnore]
    //[global::System.Text.Json.Serialization.JsonIgnore]
    //public override string? SecretData
    //{
    //    get
    //    {
    //        已由 WriteSecretData 重写实现
    //        // for Battle.net, this is the key + serial
    //        return base.SecretData + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(Serial.ThrowIsNull(nameof(Serial))));
    //    }

    //    set
    //    {
    //        // for Battle.net, extract key + serial
    //        if (!string.IsNullOrEmpty(value))
    //        {
    //            var parts = value.Split('|');
    //            if (parts.Length <= 1)
    //            {
    //                // old Authenticator2 version with secretdata + serial
    //                SecretKey = StringToByteArray(value[..40]);
    //                Serial = Encoding.UTF8.GetString(StringToByteArray(value[40..]));
    //            }
    //            else if (parts.Length == 3) // alpha 3.0.6
    //            {
    //                // secret|script|serial
    //                base.SecretData = value;
    //                Serial = parts.Length > 2 ? Encoding.UTF8.GetString(StringToByteArray(parts[2])) : null;
    //            }
    //            else
    //            {
    //                // secret|serial
    //                base.SecretData = value;
    //                Serial = parts.Length > 1 ? Encoding.UTF8.GetString(StringToByteArray(parts[1])) : null;
    //            }
    //        }
    //        else
    //        {
    //            SecretKey = null;
    //            Serial = null;
    //        }
    //    }
    //}

    /// <summary>
    /// 获取用于恢复丢失的身份验证器的身份验证器的还原代码以及序列号
    /// </summary>
    /// <returns>restore code (10 chars)</returns>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string RestoreCode
    {
        get
        {
            return BuildRestoreCode();
        }
    }
}
