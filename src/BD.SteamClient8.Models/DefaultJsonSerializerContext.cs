using BD.SteamClient8.Models.PInvoke;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.Authenticators;
using BD.SteamClient8.Models.WebApi.Authenticators.PhoneNumber;
using BD.SteamClient8.Models.WebApi.Idles;
using BD.SteamClient8.Models.WebApi.Logins;
using BD.SteamClient8.Models.WebApi.Markets;
using BD.SteamClient8.Models.WebApi.Profiles;
using BD.SteamClient8.Models.WebApi.SteamApps;
using BD.SteamClient8.Models.WebApi.SteamGridDB;
using BD.SteamClient8.Models.WebApi.Trades;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models;

/// <summary>
/// Json 源生成类型上下文
/// </summary>
[JsonSerializable(typeof(AchievementInfo))]
[JsonSerializable(typeof(AuthorizedDevice))]
[JsonSerializable(typeof(AuthorizedDevice[]))]
[JsonSerializable(typeof(IEnumerable<AuthorizedDevice>))]
[JsonSerializable(typeof(List<AuthorizedDevice>))]
[JsonSerializable(typeof(IList<AuthorizedDevice>))]
[JsonSerializable(typeof(IReadOnlyList<AuthorizedDevice>))] // 只使用源生成的类型信息时，对于集合、数组类型必须声明 attr
[JsonSerializable(typeof(CurrencyData))]
[JsonSerializable(typeof(CursorData))]
[JsonSerializable(typeof(DisableAuthorizedDevice))]
[JsonSerializable(typeof(FloatStatInfo))]
[JsonSerializable(typeof(ImportedSDAEntry))]
[JsonSerializable(typeof(IntStatInfo))]
[JsonSerializable(typeof(RedeemWalletResponse))]
[JsonSerializable(typeof(SteamMiniProfile))]
[JsonSerializable(typeof(SteamRemoteFile))]
[JsonSerializable(typeof(SteamUser))]
[JsonSerializable(typeof(SteamUser[]))]
[JsonSerializable(typeof(List<SteamUser>))]
[JsonSerializable(typeof(IList<SteamUser>))]
[JsonSerializable(typeof(IReadOnlyList<SteamUser>))]
[JsonSerializable(typeof(PlayerSummaries))]
[JsonSerializable(typeof(PlayerSummariesResponse))]
[JsonSerializable(typeof(PlayerSummariesResponse.PlayerSummariesDetail))]
//// Idle
[JsonSerializable(typeof(AppCardsAvgPrice))]
[JsonSerializable(typeof(IdleBadge))]
[JsonSerializable(typeof(CardsMarketPrice))]
[JsonSerializable(typeof(SteamCard))]
[JsonSerializable(typeof(UserIdleInfo))]
//// Login
[JsonSerializable(typeof(DoLoginResponse))]
[JsonSerializable(typeof(FinalizeLoginStatus))]
[JsonSerializable(typeof(SteamLoginState))]
[JsonSerializable(typeof(SteamSession))]
[JsonSerializable(typeof(TransferInfo))]
[JsonSerializable(typeof(TransferInfoParams))]
[JsonSerializable(typeof(TransferParameters))]
//// Market
[JsonSerializable(typeof(MarketItemOrdersHistogramResponse))]
[JsonSerializable(typeof(MarketItemPriceOverviewResponse))]
[JsonSerializable(typeof(MarketListings))]
[JsonSerializable(typeof(MarketTradingHistoryRenderPageResponse))]
[JsonSerializable(typeof(SellItemToMarketResponse))]
//// Profile
[JsonSerializable(typeof(HistoryParseResponse))]
[JsonSerializable(typeof(InventoryPageResponse))]
[JsonSerializable(typeof(InventoryTradeHistoryRenderPageResponse))]
[JsonSerializable(typeof(LoginHistoryItem))]
[JsonSerializable(typeof(SendGiftHistoryItem))]
//// SteamApp
#if !(IOS || ANDROID)
[JsonSerializable(typeof(ModifiedApp))]
[JsonSerializable(typeof(List<ModifiedApp>))]
#endif
[JsonSerializable(typeof(SteamApp))]
[JsonSerializable(typeof(SteamApp[]))]
[JsonSerializable(typeof(List<SteamApp>))]
[JsonSerializable(typeof(IList<SteamApp>))]
[JsonSerializable(typeof(IReadOnlyList<SteamApp>))]
[JsonSerializable(typeof(SteamApps))]
[JsonSerializable(typeof(SteamAppInfo))]
[JsonSerializable(typeof(SteamAppSaveFile))]
//// SteamGridDB
[JsonSerializable(typeof(SteamGridApp))]
[JsonSerializable(typeof(SteamGridItemData))]
[JsonSerializable(typeof(SteamGridItem))]
[JsonSerializable(typeof(SteamGridAppData))]
//// Trade
[JsonSerializable(typeof(TradeAsset))]
[JsonSerializable(typeof(TradeConfirmation))]
[JsonSerializable(typeof(TradeHistory))]
[JsonSerializable(typeof(TradeHistory.TradeHistoryResponseDetail))]
[JsonSerializable(typeof(TradeHistory.TradeItem))]
[JsonSerializable(typeof(TradeHistory.TradeItemAssetItem))]
[JsonSerializable(typeof(TradeHistory.TradeHistoryResponseDetail))]
[JsonSerializable(typeof(TradeOffersResponse))]
[JsonSerializable(typeof(TradeOffersResponseInfo))]
[JsonSerializable(typeof(TradeOffersInfo))]
[JsonSerializable(typeof(TradeOffersItem))]
[JsonSerializable(typeof(TradeSummary))]
[JsonSerializable(typeof(TradeSummaryResponse))]
//// Authenticator
[JsonSerializable(typeof(GetUserCountryOrRegionResponse))]
[JsonSerializable(typeof(SteamAddPhoneNumberResponse))]
[JsonSerializable(typeof(IsAccountWaitingForEmailConfirmationResponse))]
[JsonSerializable(typeof(SteamGetRsaKeyJsonStruct))]
[JsonSerializable(typeof(SteamDoLoginHasPhoneJsonStruct))]
[JsonSerializable(typeof(SteamDoLoginTfaJsonStruct))]
[JsonSerializable(typeof(SteamDoLoginFinalizeJsonStruct))]
[JsonSerializable(typeof(SteamConvertSteamDataJsonStruct))]
[JsonSerializable(typeof(SteamMobileDologinJsonStruct))]
[JsonSerializable(typeof(SteamMobileConfGetListJsonStruct))]
[JsonSerializable(typeof(RemoveAuthenticatorResponse))]
[JsonSerializable(typeof(GenerateAccessTokenForAppResponse))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(AchievementAndUnlockTimeResult))]
[JsonSerializable(typeof(CloudArchiveQuotaResult))]
#if !(IOS || ANDROID)
[JsonSerializable(typeof(SteamAppProperty))]
[JsonSerializable(typeof(SteamAppPropertyTable))]
[JsonSerializable(typeof(List<SteamAppProperty>))]
#endif
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
public sealed partial class DefaultJsonSerializerContext_ : JsonSerializerContext
{
    static DefaultJsonSerializerContext_()
    {
        // https://github.com/dotnet/runtime/issues/94135
        s_defaultOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,
        };
        Default = new DefaultJsonSerializerContext_(new JsonSerializerOptions(s_defaultOptions));
    }
}