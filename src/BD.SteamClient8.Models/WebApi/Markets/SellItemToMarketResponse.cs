using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Markets;

/// <summary>
/// 市场出售物品返回
/// </summary>
public sealed record class SellItemToMarketResponse : JsonRecordModel<SellItemToMarketResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 需要手机确认
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("needs_mobile_confirmation")]
    public bool NeedsMobileConfirmation { get; set; }

    /// <summary>
    /// 需要邮箱确认
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("needs_email_confirmation")]
    public bool NeedsEmailConfirmationConfirmed { get; set; }

    /// <summary>
    /// 邮件域名
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("email_domain")]
    public string EmailDomain { get; set; } = string.Empty;

    /// <summary>
    /// 需要确认
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("requires_confirmation")]
    public int RequiresConfirmation { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 信息
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("message")]
    public string? Message { get; set; }
}