namespace BD.SteamClient;

public struct MarketItemPriceOverviewResponse
{
    [S_JsonProperty("success")]
    public bool Success { get; set; }

    [S_JsonProperty("lowest_price")]
    public string LowestPrice { get; set; }

    [S_JsonProperty("median_price")]
    public string MedianPrice { get; set; }

    [S_JsonProperty("volume")]
    public string Volume { get; set; }

    // {
    //     "success": true,
    //     "lowest_price": "¥ 1.01",
    //     "volume": "2,606",
    //     "median_price": "¥ 1.02"
    // }
}
