using BD.Common8.Models.Abstractions;
using System.Text.Json;

namespace BD.SteamClient8.Models.WebApi.Profiles;

/// <summary>
/// 库存历史记录页面返回
/// </summary>
public sealed record class InventoryTradeHistoryRenderPageResponse : JsonRecordModel<InventoryTradeHistoryRenderPageResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Html 数据
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("html")]
    public string Html { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("num")]
    public int Num { get; set; }

    /// <summary>
    /// 库存所属游戏列表
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("apps")]
    public IEnumerable<InventoryTradeHistoryApp> Apps { get; set; } = [];

    /// <summary>
    /// 描述
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("descriptions")]
    public JsonElement? Descriptions { get; set; }

    /// <summary>
    /// 历史记录请求标记
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("cursor")]
    public InventoryTradeHistoryCursor? Cursor { get; set; }

    /// <summary>
    /// 下一次请求
    /// </summary>
    public bool Next => Success && Cursor != null;

    public record class InventoryTradeHistoryCursor : JsonRecordModel<InventoryTradeHistoryCursor>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// 请求时间
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("time")]
        public long Time { get; set; }

        /// <summary>
        /// 请求间隔
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("time_frac")]
        public long TimeFrac { get; set; }

        /// <summary>
        /// S
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("s")]
        public string S { get; set; } = string.Empty;
    }

    /// <summary>
    /// 库存交易历史游戏
    /// </summary>
    public record class InventoryTradeHistoryApp : JsonRecordModel<InventoryTradeHistoryApp>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// 游戏 AppId
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("appid")]
        public int AppId { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 游戏图标
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 链接
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;
    }
}

public sealed record class InventoryTradeHistoryRow : JsonRecordModel<InventoryTradeHistoryRow>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 库存交易日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 库存交易时间
    /// </summary>
    public TimeSpan TimeOfDate { get; set; }

    /// <summary>
    /// 库存交易日期时间
    /// </summary>
    public DateTime? DateTime => Date != default && TimeOfDate != default ? Date.Add(TimeOfDate) : default;

    /// <summary>
    /// 描述
    /// </summary>
    public string Desc { get; set; } = string.Empty;

    /// <summary>
    /// 交易物品组
    /// </summary>
    public IEnumerable<InventoryTradeHistoryGroup> Groups { get; set; } = [];
}

public sealed record class InventoryTradeHistoryGroup : JsonRecordModel<InventoryTradeHistoryGroup>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// + - 符号
    /// </summary>
    public string PlusMinus { get; set; } = string.Empty;

    /// <summary>
    /// 物品信息
    /// </summary>
    public IEnumerable<InventoryTradeHistoryItem> Items { get; set; } = [];
}

public sealed record class InventoryTradeHistoryItem : JsonRecordModel<InventoryTradeHistoryItem>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 游戏 AppId
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 物品类型 Id
    /// </summary>
    public string ClassId { get; set; } = string.Empty;

    /// <summary>
    /// 物品上下文 Id
    /// </summary>
    public string ContextId { get; set; } = string.Empty;

    /// <summary>
    /// 物品实例 Id
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// 物品名称
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// 物品预览图片 Url
    /// </summary>
    public string ProfilePreviewPageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 物品图片 Url
    /// </summary>
    public string ItemImgUrl { get; set; } = string.Empty;
}
