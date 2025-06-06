using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Constants;
using System.Extensions;
using System.Net;

namespace BD.SteamClient8.Models.WebApi.Logins;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class SteamSession : JsonModel<SteamSession>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// SteamId
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0), global::System.Text.Json.Serialization.JsonPropertyName("steamid")]
    public string SteamId { get; set; } = string.Empty;

    /// <summary>
    /// 登录 Token
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1), global::System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新 Token 所需密钥
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2), global::System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Cookie 容器
    /// </summary>
#if !NETFRAMEWORK && !(NETSTANDARD && !NETSTANDARD2_1_OR_GREATER)
    [global::MemoryPack.MemoryPackIgnore]
#endif
#if !(NETFRAMEWORK && !NET462_OR_GREATER) && !(NETSTANDARD && !NETSTANDARD2_0_OR_GREATER)
    [global::System.Text.Json.Serialization.JsonIgnore]
#endif
    public CookieCollection? Cookies
    {
        get => SteamLoginState.GetCookieCollection(field, AccessToken, SteamId);
        set => field = value;
    }

    /// <summary>
    /// 聊天消息队列唯一标识符
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string UmqId { get; set; } = string.Empty;

    /// <summary>
    /// 聊天消息唯一标识符
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// 用户 APIKey
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public string APIKey { get; set; } = string.Empty;

    /// <summary>
    /// 令牌认证密钥
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public string IdentitySecret { get; set; } = string.Empty;

    /// <summary>
    /// 与 Steam 通用时间的间隔
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(7)]
    public long ServerTimeDiff { get; set; }

    /// <summary>
    /// 存储用户登录状态的 httpClient
    /// </summary>
#if !NETFRAMEWORK && !(NETSTANDARD && !NETSTANDARD2_1_OR_GREATER)
    [global::MemoryPack.MemoryPackIgnore]
#endif
#if !(NETFRAMEWORK && !NET462_OR_GREATER) && !(NETSTANDARD && !NETSTANDARD2_0_OR_GREATER)
    [global::System.Text.Json.Serialization.JsonIgnore]
#endif
    public HttpClient? HttpClient { get; set; }
}
