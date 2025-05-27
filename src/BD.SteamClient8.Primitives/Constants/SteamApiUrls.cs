namespace BD.SteamClient8.Constants;

/// <summary>
/// Steam API 相关 URL 常量
/// </summary>
public static partial class SteamApiUrls
{
    public const string MY_PROFILE_URL = "https://steamcommunity.com/profiles/76561198289531723/";
    public const string MY_WISHLIST_URL = "https://store.steampowered.com/wishlist/profiles/76561198289531723/";
    public const string MY_MINIPROFILE_URL = "https://steamcommunity.com/miniprofile/329265995";

    public const string STEAM_PROFILES_URL = "https://steamcommunity.com/profiles/{0}";

    public const string STEAM_LOGIN_URL = "https://steamcommunity.com/login/home/?goto=my/profile";
    public const string STEAM_BADGES_URL = "https://steamcommunity.com/profiles/{0}/badges/?l=schinese&p={1}";
    public const string STEAM_GAMECARDS_URL = "https://steamcommunity.com/profiles/{0}/gamecards/{1}?l=english";
    public const string STEAMAPP_LIST_URL = "https://api.steampowered.com/ISteamApps/GetAppList/v2";
    public const string STEAMAPP_LOGO_URL = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
    public const string STEAMAPP_LIBRARY_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/library_600x900.jpg";
    public const string STEAMAPP_LIBRARYHERO_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/library_hero.jpg";
    public const string STEAMAPP_LIBRARYHEROBLUR_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/library_hero_blur.jpg";
    public const string STEAMAPP_LIBRARYLOGO_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png";

    /// <summary>
    /// 多语言版本 logo，{1} 传入 steam 多语言名称
    /// </summary>
    public const string STEAMAPP_LIBRARYLOGO_LOCALIZED_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo_{1}.png";

    public const string STEAMAPP_ICON_URL = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";
    public const string STEAMAPP_CAPSULE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/capsule_184x69.jpg";
    public const string STEAMAPP_HEADIMAGE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg";
    public const string STEAM_MEDIA_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/movie_max.webm";
    public const string STEAM_VIDEO_URL = "https://store.steampowered.com/video/watch/{0}/";
    public const string STEAM_REGISTERKEY_URL = "https://store.steampowered.com/account/registerkey?key={0}";
    public const string STEAM_FRIENDMESSAGESLOG_URL = "https://help.steampowered.com/zh-cn/accountdata/GetFriendMessagesLog";
    public const string STEAM_REMOTESTORAGE_URL = "https://store.steampowered.com/account/remotestorage";
    public const string STEAM_KEY_URL = "https://store.steampowered.com/account/registerkey?key={0}";

    public const string STEAM_INSTALL_URL = "steam://install/{0}";
    public const string STEAM_RUNGAME_URL = "steam://rungameid/{0}";
    public const string STEAM_NAVGAMEPAGE_URL = "steam://nav/games";
    public const string STEAM_NAVCONSOLE_URL = "steam://nav/console";
    public const string STEAM_NAVGAME_URL = "steam://nav/games/details/{0}";
    public const string STEAM_NAVGAMESCREENSHOTS_URL = "steam://open/screenshots/{0}";
    public const string STEAM_OPENURL = "steam://openurl/{0}";

    #region 第三方链接

    public const string STEAMDB_USERINFO_URL = "https://api.steamdb.ml/v1/users/{0}";
    public const string STEAMDB_APPINFO_URL = "https://api.steamdb.ml/v1/apps/{0}";
    public const string STEAMSTORE_APP_URL = "https://store.steampowered.com/app/{0}";
    public const string STEAMDBINFO_APP_URL = "https://steamdb.info/app/{0}";
    public const string STEAMCARDEXCHANGE_APP_URL = "https://www.steamcardexchange.net/index.php?gamepage-appid-{0}";

