namespace BD.SteamClient8.Models.WebApi.Trades;

/// <summary>
/// 确认消息
/// </summary>
public sealed record class TradeConfirmation
{
    /// <summary>
    /// 确认类型
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    [JsonPropertyName("type_name")]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 确定消息 Id
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 创建 Id
    /// </summary>
    [JsonPropertyName("creator_id")]
    public string CreatorId { get; set; } = string.Empty;

    /// <summary>
    /// 随机数
    /// </summary>
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("creation_time")]
    public long CreationTime { get; set; }

    /// <summary>
    /// 取消
    /// </summary>
    [JsonPropertyName("cancel")]
    public string Cancel { get; set; } = string.Empty;

    /// <summary>
    /// 允许
    /// </summary>
    [JsonPropertyName("accept")]
    public string Accept { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 是否多个
    /// </summary>
    [JsonPropertyName("multi")]
    public bool Multi { get; set; }

    /// <summary>
    /// 令牌的标题或摘要
    /// </summary>
    [JsonPropertyName("headline")]
    public string Headline { get; set; } = string.Empty;

    /// <summary>
    /// 汇总
    /// </summary>
    [JsonPropertyName("summary")]
    public string[]? Summary { get; set; }

    /// <summary>
    /// 警告信息
    /// </summary>
    [JsonPropertyName("warn")]
    public string[]? Warn { get; set; }
}
