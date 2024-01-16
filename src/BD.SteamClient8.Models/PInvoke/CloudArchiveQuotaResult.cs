#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 获得可用的字节数返回结果
/// </summary>
public sealed record class CloudArchiveQuotaResult
{
    /// <summary>
    /// 返回用户可访问的字节总量
    /// </summary>
    public ulong TotalBytes { get; set; }

    /// <summary>
    /// 返回可用的字节数
    /// </summary>
    public ulong AvailableBytes { get; set; }
}
