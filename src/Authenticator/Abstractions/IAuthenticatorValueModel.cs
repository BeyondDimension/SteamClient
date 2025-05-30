using BD.Common8.Models.Abstractions;
using BD.SteamClient8.WinAuth.Enums;
using BD.SteamClient8.WinAuth.Models.Abstractions;
using System.Xml;

namespace WinAuth.Abstractions;

/// <summary>
/// 身份验证器(游戏平台令牌)数据值模型
/// </summary>
[global::MessagePack.Union((int)AuthenticatorPlatform.BattleNet, typeof(BattleNetAuthenticator))]
[global::MessagePack.Union((int)AuthenticatorPlatform.Google, typeof(GoogleAuthenticator))]
[global::MessagePack.Union((int)AuthenticatorPlatform.TOTP, typeof(TOTPAuthenticator))]
[global::MessagePack.Union((int)AuthenticatorPlatform.HOTP, typeof(HOTPAuthenticator))]
[global::MessagePack.Union((int)AuthenticatorPlatform.Microsoft, typeof(MicrosoftAuthenticator))]
[global::MessagePack.Union((int)AuthenticatorPlatform.Steam, typeof(SteamAuthenticator))]
public partial interface IAuthenticatorValueModel : IAuthenticatorValueModelBase
{
}
