namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 小型用户个人资料
/// </summary>
[MP2Obj]
public sealed partial record class SteamMiniProfile
{
    /// <summary>
    /// Steam 等级
    /// </summary>
    [NewtonsoftJsonProperty("level")]
    [SystemTextJsonProperty("level")]
    public int Level { get; set; }

    /// <summary>
    /// LevelClass
    /// </summary>
    [NewtonsoftJsonProperty("level_class")]
    [SystemTextJsonProperty("level_class")]
    public string? LevelClass { get; set; }

    /// <summary>
    /// 静态头像链接
    /// </summary>
    [NewtonsoftJsonProperty("avatar_url")]
    [SystemTextJsonProperty("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Steam 昵称
    /// </summary>
    [NewtonsoftJsonProperty("persona_name")]
    [SystemTextJsonProperty("persona_name")]
    public string? PersonaName { get; set; }

    /// <summary>
    /// 收藏的徽章
    /// </summary>
    [NewtonsoftJsonProperty("favorite_badge")]
    [SystemTextJsonProperty("favorite_badge")]
    public SteamMiniProfileBadge? FavoriteBadge { get; set; }

    /// <summary>
    /// 进行中的游戏
    /// </summary>
    [NewtonsoftJsonProperty("in_game")]
    [SystemTextJsonProperty("in_game")]
    public SteamMiniProfileGame? InGame { get; set; }

    /// <summary>
    /// 背景
    /// </summary>
    [NewtonsoftJsonProperty("profile_background")]
    [SystemTextJsonProperty("profile_background")]
    public SteamMiniProfileProfileBackground? Background { get; set; }

    /// <summary>
    /// 头像框 Url
    /// </summary>
    [NewtonsoftJsonProperty("avatar_frame")]
    [SystemTextJsonProperty("avatar_frame")]
    public string? AvatarFrame { get; set; }

    /// <summary>
    /// 动态头像 Url
    /// </summary>
    [NewtonsoftJsonProperty("animated_avatar")]
    [SystemTextJsonProperty("animated_avatar")]
    public string? AnimatedAvatar { get; set; }
}
