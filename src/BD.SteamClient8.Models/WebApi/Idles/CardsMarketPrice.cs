namespace BD.SteamClient8.Models.WebApi.Idles;

/// <summary>
/// Steam 游戏卡片市场价格
/// <para>https://github.com/IsThereAnyDeal/AugmentedSteam_Server/blob/master/src/Controllers/MarketController.php</para>
/// </summary>
public sealed record class CardsMarketPrice
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
