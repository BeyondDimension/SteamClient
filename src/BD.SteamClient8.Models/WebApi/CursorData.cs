using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 钱包游标数据
/// </summary>
public sealed record class CursorData : JsonRecordModel<CursorData>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 钱包唯一标识符
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("wallet_txnid")]
    public string? WalletTxnid { get; set; }

    /// <summary>
    /// 最新时间戳
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("timestamp_newest")]
    public long TimestampNewest { get; set; }

    /// <summary>
    /// 钱包余额
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("balance")]
    public string? Balance { get; set; }

    /// <summary>
    /// 货币单位
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("currency")]
    public int Currency { get; set; }
}
