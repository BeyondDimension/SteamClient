using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Logins;

/// <summary>
/// 完成登录状态
/// </summary>
public sealed class FinalizeLoginStatus : JsonModel<FinalizeLoginStatus>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// SteamId
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("steamID")]
    public string? SteamId { get; set; }

    /// <summary>
    /// 重定向地址
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("redir")]
    public string? Redir { get; set; }

    /// <summary>
    /// 会话信息
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("transfer_info")]
    public List<TransferInfo>? TransferInfo { get; set; }

    /// <summary>
    /// 主要域名
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("primary_domain")]
    public string? PrimaryDomain { get; set; }
}