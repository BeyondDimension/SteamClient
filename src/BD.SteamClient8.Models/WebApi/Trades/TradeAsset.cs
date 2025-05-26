using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Trades;

/// <summary>
/// 库存资产
/// </summary>
public sealed record class TradeAsset : JsonRecordModel<TradeAsset>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 游戏 AppId
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("appid")]
    public int AppId { get; set; }

    /// <summary>
    /// 上下文 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("contextid")]
    public string ContextId { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// 资产 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("assetid")]
    public string AssetId { get; set; } = string.Empty;
}
