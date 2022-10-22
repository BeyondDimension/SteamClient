using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.SteamClient.Models;

public sealed class CurrencyData
{
    public string? Display { get; set; }

    public string? CurrencyCode { get; set; }
}

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
