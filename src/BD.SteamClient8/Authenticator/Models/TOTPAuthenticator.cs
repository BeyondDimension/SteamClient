#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

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