    public const string STEAMREP_USER_URL = "https://steamrep.com/search?q={0}";
    public const string STEAMREPCN_USER_URL = "https://steamrepcn.com/profiles/{0}";
    public const string STEAMDB_USER_CALC_URL = "https://steamdb.info/calculator/?player={0}";
    public const string STEAMGIFTS_USER_URL = "https://www.steamgifts.com/go/user/{0}";
    public const string STEAMTRADES_USER_URL = "https://www.steamtrades.com/user/{0}";
    public const string STEAMACHIEVEMENT_STATS_USER_URL = "https://www.achievementstats.com/index.php?action=profile&playerId={0}";
    public const string STEAMBACKPACK_USER_URL = "https://backpack.tf/profiles/{0}";

    #endregion 第三方链接

    /// <summary>
    /// 这里需要 steamid3 而不是 id64
    /// </summary>
    public const string STEAM_MINIPROFILE_URL = "https://steam-chat.com/miniprofile/{0}/json";

    public const string STEAM_USERINFO_XML_URL = "https://steamcommunity.com/profiles/{0}?xml=1";

    public const string STEAMCN_USERINFO_XML_URL = "https://my.steamchina.com/profiles/76561198289531723?xml=1";

    public const string STEAM_COMMUNITY_URL = "https://steamcommunity.com";
    public const string STEAM_STORE_URL = "https://store.steampowered.com";

    public const string GetRSAkeyUrl = $"{STEAM_STORE_URL}/login/getrsakey/";
    public const string DologinUrl = $"{STEAM_STORE_URL}/login/dologin?l=schinese";
    public const string SteamLoginUrl = $"{STEAM_STORE_URL}/login?oldauth=1";

    public const string CaptchaImageUrl = $"{STEAM_STORE_URL}/login/rendercaptcha/?gid=";

    /// <summary>
    /// Login 登录操作相关 API
    /// </summary>

    #region OpenIdLogin

    public const string OpenIdloginUrl = $"{STEAM_COMMUNITY_URL}/openid/login";

    #endregion OpenIdLogin

    public const string SteamStoreRedeemWalletCodelUrl = $"{STEAM_STORE_URL}/account/ajaxredeemwalletcode?l=schinese";

    public const string SteamStoreAccountlUrl = $"{STEAM_STORE_URL}/account?l=schinese";
    public const string SteamStoreAccountHistoryDetailUrl = $"{STEAM_STORE_URL}/account/history?l=schinese";
    public const string SteamStoreAccountHistoryAjaxlUrl = $"{STEAM_STORE_URL}/AjaxLoadMoreHistory?l=schinese";

    public const string SteamStoreAccountSetCountryUrl = $"{STEAM_STORE_URL}/account/setcountry";
    public const string SteamStoreAddFundsUrl = $"{STEAM_STORE_URL}/steamaccount/addfunds?l=schinese";

    public const string AccountGetSteamNotificationsUrl = "https://api.steampowered.com/ISteamNotificationService/GetSteamNotifications/v1?access_token={0}";

    //#region LoginV1
    //public const string GetRSAkeyUrl = $"{STEAM_STORE_URL}/login/getrsakey/";
    //public const string DologinUrl = $"{STEAM_STORE_URL}/login/dologin?l=schinese";
    //public const string SteamLoginUrl = $"{STEAM_STORE_URL}/login?oldauth=1";
    //#endregion

    #region LoginV2

    public const string GetRSAkeyV2Url = "https://api.steampowered.com/IAuthenticationService/GetPasswordRSAPublicKey/v1?input_protobuf_encoded={0}";
    public const string STEAM_LOGIN_CHECKDEVICE = "https://login.steampowered.com/jwt/checkdevice/{0}";
    public const string STEAM_LOGIN_BEGINAUTHSESSIONVIACREDENTIALS = "https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaCredentials/v1";
    public const string STEAM_LOGIN_POLLAUTHSESSIONSTATUS = "https://api.steampowered.com/IAuthenticationService/PollAuthSessionStatus/v1";
    public const string STEAM_LOGIN_UPDATEAUTHSESSIONWITHSTEAMGUARDCODE = "https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1";
    public const string STEAM_LOGIN_FINALIZELOGIN = "https://login.steampowered.com/jwt/finalizelogin";

