using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Enums.WebApi;

namespace BD.SteamClient8.Models.WebApi;

public sealed record class RedeemWalletCodeResult : JsonRecordModel<RedeemWalletCodeResult>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    public SteamResult Result { get; set; }

    public PurchaseResultDetail? Detail { get; set; }
}
