namespace BD.SteamClient8.Models.WebApi.Markets;

/// <summary>
/// 市场订单柱状图数据
/// </summary>
public sealed record class MarketItemOrdersHistogramResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public int Success { get; set; }

    /// <summary>
    /// 出售订单Html table 标签字符串
    /// </summary>
    [SystemTextJsonProperty("sell_order_table")]
    public string SellOrderTable { get; set; } = string.Empty;

    /// <summary>
    /// 出售订单简介
    /// </summary>
    [SystemTextJsonProperty("sell_order_summary")]
    public string SellOrderSummary { get; set; } = string.Empty;

    /// <summary>
    /// 订购单 Html table 标签字符串
    /// </summary>
    [SystemTextJsonProperty("buy_order_table")]
    public string BuyOrderTable { get; set; } = string.Empty;

    /// <summary>
    /// 订购单简介
    /// </summary>
    [SystemTextJsonProperty("buy_order_summary")]
    public string BuyOrderSummary { get; set; } = string.Empty;

    /// <summary>
    /// 最高订购单价格(单位分)
    /// </summary>
    [SystemTextJsonProperty("highest_buy_order")]
    public string HighestBuyOrder { get; set; } = string.Empty;

    /// <summary>
    /// 最低出售价格(单位分)
    /// </summary>
    [SystemTextJsonProperty("lowest_sell_order")]
    public string LowestSellOrder { get; set; } = string.Empty;

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
    [SystemTextJsonProperty("buy_order_graph")]
    public JsonArray? BuyOrderGraph { get; set; }

    /// <summary>
    /// 出售单图表
    /// </summary>
    [SystemTextJsonProperty("sell_order_graph")]
    public JsonArray? SellOrderGraph { get; set; }

    /// <summary>
    /// 图表x最大值
    /// </summary>
    [SystemTextJsonProperty("graph_max_x")]
    public decimal GraphMaxX { get; set; }

    /// <summary>
    /// 图标y最大值
    /// </summary>
    [SystemTextJsonProperty("graph_max_y")]
    public decimal GraphMaxY { get; set; }

    /// <summary>
    /// 图表x最小值
    /// </summary>
    [SystemTextJsonProperty("graph_min_x")]
    public decimal GraphMinX { get; set; }

    /// <summary>
    /// 价格前缀
    /// </summary>
    [SystemTextJsonProperty("price_prefix")]
    public string PricePrefix { get; set; } = string.Empty;

    /// <summary>
    /// 价格后缀
    /// </summary>
    [SystemTextJsonProperty("price_suffix")]
    public string PriceSuffix { get; set; } = string.Empty;
}
