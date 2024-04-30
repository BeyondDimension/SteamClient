namespace BD.SteamClient8.Models.WebApi.Trades;

/// <summary>
/// 库存资产
/// </summary>
public sealed record class TradeAsset
{
    /// <summary>
    /// 游戏 AppId
    /// </summary>
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    /// <summary>
    /// 上下文 Id
    /// </summary>
    [JsonPropertyName("contextid")]
    public string ContextId { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// 资产 Id
    /// </summary>
    [JsonPropertyName("assetid")]
    public string AssetId { get; set; } = string.Empty;
}
