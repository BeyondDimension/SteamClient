namespace BD.SteamClient.Models.Trade;

public class TradeInfo
{
    [JsonPropertyName("tradeofferid")]
    public string TradeOfferId { get; set; } = string.Empty;

    [JsonPropertyName("accountid_other")]
    public long AccountIdOther { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("expiration_time")]
    public long ExpirationTime { get; set; }

    [JsonPropertyName("trade_offer_state")]
    public TradeOfferState TradeOfferState { get; set; }

    [JsonPropertyName("items_to_give")]
    public List<TradeItem>? ItemsToGive { get; set; }

    [JsonPropertyName("items_to_receive")]
    public List<TradeItem>? ItemsToReceive { get; set; }

    [JsonPropertyName("is_our_offer")]
    public bool IsOurOffer { get; set; }

    [JsonPropertyName("time_created")]
    public long TimeCreated { get; set; }

    [JsonPropertyName("time_updated")]
    public long TimeUpdated { get; set; }

    [JsonPropertyName("from_real_time_trade")]
    public bool FromRealTimeTrade { get; set; }

    [JsonPropertyName("escrow_end_date")]
    public int EscrowEndDate { get; set; }

    [JsonPropertyName("confirmation_method")]
    public int ConfirmationMethod { get; set; }

    [JsonPropertyName("eresult")]
    public int Eresult { get; set; }
}

public partial class TradeResponse : JsonModel
{
    [JsonPropertyName("response")]
    public TradeResponseInfo? Response { get; set; }
}

public class TradeResponseInfo
{
    [JsonPropertyName("trade_offers_sent")]
    public List<TradeInfo>? TradeOffersSent { get; set; }

    [JsonPropertyName("trade_offers_received")]
    public List<TradeInfo>? TradeOffersReceived { get; set; }

    [JsonPropertyName("next_cursor")]
    public int NextCursor { get; set; }
}

[JsonSerializable(typeof(TradeResponse))]
internal partial class TradeResponse_ : JsonSerializerContext
{
}
