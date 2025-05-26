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
* https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/MicrosoftAuthenticator.cs
*/

using BD.SteamClient8.WinAuth.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace WinAuth;

/// <summary>
/// 微软身份验证令牌
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
public sealed class MicrosoftAuthenticator : TimeSync6AuthenticatorValueModel
{
    /// <summary>
    /// 初始化 <see cref="MicrosoftAuthenticator"/> 类的新实例
    /// </summary>
    [global::MessagePack.SerializationConstructor]
    public MicrosoftAuthenticator() : base(CODE_DIGITS)
    {
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.Microsoft;

    /// <inheritdoc/>
    protected override string[] GetTimeSyncUrls() => MSTimeSyncUrls.Values;
}

file static class MSTimeSyncUrls
{
    internal static readonly string[] Values =
    [
        "https://login.live.com",
        "https://www.microsoft.com",
        "https://www.bing.com",
    ];
}