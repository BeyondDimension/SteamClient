#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

// https://github.com/IsThereAnyDeal/AugmentedSteam_Server/blob/master/src/Controllers/MarketController.php

/// <summary>
/// Steam 游戏卡片市场价格
/// </summary>
public record class CardsMarketPrice
{
    /// <summary>
    /// 卡片名称
    /// </summary>
    public string CardName { get; set; } = string.Empty;

    /// <summary>
    /// 是否闪亮卡牌
    /// </summary>
    public bool IsFoil { get; set; }

    /// <summary>
    /// 市场上架 Url
    /// </summary>
    public string MarketUrl { get; set; } = string.Empty;

    /// <summary>
    /// 价格
    /// </summary>
    public decimal Price { get; set; }
}
