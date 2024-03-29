namespace BD.SteamClient8.Models;

/// <summary>
/// 获取用户徽章信息响应返回
/// </summary>
public record class UserBadgesResponse
{
    /// <summary>
    /// 用户 Idle 信息 <see cref="BD.SteamClient8.Models.UserIdleInfo"/>
    /// </summary>
    public UserIdleInfo? UserIdleInfo { get; set; }

    /// <summary>
    /// 用户徽章信息列表 <see cref="BD.SteamClient8.Models.IdleBadge"/>
    /// </summary>
    public List<IdleBadge> Badges { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UserBadgesResponse"/> class.
    /// </summary>
    /// <param name="userIdleInfo"></param>
    /// <param name="badges"></param>
    public UserBadgesResponse(UserIdleInfo? userIdleInfo, List<IdleBadge> badges)
    {
        UserIdleInfo = userIdleInfo;
        Badges = badges;
    }
}