    #endregion

    /// <summary>
    /// Account 账号相关信息
    /// </summary>
    public const string STEAM_ACCOUNT_REDEEMWALLETCODE = $"{STEAM_STORE_URL}/account/ajaxredeemwalletcode?l=schinese";

    public const string STEAM_ACCOUNT_HISTORY_LOGIN = "https://help.steampowered.com/zh-cn/accountdata/SteamLoginHistory";
    public const string STEAM_ACCOUNT_APIKEY_REGISTER = "https://steamcommunity.com/dev/registerkey";
    public const string STEAM_ACCOUNT_APIKEY_GET = "https://steamcommunity.com/dev/apikey";
    public const string STEAM_ACCOUNT = $"{STEAM_STORE_URL}/account?l=schinese";
    public const string STEAM_ACCOUNT_HISTORY_DETAIL = $"{STEAM_STORE_URL}/account/history?l=schinese";
    public const string STEAM_ACCOUNT_HISTORY_AJAX = $"{STEAM_STORE_URL}/AjaxLoadMoreHistory?l=schinese";

    public const string STEAM_ACCOUNT_SETCOUNTRY = $"{STEAM_STORE_URL}/account/setcountry";
    public const string STEAM_ACCOUNT_ADD_FUNDS = $"{STEAM_STORE_URL}/steamaccount/addfunds?l=schinese";
    public const string STEAM_ACCOUNT_SENDGIFTHISTORIES = "https://steamcommunity.com/gifts/0/history/";
    public const string STEAM_ACCOUNT_GET_STEAMNOTIFICATION = "https://api.steampowered.com/ISteamNotificationService/GetSteamNotifications/v1?access_token={0}";
    public const string STEAM_ACCOUNT_GET_PLAYSUMMARIES = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={0}&steamids={1}";

    /// <summary>
    /// TradeOffer 交易报价 API
    /// </summary>
    public const string STEAM_TRADEOFFER_ACCPET = "https://steamcommunity.com/tradeoffer/{0}/accept";

    public const string STEAM_TRADEOFFER_DECLINE = "https://steamcommunity.com/tradeoffer/{0}/decline";
    public const string STEAM_TRADEOFFER_CANCEL = "https://steamcommunity.com/tradeoffer/{0}/cancel";
    public const string STEAM_TRADEOFFER_GET_SUMMARY = "https://api.steampowered.com/IEconService/GetTradeOffersSummary/v1?key={0}";
    public const string STEAM_TRADEOFFER_GET_OFFERS = "https://api.steampowered.com/IEconService/GetTradeOffers/v1/";
    public const string STEAM_TRADEOFFER_GET_OFFER = "https://api.steampowered.com/IEconService/GetTradeOffer/v1/";
    public const string STEAM_TRADEOFFER_GET_HISTORY = "https://api.steampowered.com/IEconService/GetTradeHistory/v1/";
    public const string STEAM_TRADEOFFER_URL = "https://steamcommunity.com/tradeoffer/{0}";
    public const string STEAM_TRADEOFFER_SEND = "https://steamcommunity.com/tradeoffer/new/send";

    /// <summary>
    /// MOBILECONF 令牌交易确认 API
    /// </summary>
    public const string STEAM_MOBILECONF_CONFIRMATION = "https://steamcommunity.com/mobileconf/ajaxop";

    public const string STEAM_MOBILECONF_BATCH_CONFIRMATION = "https://steamcommunity.com/mobileconf/multiajaxop";
    public const string STEAM_MOBILECONF_GET_CONFIRMATIONS = "https://steamcommunity.com/mobileconf/getlist";
    public const string STEAM_MOBILECONF_GET_CONFIRMATION_DETAILS = "https://steamcommunity.com/mobileconf/details/{0}?l=schinese";

    /// <summary>
    /// Authenticator 令牌
    /// </summary>
    public const string STEAM_AUTHENTICATOR_ACCOUNTWAITINGFOREMAILCONF = "https://api.steampowered.com/IPhoneService/IsAccountWaitingForEmailConfirmation/v1?access_token={0}";

