#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

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
