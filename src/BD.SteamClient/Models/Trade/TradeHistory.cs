namespace BD.SteamClient.Models.Trade;

using DecimalConverter = BD.SteamClient.Models.ModelConverters.DecimalConverter;

public class TradeHistory
{
    [S_JsonProperty("response")]
    public TradeHistoryResponseDetail? Response { get; set; }

    /// <summary>
    /// 交易历史记录响应详情
    /// </summary>
    public struct TradeHistoryResponseDetail
    {
        [S_JsonProperty("more")]
        public bool More { get; set; }

        [S_JsonProperty("trades")]
        public TradeItem[] Trades { get; set; }

        [S_JsonProperty("descriptions")]
        public TradeItemAssetItemDesc[]? Descriptions { get; set; }
    }

    /// <summary>
    /// 交易历史记录信息
    /// </summary>
    public struct TradeItem
    {
        [S_JsonProperty("tradeid")]
        public string TradeId { get; set; }

        [S_JsonProperty("steamid_other")]
        public string SteamIdOther { get; set; }

        [S_JsonProperty("time_init")]
        public long TimeInit { get; set; }

        [S_JsonProperty("status")]
        public int Status { get; set; }

        [S_JsonProperty("assets_given")]
        public TradeItemAssetItem[]? AssetsGiven { get; set; }

        [S_JsonProperty("assets_received")]
        public TradeItemAssetItem[]? AssetsReceived { get; set; }
    }

    /// <summary>
    /// 交易历史记录的交易项
    /// </summary>
    public record struct TradeItemAssetItem
    {
        [S_JsonProperty("appid")]
        public int AppId { get; set; }

        [S_JsonProperty("contextid")]
        public string ContextId { get; set; }

        [S_JsonProperty("assetid")]
        public string AssetId { get; set; }

        [JsonConverter(typeof(DecimalConverter))]
        [S_JsonProperty("amount")]
        public decimal Amount { get; set; }

        [S_JsonProperty("classid")]
        public string ClassId { get; set; }

        [S_JsonProperty("instanceid")]
        public string InstanceId { get; set; }

        [S_JsonProperty("new_assetid")]
        public string NewAssetId { get; set; }

        [S_JsonProperty("new_contextid")]
        public string NewContextId { get; set; }
    }

    /// <summary>
    /// 交易历史记录的交易项描述
    /// </summary>
    public record struct TradeItemAssetItemDesc
    {
        [S_JsonProperty("appid")]
        public int AppId { get; set; }

        [S_JsonProperty("classid")]
        public string ClassId { get; set; }

        [S_JsonProperty("instanceid")]
        public string InstanceId { get; set; }

        [S_JsonProperty("market_name")]
        public string MarketName { get; set; }

        [S_JsonProperty("market_hash_name")]
        public string MarketHashName { get; set; }
    }
}
