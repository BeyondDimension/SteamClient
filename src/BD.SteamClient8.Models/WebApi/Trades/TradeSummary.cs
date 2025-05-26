using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Trades;

/// <summary>
/// 未处理和新的交易报价汇总
/// <para>https://partner.steamgames.com/doc/webapi/IEconService</para>
/// </summary>
public sealed class TradeSummary : JsonModel<TradeSummary>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 待处理的交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("pending_received_count")]
    public int PendingReceivedCount { get; set; }

    /// <summary>
    /// 新的收到的交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("new_received_count")]
    public int NewReceivedCount { get; set; }

    /// <summary>
    /// 更新的收到交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("updated_received_count")]
    public int UpdatedReceivedCount { get; set; }

    /// <summary>
    /// 历史收到的交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("historical_received_count")]
    public int HistoricalReceivedCount { get; set; }

    /// <summary>
    /// 待处理的发送交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("pending_sent_count")]
    public int PendingSentCount { get; set; }

    /// <summary>
    /// 新接受的发送交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("newly_accepted_sent_count")]
    public int NewlyAcceptedSentCount { get; set; }

    /// <summary>
    /// 更新的发送交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("updated_sent_count")]
    public int UpdatedSentCount { get; set; }

    /// <summary>
    /// 历史发送数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("historical_sent_count")]
    public int HistoricalSentCount { get; set; }

    /// <summary>
    /// 在托管状态中的收到交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("escrow_received_count")]
    public int EscrowReceivedCount { get; set; }

    /// <summary>
    /// 在托管状态中的发送交易报价数量
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("escrow_sent_count")]
    public int EscrowSentCount { get; set; }
}

public sealed partial class TradeSummaryResponse : JsonModel<TradeSummaryResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 交易报价汇总返回信息
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public TradeSummary? Response { get; set; }
}

