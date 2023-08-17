namespace BD.SteamClient.Models;

/// <summary>
/// 卡片
/// </summary>
public class SteamCard
{
    public string Name { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public decimal Price { get; set; }

    /// <summary>
    /// 是否已收集
    /// </summary>
    public bool IsCollected { get; set; }
}
