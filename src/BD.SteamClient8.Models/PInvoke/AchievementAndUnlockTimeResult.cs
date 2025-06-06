using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.PInvoke;

/// <summary>
/// 获取指定成就的完成状态和解锁时间结果
/// </summary>
public sealed record class AchievementAndUnlockTimeResult : JsonRecordModel<AchievementAndUnlockTimeResult>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 返回是否完成该成就
    /// </summary>
    public bool IsAchieved { get; set; }

    /// <summary>
    /// 返回解锁该成就的时间
    /// </summary>
    public long UnlockTime { get; set; }
}
