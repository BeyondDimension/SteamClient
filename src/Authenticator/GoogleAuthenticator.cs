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

using BD.Common8.Extensions;
using BD.SteamClient8.WinAuth.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Net;
using System.Runtime.Serialization;

namespace WinAuth;

/// <summary>
/// 提供谷歌身份验证
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
public sealed class GoogleAuthenticator : TimeSync6AuthenticatorValueModel
{
    /// <summary>
    /// 创建一个新的 Authenticator 对象
    /// </summary>
    [global::MessagePack.SerializationConstructor]
    public GoogleAuthenticator() : base(CODE_DIGITS)
    {
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.Google;

    /// <inheritdoc/>
    protected override string[] GetTimeSyncUrls() => GoogleTimeSyncUrls.Values;
}

file static class GoogleTimeSyncUrls
{
    internal static readonly string[] Values =
    [
        "https://developer.android.google.cn",
        "https://www.google.com",
        "https://www.bing.com",
    ];
}