namespace BD.SteamClient8.Models.WebApi;

public sealed record class RedeemWalletCodeResult
{
    public SteamResult Result { get; set; }

    public PurchaseResultDetail? Detail { get; set; }
}
