#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 小型用户个人资料
/// </summary>
public record class SteamMiniProfile
{
    /// <summary>
    /// 徽章
    /// </summary>
    public class Badge
    {
        /// <summary>
        /// 徽章名称
        /// </summary>
        [NewtonsoftJsonProperty("name")]
        [SystemTextJsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 经验值
        /// </summary>
        [NewtonsoftJsonProperty("xp")]
        [SystemTextJsonProperty("xp")]
        public string? Xp { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        [NewtonsoftJsonProperty("level")]
        [SystemTextJsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        [NewtonsoftJsonProperty("description")]
        [SystemTextJsonProperty("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 徽章图标
        /// </summary>
        [NewtonsoftJsonProperty("icon")]
        [SystemTextJsonProperty("icon")]
        public string? Icon { get; set; }
    }

    /// <summary>
    /// 游戏
    /// </summary>
    public class Game
    {
        /// <summary>
        /// 游戏名称
        /// </summary>
        [NewtonsoftJsonProperty("name")]
        [SystemTextJsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 非 Steam 游戏
        /// </summary>
        [NewtonsoftJsonProperty("is_non_steam")]
        [SystemTextJsonProperty("is_non_steam")]
        public bool IsNonSteam { get; set; }

        /// <summary>
        /// 游戏LOGO
        /// </summary>
        [NewtonsoftJsonProperty("logo")]
        [SystemTextJsonProperty("logo")]
        public string? Logo { get; set; }
    }

    /// <summary>
    /// 个人资料
    /// </summary>
    public class ProfileBackground
    {
        /// <summary>
        /// Webm 格式
        /// </summary>
        [NewtonsoftJsonProperty("video/webm")]
        [SystemTextJsonProperty("video/webm")]
        public string? VideoWebm { get; set; }

        /// <summary>
        /// Mp4 格式
        /// </summary>
        [NewtonsoftJsonProperty("video/mp4")]
        [SystemTextJsonProperty("video/mp4")]
        public string? VideoMp4 { get; set; }
    }

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
    public Badge? FavoriteBadge { get; set; }

    /// <summary>
    /// 进行中的游戏
    /// </summary>
    [NewtonsoftJsonProperty("in_game")]
    [SystemTextJsonProperty("in_game")]
    public Game? InGame { get; set; }

    /// <summary>
    /// 背景
    /// </summary>
    [NewtonsoftJsonProperty("profile_background")]
    [SystemTextJsonProperty("profile_background")]
    public ProfileBackground? Background { get; set; }

    /// <summary>
    /// 头像框 Url
    /// </summary>
    [NewtonsoftJsonProperty("avatar_frame")]
    [SystemTextJsonProperty("avatar_frame")]
    public string? AvatarFrame { get; set; }

    /// <summary>
    /// 头像帧流
    /// </summary>
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public Task<string?>? AvatarFrameStream { get; set; }

    /// <summary>
    /// 动态头像 Url
    /// </summary>
    [NewtonsoftJsonProperty("animated_avatar")]
    [SystemTextJsonProperty("animated_avatar")]
    public string? AnimatedAvatar { get; set; }

    /// <summary>
    /// 动态头像流
    /// </summary>
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public Task<string?>? AnimatedAvatarStream { get; set; }
}
