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
