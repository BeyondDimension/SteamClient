#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models.Abstractions;

/// <summary>
/// 身份验证器(游戏平台令牌)数据值可传输模型
/// </summary>
[MPUnion((int)AuthenticatorPlatform.BattleNet, typeof(BattleNetAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.Google, typeof(GoogleAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.TOTP, typeof(TOTPAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.HOTP, typeof(HOTPAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.Microsoft, typeof(MicrosoftAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.Steam, typeof(SteamAuthenticator))]
public partial interface IAuthenticatorValueDTO : IExplicitHasValue
{
    /// <summary>
    /// 身份验证器平台
    /// </summary>
    AuthenticatorPlatform Platform { get; }

    /// <summary>
    /// 本地机器和服务器的时间差（毫秒 ms）
    /// </summary>
    long ServerTimeDiff { get; set; }

    /// <summary>
    /// 上次同步时间
    /// </summary>
    long LastServerTime { get; set; }

    /// <summary>
    /// 用于身份验证器的密钥
    /// </summary>
    byte[]? SecretKey { get; set; }

    /// <summary>
    /// 代码中返回的位数（默认为 6）
    /// </summary>
    int CodeDigits { get; set; }

    /// <summary>
    /// 用于 OTP 生成的哈希算法（默认为SHA1）
    /// </summary>
    HMACTypes HMACType { get; set; }

    /// <summary>
    /// 下一个代码的周期（秒）
    /// </summary>
    int Period { get; set; }
}

[Obsolete("use IAuthenticatorValueDTO", true)]
#pragma warning disable SA1600 // Elements should be documented
public interface IGAPAuthenticatorValueDTO : IAuthenticatorValueDTO
#pragma warning restore SA1600 // Elements should be documented
{
}
