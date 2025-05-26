using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Trades;

/// <summary>
/// 确认消息
/// </summary>
public sealed record class TradeConfirmation : JsonRecordModel<TradeConfirmation>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 确认类型
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("type_name")]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 确定消息 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 创建 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("creator_id")]
    public string CreatorId { get; set; } = string.Empty;

    /// <summary>
    /// 随机数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("creation_time")]
    public long CreationTime { get; set; }

    /// <summary>
    /// 取消
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("cancel")]
    public string Cancel { get; set; } = string.Empty;

    /// <summary>
    /// 允许
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("accept")]
    public string Accept { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 是否多个
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("multi")]
    public bool Multi { get; set; }

    /// <summary>
    /// 令牌的标题或摘要
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("headline")]
    public string Headline { get; set; } = string.Empty;

    /// <summary>
    /// 汇总
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("summary")]
    public string[]? Summary { get; set; }

    /// <summary>
    /// 警告信息
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("warn")]
    public string[]? Warn { get; set; }
}
