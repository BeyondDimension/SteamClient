namespace BD.SteamClient.Models.Idle;

public class CardsMarketPrice
{
    public string CardName { get; set; } = string.Empty;

    public bool IsFoil { get; set; }

    public string MarketUrl { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
