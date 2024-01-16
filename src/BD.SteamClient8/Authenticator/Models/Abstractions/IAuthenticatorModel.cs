#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models.Abstractions;

/// <summary>
/// 身份验证器(游戏平台令牌)模型
/// </summary>
public interface IAuthenticatorModel : IOrderAuthenticator, IExplicitHasValue
{
    /// <summary>
    /// 名称最大长度
    /// </summary>
    const int MaxLength_Name = 32;

    /// <summary>
    /// 名称
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 令牌平添
    /// </summary>
    AuthenticatorPlatform Platform { get; }

    /// <summary>
    /// 服务器唯一标识
    /// </summary>
    Guid? ServerId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTimeOffset LastUpdate { get; set; }

    /// <inheritdoc cref="IAuthenticatorValueModel"/>
    IAuthenticatorValueModel Value { get; set; }
}
