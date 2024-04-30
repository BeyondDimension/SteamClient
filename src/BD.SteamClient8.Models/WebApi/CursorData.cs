namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 钱包游标数据
/// </summary>
public sealed record class CursorData
{
    /// <summary>
    /// 钱包唯一标识符
    /// </summary>
    [JsonPropertyName("wallet_txnid")]
    public string? WalletTxnid { get; set; }

    /// <summary>
    /// 最新时间戳
    /// </summary>
    [JsonPropertyName("timestamp_newest")]
    public long TimestampNewest { get; set; }

    /// <summary>
    /// 钱包余额
    /// </summary>
    [JsonPropertyName("balance")]
    public string? Balance { get; set; }

    /// <summary>
    /// 货币单位
    /// </summary>
    [JsonPropertyName("currency")]
    public int Currency { get; set; }
}
