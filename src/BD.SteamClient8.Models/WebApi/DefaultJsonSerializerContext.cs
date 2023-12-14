namespace BD.SteamClient8.Models.WebApi;

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
[SystemTextJsonSerializable(typeof(Badge))]
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
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[SystemTextJsonSerializable(typeof(ModifiedApp))]
#endif
[SystemTextJsonSerializable(typeof(SteamApp.SteamApp))]
[SystemTextJsonSerializable(typeof(SteamApps))]
[SystemTextJsonSerializable(typeof(SteamAppInfo))]
[SystemTextJsonSerializable(typeof(SteamAppSaveFile))]
//// SteamGridDB
[SystemTextJsonSerializable(typeof(SteamGridApp))]
[SystemTextJsonSerializable(typeof(SteamGridItem))]
//// Trade
[SystemTextJsonSerializable(typeof(Asset))]
[SystemTextJsonSerializable(typeof(Confirmation))]
[SystemTextJsonSerializable(typeof(TradeHistory))]
[SystemTextJsonSerializable(typeof(TradeInfo))]
[SystemTextJsonSerializable(typeof(TradeItem))]
[SystemTextJsonSerializable(typeof(TradeSummary))]
[SystemTextJsonSerializable(typeof(TradeResponse))]
[SystemTextJsonSerializable(typeof(TradeSummaryResponse))]
//// Authenticator
[SystemTextJsonSerializable(typeof(GetUserCountryResponse))]
[SystemTextJsonSerializable(typeof(SteamAddPhoneNumberResponse))]
[SystemTextJsonSerializable(typeof(IsAccountWaitingForEmailConfirmationResponse))]
[SystemTextJsonSerializable(typeof(SteamGetRsaKeyJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginOauthJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginHasPhoneJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginTfaJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamDoLoginFinalizeJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamSyncStruct))]
[SystemTextJsonSerializable(typeof(SteamConvertSteamDataJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamMobileDologinJsonStruct))]
[SystemTextJsonSerializable(typeof(SteamMobileConfGetListJsonStruct))]
[SystemTextJsonSerializable(typeof(RemoveAuthenticatorResponse))]
[SystemTextJsonSerializable(typeof(GenerateAccessTokenForAppResponse))]
public partial class DefaultJsonSerializerContext_ : SystemTextJsonSerializerContext
{
}
