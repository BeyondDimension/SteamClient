using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// Steam 小型用户个人资料
/// </summary>
[global::MemoryPack.MemoryPackable]
public sealed partial record class SteamMiniProfile : JsonRecordModel<SteamMiniProfile>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// Steam 等级
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("level")]
    [global::System.Text.Json.Serialization.JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// LevelClass
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("level_class")]
    [global::System.Text.Json.Serialization.JsonPropertyName("level_class")]
    public string? LevelClass { get; set; }

    /// <summary>
    /// 静态头像链接
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("avatar_url")]
    [global::System.Text.Json.Serialization.JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Steam 昵称
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("persona_name")]
    [global::System.Text.Json.Serialization.JsonPropertyName("persona_name")]
    public string? PersonaName { get; set; }

    /// <summary>
    /// 收藏的徽章
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("favorite_badge")]
    [global::System.Text.Json.Serialization.JsonPropertyName("favorite_badge")]
    public SteamMiniProfileBadge? FavoriteBadge { get; set; }

    /// <summary>
    /// 进行中的游戏
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("in_game")]
    [global::System.Text.Json.Serialization.JsonPropertyName("in_game")]
    public SteamMiniProfileGame? InGame { get; set; }

    /// <summary>
    /// 背景
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("profile_background")]
    [global::System.Text.Json.Serialization.JsonPropertyName("profile_background")]
    public SteamMiniProfileProfileBackground? Background { get; set; }

    /// <summary>
    /// 头像框 Url
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("avatar_frame")]
    [global::System.Text.Json.Serialization.JsonPropertyName("avatar_frame")]
    public string? AvatarFrame { get; set; }

    /// <summary>
    /// 动态头像 Url
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("animated_avatar")]
    [global::System.Text.Json.Serialization.JsonPropertyName("animated_avatar")]
    public string? AnimatedAvatar { get; set; }
}
