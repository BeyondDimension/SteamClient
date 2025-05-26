using BD.SteamClient8.WinAuth.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace WinAuth;

/// <summary>
/// TOTP 身份验证
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
public sealed class TOTPAuthenticator : TimeSync6AuthenticatorValueModel
{
    /// <summary>
    /// 初始化 <see cref="TOTPAuthenticator"/> 类的新实例
    /// </summary>
    [global::MessagePack.SerializationConstructor]
    public TOTPAuthenticator() : base(CODE_DIGITS)
    {
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [global::MessagePack.IgnoreMember]
    [global::System.Text.Json.Serialization.JsonIgnore]
    [global::Newtonsoft.Json.JsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.TOTP;

    /// <inheritdoc/>
    protected override string[] GetTimeSyncUrls()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void Sync()
    {
    }
}
