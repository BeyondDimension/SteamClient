#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
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
