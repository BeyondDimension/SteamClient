namespace BD.SteamClient.Models.Idle;

/// <summary>
/// 徽章
/// </summary>
public class Badge
{
    public string BadgeName { get; set; } = string.Empty;

    public int AppId { get; set; }

    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 游戏时长
    /// </summary>
    public double MinutesPlayed { get; set; }

    /// <summary>
    /// 剩余可掉落卡片数量
    /// </summary>
    public int CardsRemaining { get; set; }

    /// <summary>
    /// 常规卡片平均价格
    /// </summary>
    public decimal RegularAvgPrice { get; set; }

    /// <summary>
    /// 闪亮卡片平均价格
    /// </summary>
    public decimal FoilAvgPrice { get; set; }

    /// <summary>
    /// 徽章卡组
    /// </summary>
    public List<SteamCard>? Cards { get; set; }
}