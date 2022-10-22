using BD.SteamClient.Enums;
using BD.SteamClient.Models;

namespace BD.SteamClient.Services;

public interface ISteamAccountService
{
    /// <summary>
    /// 获取Steam登录所需的RSAkey来加密password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<(string encryptedPassword64, string timestamp)> GetRSAkeyAsync(string username, string password);

    /// <summary>
    /// 执行请求Steam登录，返回登录状态
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <exception cref="Exception"></exception>
    Task DoLoginAsync(SteamLoginResponse loginState, bool isTransfer = false, bool isDownloadCaptchaImage = false);

    /// <summary>
    /// 执行新版请求Steam登录，返回登录状态
    /// </summary>
    /// <param name="loginState"></param>
    /// <returns></returns>
    Task DoLoginV2Async(SteamLoginResponse loginState);

    /// <summary>
    /// 新版登录时获取RSAkey
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<(string encryptedPassword64, ulong timestamp)> GetRSAkeyV2Async(string username, string password);

    /// <summary>
    /// 执行Steam第三方快速登录接口请求并返回登录后Cookie
    /// </summary>
    /// <param name="openidparams">请求参数</param>
    /// <param name="nonce">nonce</param>
    /// <param name="cookie">登录成功状态</param>
    /// <returns></returns>
    Task<CookieCollection?> OpenIdLoginAsync(string openidparams, string nonce, CookieCollection cookie);

    /// <summary>
    /// 获取Steam账号消费历史记录
    /// </summary>
    Task<(bool IsSuccess, string? Message, HistoryParseResponse? History)> GetAccountHistoryDetail(SteamLoginResponse loginState);

    /// <summary>
    /// 获取Steam账号余额货币类型及账号区域信息,并检测账号是否定区
    /// </summary>
    Task<bool> GetWalletBalance(SteamLoginResponse loginState);

    /// <summary>
    /// 调用Steam充值卡充值接口
    /// </summary>
    Task<(SteamResult Result, PurchaseResultDetail? Detail)?> RedeemWalletCode(SteamLoginResponse loginState, string walletCode);

    /// <summary>
    /// 设置Steam账号区域到指定位置
    /// </summary>
    Task<bool> SetSteamAccountCountry(SteamLoginResponse loginState, string currencyCode);

    /// <summary>
    /// 获取当前可设置得区域列表
    /// </summary>
    Task<List<CurrencyData>?> GetSteamAccountCountryCodes(SteamLoginResponse loginState);

    public static class Urls
    {
        public const string SteamCommunity = "https://steamcommunity.com";
        public const string SteamStore = "https://store.steampowered.com";

        public const string GetRSAkeyUrl = $"{SteamStore}/login/getrsakey/";
        public const string DologinUrl = $"{SteamStore}/login/dologin?l=schinese";
        public const string SteamLoginUrl = $"{SteamStore}/login?oldauth=1";

        public const string OpenIdloginUrl = $"{SteamCommunity}/openid/login";

        public const string CaptchaImageUrl = $"{SteamStore}/login/rendercaptcha/?gid=";

        public const string SteamStoreRedeemWalletCodelUrl = $"{SteamStore}/account/ajaxredeemwalletcode?l=schinese";

        public const string SteamStoreAccountlUrl = $"{SteamStore}/account?l=schinese";
        public const string SteamStoreAccountHistoryDetailUrl = $"{SteamStore}/account/history?l=schinese";
        public const string SteamStoreAccountHistoryAjaxlUrl = $"{SteamStore}/AjaxLoadMoreHistory?l=schinese";

        public const string SteamStoreAccountSetCountryUrl = $"{SteamStore}/account/setcountry";
        public const string SteamStoreAddFundsUrl = $"{SteamStore}/steamaccount/addfunds?l=schinese";
    }

    public const string default_donotache = "-62135596800000"; // default(DateTime).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString();
}
