namespace WinAuth.WinAuth;

/// <summary>
/// TOTP 身份验证
/// </summary>
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
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.TOTP;

    /// <inheritdoc/>
    public override void Sync()
    {
    }
}
