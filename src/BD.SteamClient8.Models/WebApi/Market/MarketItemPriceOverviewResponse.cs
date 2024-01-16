#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 市场物品价格概述
/// </summary>
public record class MarketItemPriceOverviewResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 最低价格
    /// </summary>
    [SystemTextJsonProperty("lowest_price")]
    public string LowestPrice { get; set; } = string.Empty;

    /// <summary>
    /// 价格中位数
    /// </summary>
    [SystemTextJsonProperty("median_price")]
    public string MedianPrice { get; set; } = string.Empty;

    /// <summary>
    /// 成交量
    /// </summary>
    [SystemTextJsonProperty("volume")]
    public string Volume { get; set; } = string.Empty;

    // {
    //     "success": true,
    //     "lowest_price": "¥ 1.01",
    //     "volume": "2,606",
    //     "median_price": "¥ 1.02"
    // }
}
