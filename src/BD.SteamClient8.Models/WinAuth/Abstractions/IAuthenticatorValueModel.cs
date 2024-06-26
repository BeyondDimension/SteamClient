namespace BD.SteamClient8.Models.WinAuth.Abstractions;

/// <summary>
/// 身份验证器(游戏平台令牌)数据值模型
/// </summary>
[MPUnion((int)AuthenticatorPlatform.BattleNet, typeof(BattleNetAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.Google, typeof(GoogleAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.TOTP, typeof(TOTPAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.HOTP, typeof(HOTPAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.Microsoft, typeof(MicrosoftAuthenticator))]
[MPUnion((int)AuthenticatorPlatform.Steam, typeof(SteamAuthenticator))]
public partial interface IAuthenticatorValueModel : IExplicitHasValue
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
    /// 用于 OTP 生成的哈希算法（默认为 SHA1）
    /// </summary>
    HMACTypes HMACType { get; set; }

    /// <summary>
    /// 下一个代码的周期（秒）
    /// </summary>
    int Period { get; set; }

    /// <summary>
    /// 使用当前加密和/或密码保存的当前数据（可能为无）
    /// </summary>
    string? EncryptedData { get; set; }

    /// <summary>
    /// 服务器时间戳
    /// </summary>
    long ServerTime { get; }

    /// <summary>
    /// 用于加密机密数据的密码类型
    /// </summary>
    public enum PasswordTypes
    {
        None = 0,
        Explicit = 1,
        User = 2,
        Machine = 4,
        YubiKeySlot1 = 8,
        YubiKeySlot2 = 16,
    }

    /// <summary>
    /// 用于加密机密数据的密码类型
    /// </summary>
    PasswordTypes PasswordType { get; set; }

    /// <summary>
    /// 是否必须密码
    /// </summary>
    bool RequiresPassword { get; }

    /// <summary>
    /// 发行者名称
    /// </summary>
    string? Issuer { get; }

    /// <summary>
    /// 获取/设置组合的机密数据值
    /// </summary>
    string? SecretData { get; set; }

    /// <summary>
    /// 根据计算的服务器时间计算代码间隔
    /// </summary>
    long CodeInterval { get; }

    /// <summary>
    /// 获取验证器的当前代码
    /// </summary>
    string CurrentCode { get; }

    /// <summary>
    /// 将此验证器的时间与服务器时间同步，我们用 UTC 时间的差值更新数据记录
    /// </summary>
    void Sync();

    /// <summary>
    /// 令牌保护
    /// </summary>
    void Protect();

    /// <summary>
    /// 取消令牌保护
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    bool Unprotect(string? password);

    /// <summary>
    /// Xml 内容读取
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    bool ReadXml(XmlReader reader, string? password = null);
}