    public const string STEAM_AUTHENTICATOR_ADD_PHONENUMBER = "https://api.steampowered.com/IPhoneService/SetAccountPhoneNumber/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_VERIFY_PHONENUMBER = "https://api.steampowered.com/IPhoneService/VerifyAccountPhoneWithCode/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_GET_USERCOUNTRY = "https://api.steampowered.com/IUserAccountService/GetUserCountry/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_SEND_PHONEVERIFICATIONCODE = "https://api.steampowered.com/IPhoneService/SendPhoneVerificationCode/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_TWOFAQUERYTIME = "https://api.steampowered.com/ITwoFactorService/QueryTime/v0001";
    public const string STEAM_AUTHENTICATOR_ADD = "https://api.steampowered.com/ITwoFactorService/AddAuthenticator/v1/?access_token={0}";
    public const string STEAM_AUTHENTICATOR_FINALIZEADD = "https://api.steampowered.com/ITwoFactorService/FinalizeAddAuthenticator/v1/?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REMOVE = "https://api.steampowered.com/ITwoFactorService/RemoveAuthenticator/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGESTARTSYNC = "https://api.steampowered.com/ITwoFactorService/RemoveAuthenticatorViaChallengeStart/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGECONTINUESYNC = "https://api.steampowered.com/ITwoFactorService/RemoveAuthenticatorViaChallengeContinue/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REFRESHACCESSTOKEN = "https://api.steampowered.com/IAuthenticationService/GenerateAccessTokenForApp/v1/";
    public const string STEAM_AUTHENTICATOR_ACCOUNTPHONESTATUS = "https://api.steampowered.com/IPhoneService/AccountPhoneStatus/v1?access_token={0}";

    /// <summary>
    /// Idle 挂卡
    /// </summary>
    public const string STEAM_IDLE_APPCARDS_AVG = "https://api.augmentedsteam.com/market/cards/average-prices/v2?appids={0}&currency={1}";

    public const string STEAM_IDLE_APPCARDS_MARKETPRICE = "https://api.augmentedsteam.com/market/cards/v2?appid={0}&currency={1}";

    /// <summary>
    /// 家庭监控
    /// </summary>
    public const string STEAM_PARENTAL_UNLOCK_COMMUNITY = "https://steamcommunity.com/parental/ajaxunlock/";

    public const string STEAM_PARENTAL_UNLOCK_STORE = "https://store.steampowered.com/parental/ajaxunlock/";
    public const string STEAM_PARENTAL_UNLOCK_CHECKOUT = "https://checkout.steampowered.com/parental/ajaxunlock/";
    public const string STEAM_PARENTAL_UNLOCK_TV = "https://steam.tv/parental/ajaxunlock";

    public const string STEAM_PARENTAL_LOCK_COMMUNITY = "https://steamcommunity.com/parental/ajaxlock/";
    public const string STEAM_PARENTAL_LOCK_STORE = "https://store.steampowered.com/parental/ajaxlock/";
    public const string STEAM_PARENTAL_LOCK_CHECKOUT = "https://checkout.steampowered.com/parental/ajaxlock/";
    public const string STEAM_PARENTAL_LOCK_TV = "https://steam.tv/parental/ajaxlock";

    /// <summary>
    /// Market 市场交易
    /// </summary>
    public const string STEAM_MARKET_ITEMPRICEOVERVIEW_GET = "https://steamcommunity.com/market/priceoverview/?appid={0}&currency={1}&market_hash_name={2}";

    public const string STEAM_MARKET_ITEMORDERHISTOGRAM_GET = "https://steamcommunity.com/market/itemordershistogram?country={0}&language={1}&currency={2}&item_nameid={3}";
    public const string STEAM_MARKET_SELLITEM = "https://steamcommunity.com/market/sellitem/";
    public const string STEAM_MARKET_TRADING_HISTORY_GET = "https://steamcommunity.com/market/myhistory/render/?query=&start={0}&count={1}";
    public const string STEAM_MARKET = "https://steamcommunity.com/market/";
}