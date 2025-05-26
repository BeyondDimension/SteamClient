using BD.Common8.Models.Abstractions;
using BD.SteamClient8.WinAuth.Enums;
using System.Diagnostics.CodeAnalysis;
using WinAuth.Abstractions;

namespace WinAuth;

/// <inheritdoc cref="IAuthenticatorModel"/>
[global::MessagePack.MessagePackObject(keyAsPropertyName: true)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed partial class AuthenticatorModel : IAuthenticatorModel
{
    /// <summary>
    /// 当前项目使用的可回收内存流管理器
    /// </summary>
    internal static readonly global::Microsoft.IO.RecyclableMemoryStreamManager Manager = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorModel"/> class.
    /// </summary>
    public AuthenticatorModel()
    {
        Value = new AuthenticatorValueModel();
    }

    /// <summary>
    /// 唯一标识符
    /// </summary>
    [global::MemoryPack.MemoryPackIgnore, global::Newtonsoft.Json.JsonIgnore, global::System.Text.Json.Serialization.JsonIgnore]
    public ushort Id { get; set; }

    /// <summary>
    /// 索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 验证器名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 身份验证器平台
    /// </summary>
    [global::MemoryPack.MemoryPackIgnore, global::Newtonsoft.Json.JsonIgnore, global::System.Text.Json.Serialization.JsonIgnore]
    public AuthenticatorPlatform Platform => Value == null ? default : Value.Platform;

    /// <summary>
    /// 服务器唯一标识
    /// </summary>
    public Guid? ServerId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTimeOffset LastUpdate { get; set; }

    /// <summary>
    /// 身份验证器 ValueDto
    /// </summary>
    public IAuthenticatorValueModel Value { get; set; }

    /// <summary>
    /// 该验证器是否含有内容
    /// </summary>
    /// <returns></returns>
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return !string.IsNullOrEmpty(Name) && Value != null;
    }
}
