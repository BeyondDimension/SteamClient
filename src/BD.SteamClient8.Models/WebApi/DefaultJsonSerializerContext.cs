#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// Json 源生成类型上下文
/// </summary>
[SystemTextJsonSerializable(typeof(AchievementInfo))]
[SystemTextJsonSerializable(typeof(AuthorizedDevice))]
[SystemTextJsonSerializable(typeof(CurrencyData))]
[SystemTextJsonSerializable(typeof(CursorData))]
[SystemTextJsonSerializable(typeof(DisableAuthorizedDevice))]
[SystemTextJsonSerializable(typeof(FloatStatInfo))]
[SystemTextJsonSerializable(typeof(ImportedSDAEntry))]
[SystemTextJsonSerializable(typeof(IntStatInfo))]
[SystemTextJsonSerializable(typeof(RedeemWalletResponse))]
[SystemTextJsonSerializable(typeof(SteamMiniProfile))]
[SystemTextJsonSerializable(typeof(SteamRemoteFile))]
[SystemTextJsonSerializable(typeof(SteamUser))]
//// Idle
[SystemTextJsonSerializable(typeof(AppCardsAvgPrice))]
[SystemTextJsonSerializable(typeof(IdleBadge))]
[SystemTextJsonSerializable(typeof(CardsMarketPrice))]
[SystemTextJsonSerializable(typeof(SteamCard))]
[SystemTextJsonSerializable(typeof(UserIdleInfo))]
//// Login
[SystemTextJsonSerializable(typeof(DoLoginResponse))]
[SystemTextJsonSerializable(typeof(FinalizeLoginStatus))]
[SystemTextJsonSerializable(typeof(SteamLoginState))]
[SystemTextJsonSerializable(typeof(SteamSession))]
[SystemTextJsonSerializable(typeof(TransferInfo))]
[SystemTextJsonSerializable(typeof(TransferInfoParams))]
[SystemTextJsonSerializable(typeof(TransferParameters))]
//// Market
[SystemTextJsonSerializable(typeof(MarketItemOrdersHistogramResponse))]
[SystemTextJsonSerializable(typeof(MarketItemPriceOverviewResponse))]
[SystemTextJsonSerializable(typeof(MarketListings))]
[SystemTextJsonSerializable(typeof(MarketTradingHistoryRenderPageResponse))]
[SystemTextJsonSerializable(typeof(SellItemToMarketResponse))]
//// Profile
[SystemTextJsonSerializable(typeof(HistoryParseResponse))]
[SystemTextJsonSerializable(typeof(InventoryPageResponse))]
[SystemTextJsonSerializable(typeof(InventoryTradeHistoryRenderPageResponse))]
[SystemTextJsonSerializable(typeof(LoginHistoryItem))]
[SystemTextJsonSerializable(typeof(SendGiftHistoryItem))]
//// SteamApp
#if !(IOS || ANDROID)
[SystemTextJsonSerializable(typeof(ModifiedApp))]
#endif
[SystemTextJsonSerializable(typeof(SteamApp))]
[SystemTextJsonSerializable(typeof(SteamApps))]
[SystemTextJsonSerializable(typeof(SteamAppInfo))]
[SystemTextJsonSerializable(typeof(SteamAppSaveFile))]
//// SteamGridDB
[SystemTextJsonSerializable(typeof(SteamGridApp))]
[SystemTextJsonSerializable(typeof(SteamGridItemData))]
[SystemTextJsonSerializable(typeof(SteamGridItem))]
[SystemTextJsonSerializable(typeof(SteamGridAppData))]
//// Trade
[SystemTextJsonSerializable(typeof(TradeAsset))]
[SystemTextJsonSerializable(typeof(TradeConfirmation))]
[SystemTextJsonSerializable(typeof(TradeHistory))]
[SystemTextJsonSerializable(typeof(TradeHistory.TradeHistoryResponseDetail))]
[SystemTextJsonSerializable(typeof(TradeHistory.TradeItem))]
[SystemTextJsonSerializable(typeof(TradeHistory.TradeItemAssetItem))]
[SystemTextJsonSerializable(typeof(TradeHistory.TradeHistoryResponseDetail))]
[SystemTextJsonSerializable(typeof(TradeOffersResponse))]
[SystemTextJsonSerializable(typeof(TradeOffersResponseInfo))]
[SystemTextJsonSerializable(typeof(TradeOffersInfo))]
[SystemTextJsonSerializable(typeof(TradeOffersItem))]
[SystemTextJsonSerializable(typeof(TradeSummary))]
[SystemTextJsonSerializable(typeof(TradeSummaryResponse))]
//// Authenticator
[SystemTextJsonSerializable(typeof(GetUserCountryOrRegionResponse))]
[SystemTextJsonSerializable(typeof(SteamAddPhoneNumberResponse))]
[SystemTextJsonSerializable(typeof(IsAccountWaitingForEmailConfirmationResponse))]
[SystemTextJsonSerializable(typeof(SteamGetRsaKeyJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginHasPhoneJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginTfaJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginFinalizeJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamSyncStruct))]
[SystemTextJsonSerializable(typeof(SteamConvertSteamDataJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamMobileDologinJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamMobileConfGetListJsonStruct))]
[SystemTextJsonSerializable(typeof(RemoveAuthenticatorResponse))]
[SystemTextJsonSerializable(typeof(GenerateAccessTokenForAppResponse))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
public partial class DefaultJsonSerializerContext_ : SystemTextJsonSerializerContext
{
}
