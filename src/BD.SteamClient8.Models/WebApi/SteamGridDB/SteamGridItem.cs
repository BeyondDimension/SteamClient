using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Enums.WebApi.SteamGridDB;

namespace BD.SteamClient8.Models.WebApi.SteamGridDB;

public sealed record class SteamGridItem : JsonRecordModel<SteamGridItem>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 唯一标识符
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 分数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>
    /// 风格
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("style")]
    public string Style { get; set; } = "";

    /// <summary>
    /// 链接地址
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [global::System.Text.Json.Serialization.JsonPropertyName("thumb")]
    public string Thumb { get; set; } = "";

    /// <summary>
    /// 标签
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 作者
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("author")]
    public SteamGridItemAuthor Author { get; set; } = new();

    /// <summary>
    /// <see cref="SteamGridItemType" /> 类型
    /// </summary>
    [global::System.Text.Json.Serialization.JsonIgnore]
    public SteamGridItemType GridType { get; set; }
}

public sealed class SteamGridItemAuthor : JsonModel<SteamGridItemAuthor>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 用户名称
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Steam64 ID
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("steam64")]
    public string Steam64 { get; set; } = "";

    /// <summary>
    /// 头像
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("avatar")]
    public string Avatar { get; set; } = "";
}

public sealed class SteamGridItemData : JsonModel<SteamGridItemData>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 数据列表
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("data")]
    public List<SteamGridItem> Data { get; set; } = [];

    /// <summary>
    /// 错误列表
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = [];
}
