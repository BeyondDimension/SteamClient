namespace BD.SteamClient8.Enums;

/// <summary>
/// 令牌平台类型
/// </summary>
public enum AuthenticatorPlatform : byte
{
    Steam = 1,

    Microsoft = 5,

    BattleNet = 11,

    Google = 12,

    HOTP = 13,

    TOTP = 14,
}