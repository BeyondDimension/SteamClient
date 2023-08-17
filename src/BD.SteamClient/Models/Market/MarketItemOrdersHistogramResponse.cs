namespace BD.SteamClient;

public struct MarketItemOrdersHistogramResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [S_JsonProperty("success")]
    public int Success { get; set; }

    /// <summary>
    /// 出售订单Html table 标签字符串
    /// </summary>
    [S_JsonProperty("sell_order_table")]
    public string SellOrderTable { get; set; }

    /// <summary>
    /// 出售订单简介
    /// </summary>
    [S_JsonProperty("sell_order_summary")]
    public string SellOrderSummary { get; set; }

    /// <summary>
    /// 订购单Html table 标签字符串
    /// </summary>
    [S_JsonProperty("buy_order_table")]
    public string BuyOrderTable { get; set; }

    /// <summary>
    /// 订购单简介
    /// </summary>
    [S_JsonProperty("buy_order_summary")]
    public string BuyOrderSummary { get; set; }

    /// <summary>
    /// 最高订购单价格(单位分)
    /// </summary>
    [S_JsonProperty("highest_buy_order")]
    public string HighestBuyOrder { get; set; }

    /// <summary>
    /// 最低出售价格(单位分)
    /// </summary>
    [S_JsonProperty("lowest_sell_order")]
    public string LowestSellOrder { get; set; }

    // [
    //     [
    //         1,
    //         19,
    //         "19 份 ¥ 1.00 以上的订购单"
    //     ],
    //     [
    //         0.99,
    //         769,
    //         "769 份 ¥ 0.99 以上的订购单"
    //     ]
    // ]

    /// <summary>
    /// 订购单图表
    /// </summary>
    [S_JsonProperty("buy_order_graph")]
    public JsonArray BuyOrderGraph { get; set; }

    /// <summary>
    /// 出售单图表
    /// </summary>
    [S_JsonProperty("sell_order_graph")]
    public JsonArray SellOrderGraph { get; set; }

    /// <summary>
    /// 图表x最大值
    /// </summary>
    [S_JsonProperty("graph_max_x")]
    public decimal GraphMaxX { get; set; }

    /// <summary>
    /// 图标y最大值
    /// </summary>
    [S_JsonProperty("graph_max_y")]
    public decimal GraphMaxY { get; set; }

    /// <summary>
    /// 图表x最小值
    /// </summary>
    [S_JsonProperty("graph_min_x")]
    public decimal GraphMinX { get; set; }

    /// <summary>
    /// 价格前缀
    /// </summary>
    [S_JsonProperty("price_prefix")]
    public string PricePrefix { get; set; }

    /// <summary>
    /// 价格后缀
    /// </summary>
    [S_JsonProperty("price_suffix")]
    public string PriceSuffix { get; set; }
}
