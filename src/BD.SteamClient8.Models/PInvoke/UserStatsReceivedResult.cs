#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 接收用户统计信息模型
/// </summary>
public sealed record class UserStatsReceivedResult
{
    /// <summary>
    /// 游戏 Id
    /// </summary>
    public ulong GameId { get; set; }

    /// <summary>
    /// 结果
    /// </summary>
    public int Result { get; set; }
}
