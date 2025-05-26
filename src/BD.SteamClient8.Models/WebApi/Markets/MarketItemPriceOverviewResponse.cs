using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Markets;

/// <summary>
/// 市场物品价格概述
/// </summary>
public sealed record class MarketItemPriceOverviewResponse : JsonRecordModel<MarketItemPriceOverviewResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 最低价格
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("lowest_price")]
    public string LowestPrice { get; set; } = string.Empty;

    /// <summary>
    /// 价格中位数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("median_price")]
    public string MedianPrice { get; set; } = string.Empty;

    /// <summary>
    /// 成交量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("volume")]
    public string Volume { get; set; } = string.Empty;

    // {
    //     "success": true,
    //     "lowest_price": "¥ 1.01",
    //     "volume": "2,606",
    //     "median_price": "¥ 1.02"
    // }
}
