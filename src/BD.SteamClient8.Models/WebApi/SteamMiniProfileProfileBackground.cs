using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// Steam 小型用户个人资料视频
/// </summary>
[global::MemoryPack.MemoryPackable]
public sealed partial record class SteamMiniProfileProfileBackground : JsonRecordModel<SteamMiniProfileProfileBackground>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// Webm 格式
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("video/webm")]
    [global::System.Text.Json.Serialization.JsonPropertyName("video/webm")]
    public string? VideoWebm { get; set; }

    /// <summary>
    /// Mp4 格式
    /// </summary>
    [global::Newtonsoft.Json.JsonProperty("video/mp4")]
    [global::System.Text.Json.Serialization.JsonPropertyName("video/mp4")]
    public string? VideoMp4 { get; set; }
}
