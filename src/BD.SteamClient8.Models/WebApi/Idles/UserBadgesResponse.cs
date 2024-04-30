namespace BD.SteamClient8.Models.WebApi.Idles;

/// <summary>
/// 获取用户徽章信息响应返回
/// </summary>
public sealed record class UserBadgesResponse
{
    /// <summary>
    /// 用户 Idle 信息
    /// </summary>
    public UserIdleInfo? UserIdleInfo { get; set; }

    /// <summary>
    /// 用户徽章信息列表
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
