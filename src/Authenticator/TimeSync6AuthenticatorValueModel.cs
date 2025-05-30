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
* https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/GoogleAuthenticator.cs
*/

using BD.SteamClient8.WinAuth.Enums;
using BD.SteamClient8.WinAuth.Models.Abstractions;
using BD.SteamClient8.WinAuth.Services.Abstractions;
using System.Extensions;
using System.Runtime.Serialization;

namespace WinAuth;

public abstract partial class TimeSync6AuthenticatorValueModel : AuthenticatorValueModel
{
    /// <summary>
    /// 代码中的位数
    /// </summary>
    protected const int CODE_DIGITS = 6;

    /// <summary>
    /// 创建一个新的 Authenticator 对象
    /// </summary>
    public TimeSync6AuthenticatorValueModel() : this(CODE_DIGITS)
    {
    }

    /// <summary>
    /// 创建一个新的 Authenticator 对象
    /// </summary>
    public TimeSync6AuthenticatorValueModel(
        int codeDigits = DEFAULT_CODE_DIGITS,
        HMACTypes hmacType = DEFAULT_HMACTYPE,
        int period = DEFAULT_PERIOD) : base(CODE_DIGITS, hmacType, period)
    {
    }

    /// <summary>
    /// 如果网络错误，忽略同步的分钟数
    /// </summary>
    const int SYNC_ERROR_MINUTES = 5;

    /// <summary>
    /// 上次同步时间错误
    /// </summary>
    static DateTime _lastSyncError = DateTime.MinValue;

    /// <summary>
    /// 获取序列号
    /// </summary>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string Serial
    {
        get
        {
            var result = Base32.GetInstance().Encode(SecretKey.ThrowIsNull(nameof(SecretKey)));
            return result;
        }
    }

    /// <summary>
    /// 向服务器注册身份验证器
    /// </summary>
    /// <param name="b32key"></param>
    public void Enroll(string b32key)
    {
        SecretKey = Base32.GetInstance().Decode(b32key);
        Sync();
    }

    /// <summary>
    /// 获取多个用于同步时间的 Url
    /// </summary>
    /// <returns></returns>
    protected abstract string[] GetTimeSyncUrls();

    /// <summary>
    /// 将验证者的时间与 Google 同步，我们用与 UTC 时间的差异来更新数据记录
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

        // https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/GoogleAuthenticator.cs#L118-L163
        // https://github.com/BeyondDimension/WinAuth/blob/main/src/WinAuth/WinAuth/GoogleAuthenticator.cs#L101-L139
        try
        {
            // we use the Header response field from a request to www.google.come
            var timeSyncUrls = GetTimeSyncUrls();
            var (statusCode, date) = await IAuthenticatorNetService.Instance.GetDateTimeAsync(timeSyncUrls);

            var statusCodeInt32 = (int)statusCode;
            var isSuccessStatusCode = (statusCodeInt32 >= 200) && (statusCodeInt32 <= 299);
            // OK?
            if (!isSuccessStatusCode)
            {
                throw new ApplicationException(string.Format("{0}: {1}", statusCodeInt32, statusCode));
            }

            if (date.HasValue)
            {
                // get as ms since epoch
                var dtms = date.Value.ToUnixTimeMilliseconds();

                // get the difference between the server time and our current time
                var serverTimeDiff = dtms - IAuthenticatorValueModelBase.CurrentTime;

                // update the Data object
                ServerTimeDiff = serverTimeDiff;
                LastServerTime = DateTime.Now.Ticks;
            }

            // clear any sync error
            _lastSyncError = DateTime.MinValue;
        }
        catch
        {
            // don't retry for a while after error
            _lastSyncError = DateTime.Now;

            // set to zero to force reset
            ServerTimeDiff = 0;
        }
    }
}
