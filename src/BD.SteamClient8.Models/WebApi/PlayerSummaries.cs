using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

public sealed record PlayerSummaries : JsonRecordModel<PlayerSummaries>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// Steam64 Id
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("steamid")]
    [global::System.Text.Json.Serialization.JsonPropertyName("steamid")]
    public string SteamId { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("personaname")]
    [global::System.Text.Json.Serialization.JsonPropertyName("personaname")]
    public string PersonaName { get; set; } = string.Empty;

    /// <summary>
    /// 用户真实姓名
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("realname")]
    [global::System.Text.Json.Serialization.JsonPropertyName("realname")]
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// 账号创建时间戳
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("timecreated")]
    [global::System.Text.Json.Serialization.JsonPropertyName("timecreated")]
    public long TimeCreated { get; set; }

    /// <summary>
    /// 用户个人资料主页
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("profileurl")]
    [global::System.Text.Json.Serialization.JsonPropertyName("profileurl")]
    public string Profileurl { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("avatar")]
    [global::System.Text.Json.Serialization.JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像（中）
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("avatarmedium")]
    [global::System.Text.Json.Serialization.JsonPropertyName("avatarmedium")]
    public string AvatarMedium { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像（大）
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("avatarfull")]
    [global::System.Text.Json.Serialization.JsonPropertyName("avatarfull")]
    public string AvatarFull { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像哈希值，可用于拼接头像地址
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("avatarhash")]
    [global::System.Text.Json.Serialization.JsonPropertyName("avatarhash")]
    public string AvatarHash { get; set; } = string.Empty;

    /// <summary>
    /// 用户创建时间
    /// </summary>
    [global::Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public DateTimeOffset CreationTime => DateTimeOffset.FromUnixTimeSeconds(TimeCreated);
}
