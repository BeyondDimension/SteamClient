namespace BD.SteamClient.Models.Trade;

internal sealed class TradeOffer
{
    [JsonPropertyName("newversion")]
    public bool Newversion { get; set; } = true;

    [JsonPropertyName("version")]
    public int Version { get; set; } = 4;

    [JsonPropertyName("me")]
    public TradeOfferParty Me { get; set; } = null!;

    [JsonPropertyName("them")]
    public TradeOfferParty Them { get; set; } = null!;
}

internal sealed class TradeOfferParty
{
    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; } = null!;

    [JsonPropertyName("currency")]
    public int[] Currency { get; set; } = Array.Empty<int>();

    [JsonPropertyName("ready")]
    public bool Ready { get; set; }
}
