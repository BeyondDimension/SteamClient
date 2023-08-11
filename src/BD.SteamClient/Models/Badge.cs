namespace BD.SteamClient.Models;

/// <summary>
/// 徽章
/// </summary>
public class Badge
{
    public string BadgeName { get; set; } = string.Empty;

    public int AppId { get; set; }

    public string AppName { get; set; } = string.Empty;

    public double MinutesPlayed { get; set; }

    /// <summary>
    /// 剩余可掉落卡片数量
    /// </summary>
    public int CardsRemaining { get; set; }

    /// <summary>
    /// 徽章卡组
    /// </summary>
    public List<SteamCard>? Cards { get; set; }
}