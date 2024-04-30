namespace BD.SteamClient8.Models.PInvoke;

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
