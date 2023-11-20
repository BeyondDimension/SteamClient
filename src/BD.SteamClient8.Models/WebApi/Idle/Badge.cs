namespace BD.SteamClient8.Models.WebApi.Idle;

/// <summary>
/// 徽章
/// </summary>
public record class Badge
{
    /// <summary>
    /// 徽章名称
    /// </summary>
    public string BadgeName { get; set; } = string.Empty;

    /// <summary>
    /// 徽章图片 Url
    /// </summary>
    public string BadgeImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 游戏 AppId
    /// </summary>
    public uint AppId { get; set; }

    /// <summary>
    /// 游戏名称
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 徽章等级
    /// </summary>
    public byte BadgeLevel { get; set; }

    /// <summary>
    /// 徽章当前经验
    /// </summary>
    public short BadgeCurrentExp { get; set; }

    /// <summary>
    /// 游戏时长
    /// </summary>
    public double MinutesPlayed { get; set; }

    /// <summary>
    /// 剩余可掉落卡片数量
    /// </summary>
    public int CardsRemaining { get; set; }

    /// <summary>
    /// 已收集的卡片数量
    /// </summary>
    public int CardsCollected { get; set; }

    /// <summary>
    /// 集齐卡组所需卡片数量
    /// </summary>
    public int CardsGathering { get; set; }

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