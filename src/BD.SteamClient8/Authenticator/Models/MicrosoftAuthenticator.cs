#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
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