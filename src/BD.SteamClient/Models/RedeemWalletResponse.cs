namespace BD.SteamClient.Primitives.Models;

public sealed class RedeemWalletResponse : JsonModel
{
    [JsonPropertyName("success")]
    public SteamResult Result { get; private set; }

    [JsonPropertyName("detail")]
    public PurchaseResultDetail Detail { get; set; }
}

[JsonSerializable(typeof(RedeemWalletResponse))]
public partial class RedeemWalletResponse_ : JsonSerializerContext
{

}