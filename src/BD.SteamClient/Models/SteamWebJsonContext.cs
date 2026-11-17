using BD.SteamClient.Models.Profile;
using BD.SteamClient.Models.Trade;
using BD.SteamClient.Services.Implementation;
using BD.SteamClient.Models.SteamGridDB;

namespace BD.SteamClient.Models;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(InventoryPageResponse))]
[JsonSerializable(typeof(InventoryTradeHistoryRenderPageResponse))]
[JsonSerializable(typeof(MarketItemPriceOverviewResponse))]
[JsonSerializable(typeof(MarketItemOrdersHistogramResponse))]
[JsonSerializable(typeof(SellItemToMarketResponse))]
[JsonSerializable(typeof(MarketTradingHisotryRenderPageResponse))]
[JsonSerializable(typeof(SteamMiniProfile))]
[JsonSerializable(typeof(SteamGridAppData))]
[JsonSerializable(typeof(SteamGridItemData))]
internal partial class SteamWebJsonContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(SteamAuthenticatorServiceImpl.SteamAccessToken))]
[JsonSerializable(typeof(SteamAuthenticatorServiceImpl.GenerateAccessTokenForAppResponse))]
[JsonSerializable(typeof(TradeHistory))]
[JsonSerializable(typeof(TradeHistory.TradeItem), TypeInfoPropertyName = "TradeHistoryItem")]
[JsonSerializable(typeof(TradeInfo))]
[JsonSerializable(typeof(TradeOffer))]
[JsonSerializable(typeof(IEnumerable<Confirmation>))]
internal partial class SteamTradeJsonContext : JsonSerializerContext
{
}
