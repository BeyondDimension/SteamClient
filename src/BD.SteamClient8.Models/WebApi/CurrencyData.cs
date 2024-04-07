namespace BD.SteamClient8.Models;

/// <summary>
/// 货币信息
/// </summary>
public sealed record class CurrencyData
{
    /// <summary>
    /// 展示
    /// </summary>
    public string? Display { get; set; }

    /// <summary>
    /// 货币 ISO Code
    /// </summary>
    public string? CurrencyCode { get; set; }
}
