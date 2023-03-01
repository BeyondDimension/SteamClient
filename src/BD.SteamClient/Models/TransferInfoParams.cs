namespace BD.SteamClient.Models;

public sealed class TransferInfoParams : JsonModel
{
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }

    [JsonPropertyName("auth")]
    public string? Auth { get; set; }
}