namespace BD.SteamClient.Primitives.Models;

public sealed class CursorData
{
    [JsonPropertyName("wallet_txnid")]
    public string? WalletTxnid { get; set; }

    [JsonPropertyName("timestamp_newest")]
    public long TimestampNewest { get; set; }

    [JsonPropertyName("balance")]
    public string? Balance { get; set; }

    [JsonPropertyName("currency")]
    public int Currency { get; set; }
}
