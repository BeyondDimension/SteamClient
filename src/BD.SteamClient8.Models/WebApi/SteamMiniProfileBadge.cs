using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// Steam 小型用户个人资料徽章
/// </summary>
[global::MemoryPack.MemoryPackable]
public sealed partial record class SteamMiniProfileBadge : JsonRecordModel<SteamMiniProfileBadge>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 徽章名称
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("name")]
    [global::System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("xp")]
    [global::System.Text.Json.Serialization.JsonPropertyName("xp")]
    public string? Xp { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("level")]
    [global::System.Text.Json.Serialization.JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("description")]
    [global::System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 徽章图标
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("icon")]
    [global::System.Text.Json.Serialization.JsonPropertyName("icon")]
    public string? Icon { get; set; }
}
