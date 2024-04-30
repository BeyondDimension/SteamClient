namespace BD.SteamClient8.Models.WinAuth;

/// <summary>
/// TOTP 身份验证
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[MessagePackObject(keyAsPropertyName: true)]
public class TOTPAuthenticator : GoogleAuthenticator
{
    /// <summary>
    /// 初始化 <see cref="TOTPAuthenticator"/> 类的新实例
    /// </summary>
    [SerializationConstructor]
    public TOTPAuthenticator() : base()
    {
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [SystemTextJsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.TOTP;

    /// <inheritdoc/>
    public override void Sync()
    {
    }
}
