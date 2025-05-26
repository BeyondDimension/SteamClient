using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// Steam 小型用户个人资料游戏
/// </summary>
[global::MemoryPack.MemoryPackable]
public sealed partial record class SteamMiniProfileGame : JsonRecordModel<SteamMiniProfileGame>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 游戏名称
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("name")]
    [global::System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 非 Steam 游戏
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("is_non_steam")]
    [global::System.Text.Json.Serialization.JsonPropertyName("is_non_steam")]
    public bool IsNonSteam { get; set; }

    /// <summary>
    /// 游戏 Logo
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("logo")]
    [global::System.Text.Json.Serialization.JsonPropertyName("logo")]
    public string? Logo { get; set; }
}
