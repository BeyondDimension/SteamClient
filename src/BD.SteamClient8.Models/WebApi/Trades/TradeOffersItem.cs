using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Trades;

/// <summary>
/// 交易报价物品信息
/// </summary>
public sealed record class TradeOffersItem : JsonRecordModel<TradeOffersItem>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 游戏 AppId
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("appid")]
    public int AppId { get; set; }

    /// <summary>
    /// 资产上下文 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("contextid")]
    public string ContextId { get; set; } = string.Empty;

    /// <summary>
    /// 资产 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("assetid")]
    public string AssetId { get; set; } = string.Empty;

    /// <summary>
    /// 资产类别 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("classid")]
    public string ClassId { get; set; } = string.Empty;

    /// <summary>
    /// 资产实例 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("instanceid")]
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// 市场中找不到该物品
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("missing")]
    public bool Missing { get; set; }

    /// <summary>
    /// 预估的美元
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("est_usd")]
    public string EstUsd { get; set; } = string.Empty;
}