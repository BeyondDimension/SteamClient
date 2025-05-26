using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.SteamGridDB;

/// <summary>
/// steamgriddb.com App
/// </summary>
public sealed record class SteamGridApp : JsonRecordModel<SteamGridApp>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 唯一标识符
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 类型列表
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("types")]
    public List<string> Types { get; set; } = [];

    /// <summary>
    /// 是否验证
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("verified")]
    public bool Verified { get; set; }
}

/// <summary>
/// steamgriddb.com AppData
/// </summary>
public sealed class SteamGridAppData : JsonModel<SteamGridAppData>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 数据内容
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("data")]
    public SteamGridApp Data { get; set; } = new();

    /// <summary>
    /// 错误列表
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = [];
}
