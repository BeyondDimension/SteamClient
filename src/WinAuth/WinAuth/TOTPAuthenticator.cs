namespace WinAuth.WinAuth;

#pragma warning disable SA1600 // Elements should be documented

public class TOTPAuthenticator : GoogleAuthenticator
{
    /// <summary>
    /// Create a new Authenticator object
    /// </summary>
    [SerializationConstructor]
    public TOTPAuthenticator() : base()
    {
    }

    [IgnoreDataMember]
    [MPIgnore]
#if __HAVE_N_JSON__
    [NewtonsoftJsonIgnore]
#endif
#if !__NOT_HAVE_S_JSON__
    [SystemTextJsonIgnore]
#endif
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.TOTP;

    public override void Sync()
    {
    }
}
