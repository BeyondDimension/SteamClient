namespace BD.SteamClient.Models.Trade;

public class TradeItem
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("contextid")]
    public string ContextId { get; set; } = string.Empty;

    [JsonPropertyName("assetid")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("classid")]
    public string ClassId { get; set; } = string.Empty;

    [JsonPropertyName("instanceid")]
    public string InstanceId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("missing")]
    public bool Missing { get; set; }

    [JsonPropertyName("est_usd")]
    public string EstUsd { get; set; } = string.Empty;
}