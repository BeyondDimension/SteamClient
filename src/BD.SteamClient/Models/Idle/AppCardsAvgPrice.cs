namespace BD.SteamClient.Models.Idle;

public class AppCardsAvgPrice
{
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
