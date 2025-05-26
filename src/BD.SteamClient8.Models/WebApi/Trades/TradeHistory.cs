using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Trades;

/// <summary>
/// 交易历史记录
/// <para>https://partner.steamgames.com/doc/webapi/IEconService</para>
/// </summary>
public sealed record class TradeHistory : JsonRecordModel<TradeHistory>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 返回详情
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public TradeHistoryResponseDetail? Response { get; set; }

    /// <summary>
    /// 交易历史记录响应详情
    /// </summary>
    public sealed record class TradeHistoryResponseDetail : JsonRecordModel<TradeHistoryResponseDetail>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// 是否更多
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("more")]
        public bool More { get; set; }

        /// <summary>
        /// 资产物品列表
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("trades")]
        public TradeItem[] Trades { get; set; } = [];

        /// <summary>
        /// 描述信息
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("descriptions")]
        public TradeItemAssetItemDesc[]? Descriptions { get; set; }
    }

    /// <summary>
    /// 交易历史记录信息
    /// </summary>
    public sealed record class TradeItem : JsonRecordModel<TradeItem>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// 交易 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("tradeid")]
        public string TradeId { get; set; } = string.Empty;

        /// <summary>
        /// 交易对象 SteamId
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("steamid_other")]
        public string SteamIdOther { get; set; } = string.Empty;

        /// <summary>
        /// 初始化时间
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("time_init")]
        public long TimeInit { get; set; }

        /// <summary>
        /// 交易状态
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>
        /// 失去的物品列表
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("assets_given")]
        public TradeItemAssetItem[]? AssetsGiven { get; set; }

        /// <summary>
        /// 接收的物品列表
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("assets_received")]
        public TradeItemAssetItem[]? AssetsReceived { get; set; }
    }

    /// <summary>
    /// 交易历史记录的交易项
    /// </summary>
    public sealed record class TradeItemAssetItem : JsonRecordModel<TradeItemAssetItem>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// 游戏 AppId
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("appid")]
        public int AppId { get; set; }

        /// <summary>
        /// 库存资产上下文 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("contextid")]
        public string ContextId { get; set; } = string.Empty;

        /// <summary>
        /// 库存资产 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("assetid")]
        public string AssetId { get; set; } = string.Empty;

        /// <summary>
        /// 数量
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("amount")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::System.Text.Json.Serialization.StringToDecimalConverter))]
        public decimal Amount { get; set; }

        /// <summary>
        /// 库存资产类别 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("classid")]
        public string ClassId { get; set; } = string.Empty;

        /// <summary>
        /// 库存资产实例 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("instanceid")]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 交易后新资产 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("new_assetid")]
        public string NewAssetId { get; set; } = string.Empty;

        /// <summary>
        /// 交易后新上下文 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("new_contextid")]
        public string NewContextId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 交易历史记录的交易项描述
    /// </summary>
    public sealed record class TradeItemAssetItemDesc : JsonRecordModel<TradeItemAssetItemDesc>, IJsonSerializerContext
    {
        /// <inheritdoc/>
        static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

        /// <summary>
        /// 游戏 AppId
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("appid")]
        public int AppId { get; set; }

        /// <summary>
        /// 库存资产类别 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("classid")]
        public string ClassId { get; set; } = string.Empty;

        /// <summary>
        /// 库存资产实例 Id
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("instanceid")]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 市场名称
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("market_name")]
        public string MarketName { get; set; } = string.Empty;

        /// <summary>
        /// 市场 hash 名称
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("market_hash_name")]
        public string MarketHashName { get; set; } = string.Empty;
    }
}
