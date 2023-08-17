namespace BD.SteamClient;

public class MarketTradingHisotryRenderPageResponse
{
    [S_JsonProperty("success")]
    public bool Success { get; set; }

    [S_JsonProperty("pagesize")]
    public int PageSize { get; set; }

    [S_JsonProperty("total_count")]
    public int TotalCount { get; set; }

    [S_JsonProperty("assets")]
    public Dictionary<string, JsonElement>? Assets { get; set; }

    [S_JsonProperty("hovers")]
    public string? Hovers { get; set; }

    [S_JsonProperty("results_html")]
    public string ResultsHtml { get; set; } = string.Empty;
}

public record struct MarketTradingHisotryRenderItem
{
    public string HistoryRow { get; set; }

    public string MarketItemName { get; set; }

    public string GameName { get; set; }

    public string MarketItemImgUrl { get; set; }

    public string? ListingPrice { get; set; }

    public DateTime ListingDate { get; set; }

    public DateTime? TradingDate { get; set; }

    public string? TradingPartnerLabel { get; set; }

    public string? TradingPartnerAvatar { get; set; }

    public MarketTradingHisotryRowType RowType { get; set; }
}