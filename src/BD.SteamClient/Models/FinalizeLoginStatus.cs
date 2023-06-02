namespace BD.SteamClient.Models;

public sealed class FinalizeLoginStatus : JsonModel
{
    [JsonPropertyName("steamID")]
    public string? SteamId { get; set; }

    [JsonPropertyName("redir")]
    public string? Redir { get; set; }

    [JsonPropertyName("transfer_info")]
    public List<TransferInfo>? TransferInfo { get; set; }

    [JsonPropertyName("primary_domain")]
    public string? PrimaryDomain { get; set; }
}

[JsonSerializable(typeof(FinalizeLoginStatus))]
public partial class FinalizeLoginStatus_ : JsonSerializerContext
{

}