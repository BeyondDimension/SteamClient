namespace BD.SteamClient.Models;

public sealed class DoLoginRespone : JsonModel
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("requires_twofactor")]
    public bool RequiresTwofactor { get; set; }

    [JsonPropertyName("login_complete")]
    public bool LoginComplete { get; set; }

    [JsonPropertyName("transfer_urls")]
    public List<string>? TransferUrls { get; set; }

    [JsonPropertyName("transfer_parameters")]
    public TransferParameters? TransferParameters { get; set; }
}

[JsonSerializable(typeof(DoLoginRespone))]
public partial class DoLoginRespone_ : JsonSerializerContext
{

}
