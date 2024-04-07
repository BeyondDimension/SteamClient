namespace BD.SteamClient8.Models;

/// <summary>
/// 微软身份验证令牌
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[MessagePackObject(keyAsPropertyName: true)]
public sealed class MicrosoftAuthenticator : GoogleAuthenticator
{
    /// <summary>
    /// 初始化 <see cref="MicrosoftAuthenticator"/> 类的新实例
    /// </summary>
    [SerializationConstructor]
    public MicrosoftAuthenticator() : base()
    {
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.Microsoft;
}