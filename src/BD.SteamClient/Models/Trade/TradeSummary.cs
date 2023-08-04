namespace BD.SteamClient.Models.Trade;

public class TradeSummary
{
    [JsonPropertyName("pending_received_count")]
    public int PendingReceivedCount { get; set; }

    [JsonPropertyName("new_received_count")]
    public int NewReceivedCount { get; set; }

    [JsonPropertyName("updated_received_count")]
    public int UpdatedReceivedCount { get; set; }

    [JsonPropertyName("historical_received_count")]
    public int HistoricalReceivedCount { get; set; }

    [JsonPropertyName("pending_sent_count")]
    public int PendingSentCount { get; set; }

    [JsonPropertyName("newly_accepted_sent_count")]
    public int NewlyAcceptedSentCount { get; set; }

    [JsonPropertyName("updated_sent_count")]
    public int UpdatedSentCount { get; set; }

    [JsonPropertyName("historical_sent_count")]
    public int HistoricalSentCount { get; set; }

    [JsonPropertyName("escrow_received_count")]
    public int EscrowReceivedCount { get; set; }

    [JsonPropertyName("escrow_sent_count")]
    public int EscrowSentCount { get; set; }
}

public partial class TradeSummaryResponse : JsonModel
{
    [JsonPropertyName("response")]
    public TradeSummary? Response { get; set; }
}

[JsonSerializable(typeof(TradeSummaryResponse))]
internal partial class TradeSummaryResponse_ : JsonSerializerContext
{
}


