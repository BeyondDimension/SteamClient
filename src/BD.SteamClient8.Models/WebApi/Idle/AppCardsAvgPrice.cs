#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

// https://github.com/IsThereAnyDeal/AugmentedSteam_Server/blob/master/src/Controllers/MarketController.php

/// <summary>
/// Steam 游戏卡片平均价格
/// </summary>
public record class AppCardsAvgPrice
{
    /// <summary>
    /// 游戏 AppId
    /// </summary>
    public uint AppId { get; set; }

    /// <summary>
    /// 常规卡片平均价格
    /// </summary>
    public decimal Regular { get; set; }

    /// <summary>
    /// 闪亮卡片平均价格
    /// </summary>
    public decimal Foil { get; set; }
}
