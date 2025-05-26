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
* https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/HOTPAuthenticator.cs
*/

using BD.SteamClient8.WinAuth.Enums;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace WinAuth;

/// <summary>
/// HOTP 身份认证
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
public sealed class HOTPAuthenticator : AuthenticatorValueModel
{
    /// <summary>
    /// 创建一个新的 Authenticator 对象
    /// </summary>
    [global::MessagePack.SerializationConstructor]
    public HOTPAuthenticator() : base(DEFAULT_CODE_DIGITS)
    {
    }

    /// <summary>
    /// 创建一个新的 Authenticator 对象，可选地使用指定的位数
    /// </summary>
    public HOTPAuthenticator(int digits) : base(digits)
    {
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public override AuthenticatorPlatform Platform => default;

    /// <summary>
    /// 用于验证器输入的计数器
    /// </summary>
    public long Counter { get; set; }

    protected override void WriteSecretData(StringBuilder b)
    {
        base.WriteSecretData(b);
        // this is the key |  serial | deviceid
        b.Append('|');
        b.Append(Counter);
    }

    protected override void SetSecretData(ReadOnlySpan<char> s)
    {
        if (s.Length == 0)
        {
            Counter = 0;
            SecretKey = null;
            return;
        }

        // extract key + counter
        base.SetSecretData(s);
        var parts = s.Split('|');
        if (parts.MoveNext())
        {
            if (long.TryParse(s[parts.Current], out var counter))
            {
                Counter = counter;
            }
            else
            {
                Counter = 0;
            }
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
    //        // this is the key |  serial | deviceid
    //        return base.SecretData
    //            + "|" + Counter.ToString();
    //    }

    //    set
    //    {
    //        !!!SetSecretData
    //        // extract key + counter
    //        if (string.IsNullOrEmpty(value) == false)
    //        {
    //            string[] parts = value.Split('|');
    //            base.SecretData = value;
    //            Counter = parts.Length > 1 ? long.Parse(parts[1]) : 0;
    //        }
    //        else
    //        {
    //            Counter = 0;
    //        }
    //    }
    //}

    /// <summary>
    /// 设置验证者的私钥
    /// </summary>
    /// <param name="b32key">base32 encoded key</param>
    /// <param name="counter"></param>
    public void Enroll(string b32key, long counter = 0)
    {
        SecretKey = Base32.GetInstance().Decode(b32key);
        Counter = counter;
    }

    /// <summary>
    /// 同步此身份验证器
    /// </summary>
    public override void Sync()
    {
    }

    /// <summary>
    /// 计算验证器的当前代码
    /// </summary>
    /// <returns>authenticator code</returns>
    public override string CalculateCode(bool sync = false, long counter = -1)
    {
        if (sync == true)
        {
            if (counter == -1)
            {
                throw new ArgumentException("counter must be >= 0");
            }

            // set as previous because we increment
            Counter = counter - 1;
        }

        var hmac = new HMac(new Sha1Digest());
        hmac.Init(new KeyParameter(SecretKey));

        // increment counter
        Counter++;

        byte[] codeIntervalArray = BitConverter.GetBytes(Counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(codeIntervalArray);
        }
        hmac.BlockUpdate(codeIntervalArray, 0, codeIntervalArray.Length);

        byte[] mac = new byte[hmac.GetMacSize()];
        hmac.DoFinal(mac, 0);

        // the last 4 bits of the mac say where the code starts (e.g. if last 4 bit are 1100, we start at byte 12)
        int start = mac[19] & 0x0f;

        // extract those 4 bytes
        byte[] bytes = new byte[4];
        Array.Copy(mac, start, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        uint fullcode = BitConverter.ToUInt32(bytes, 0) & 0x7fffffff;

        // we use the last 8 digits of this code in radix 10
        uint codemask = (uint)Math.Pow(10, CodeDigits);
        string format = new string('0', CodeDigits);
        string code = (fullcode % codemask).ToString(format);

        return code;
    }
}
