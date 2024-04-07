namespace BD.SteamClient8.Models;

/// <summary>
/// 卡片
/// </summary>
public record class SteamCard
{
    /// <summary>
    /// 卡片名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 卡片图片 Url
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 是否已收集
    /// </summary>
    public bool IsCollected { get; set; }
}
