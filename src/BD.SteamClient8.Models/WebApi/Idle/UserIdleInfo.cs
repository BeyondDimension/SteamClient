#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 用户 Idle 信息
/// </summary>
public record class UserIdleInfo
{
    /// <summary>
    /// 用户昵称
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 用户头像
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 用户等级
    /// </summary>
    public ushort UserLevel { get; set; }

    /// <summary>
    /// 用户下一等级
    /// </summary>
    public ushort NextLevel => (ushort)(UserLevel + 1);

    /// <summary>
    /// 用户下一等级所需经验百分比
    /// </summary>
    public short NextLevelExpPercentage { get; set; }

    /// <summary>
    /// 用户当前经验
    /// </summary>
    public int CurrentExp { get; set; }

    /// <summary>
    /// 升级所需经验
    /// </summary>
    public int UpExp { get; set; }
}
