namespace BD.SteamClient8.Models;

/// <summary>
/// 获取指定成就的完成状态和解锁时间结果
/// </summary>
public sealed record class AchievementAndUnlockTimeResult
{
    /// <summary>
    /// 返回是否完成该成就
    /// </summary>
    public bool IsAchieved { get; set; }

    /// <summary>
    /// 返回解锁该成就的时间
    /// </summary>
    public long UnlockTime { get; set; }
}
