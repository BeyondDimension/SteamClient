namespace BD.SteamClient.Constants;

public static class SteamApiUrls
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
    #endregion

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

    public const string OpenIdloginUrl = $"{STEAM_COMMUNITY_URL}/openid/login";

    public const string CaptchaImageUrl = $"{STEAM_STORE_URL}/login/rendercaptcha/?gid=";

    public const string SteamStoreRedeemWalletCodelUrl = $"{STEAM_STORE_URL}/account/ajaxredeemwalletcode?l=schinese";

    public const string SteamStoreAccountlUrl = $"{STEAM_STORE_URL}/account?l=schinese";
    public const string SteamStoreAccountHistoryDetailUrl = $"{STEAM_STORE_URL}/account/history?l=schinese";
    public const string SteamStoreAccountHistoryAjaxlUrl = $"{STEAM_STORE_URL}/AjaxLoadMoreHistory?l=schinese";

    public const string SteamStoreAccountSetCountryUrl = $"{STEAM_STORE_URL}/account/setcountry";
    public const string SteamStoreAddFundsUrl = $"{STEAM_STORE_URL}/steamaccount/addfunds?l=schinese";

    public const string AccountGetSteamNotificationsUrl = "https://api.steampowered.com/ISteamNotificationService/GetSteamNotifications/v1?access_token={0}";

    /// <summary>
    /// TradeOffer 交易报价API
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
    /// MOBILECONF 令牌交易确认API
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
    public const string STEAM_AUTHENTICATOR_GET_USERCOUNTRY = "https://api.steampowered.com/IUserAccountService/GetUserCountry/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_SEND_PHONEVERIFICATIONCODE = "https://api.steampowered.com/IPhoneService/IsAccountWaitingForEmailConfirmation/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_TWOFAQUERYTIME = "https://api.steampowered.com/ITwoFactorService/QueryTime/v0001";
    public const string STEAM_AUTHENTICATOR_ADD = "https://api.steampowered.com/ITwoFactorService/AddAuthenticator/v1/?access_token={0}";
    public const string STEAM_AUTHENTICATOR_FINALIZEADD = "https://api.steampowered.com/ITwoFactorService/FinalizeAddAuthenticator/v1/?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REMOVE = "https://api.steampowered.com/ITwoFactorService/RemoveAuthenticator/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGESTARTSYNC = "https://api.steampowered.com/ITwoFactorService/RemoveAuthenticatorViaChallengeStart/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REMOVE_VIACHALLENGECONTINUESYNC = "https://api.steampowered.com/ITwoFactorService/RemoveAuthenticatorViaChallengeContinue/v1?access_token={0}";
    public const string STEAM_AUTHENTICATOR_REFRESHACCESSTOKEN = "https://api.steampowered.com/IAuthenticationService/GenerateAccessTokenForApp/v1/";

    /// <summary>
    /// Idle 挂卡
    /// </summary>
    public const string STEAM_IDLE_APPCARDS_AVG = "https://api.augmentedsteam.com/v2/market/cards/average-prices/?appids={0}&currency={1}";
    public const string STEAM_IDLE_APPCARDS_MARKETPRICE = "https://api.augmentedsteam.com/v2/market/cards/?appid={0}&currency={1}";
}
