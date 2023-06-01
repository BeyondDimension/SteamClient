namespace BD.SteamClient.Primitives.Models;

public sealed class TransferInfo : JsonModel
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("params")]
    public TransferInfoParams? Params { get; set; }
}
