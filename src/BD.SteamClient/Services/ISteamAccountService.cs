namespace BD.SteamClient.Services;

public interface ISteamAccountService
{
    /// <summary>
    /// 获取 Steam 登录所需的 RSAkey 来加密 password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<(string encryptedPassword64, string timestamp)> GetRSAkeyAsync(
        string username,
        string password);

    /// <summary>
    /// 新版登录时获取 RSAkey
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<(string encryptedPassword64, ulong timestamp)> GetRSAkeyV2Async(
        string username,
        string password);

    /// <summary>
    /// 执行请求 Steam 登录，返回登录状态
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <param name="isTransfer"></param>
    /// <param name="isDownloadCaptchaImage"></param>
    Task DoLoginAsync(SteamLoginState loginState, bool isTransfer = false, bool isDownloadCaptchaImage = false);

    /// <summary>
    /// 执行新版请求 Steam 登录，返回登录状态
    /// </summary>
    /// <param name="loginState"></param>
    /// <returns></returns>
    Task DoLoginV2Async(SteamLoginState loginState);

    /// <summary>
    /// 执行 Steam 第三方快速登录接口请求并返回登录后 Cookie
    /// </summary>
    /// <param name="openidparams">请求参数</param>
    /// <param name="nonce">nonce</param>
    /// <param name="cookie">登录成功状态</param>
    /// <returns></returns>
    [Obsolete("现已使用 Steam 登录状态在浏览器中登录")]
    Task<CookieCollection?> OpenIdLoginAsync(
        string openidparams,
        string nonce,
        CookieCollection cookie);

    /// <summary>
    /// 获取 Steam 账号消费历史记录
    /// </summary>
    Task<(bool IsSuccess, string? Message, HistoryParseResponse? History)> GetAccountHistoryDetail(SteamLoginState loginState);

    /// <summary>
    /// 获取 Steam 账号余额货币类型及账号区域信息，并检测账号是否定区
    /// </summary>
    Task<bool> GetWalletBalance(SteamLoginState loginState);

    /// <summary>
    /// 调用 Steam 充值卡充值接口
    /// </summary>
    /// <param name="loginState"></param>
    /// <param name="walletCode"></param>
    /// <param name="isRetry">是否重试，在尝试定区的时候可以重试，真正充值时不应重试</param>
    /// <returns></returns>
    Task<(SteamResult Result, PurchaseResultDetail? Detail)?> RedeemWalletCode(
        SteamLoginState loginState,
        string walletCode,
        bool isRetry = false);

    /// <summary>
    /// 设置 Steam 账号区域到指定位置
    /// </summary>
    Task<bool> SetSteamAccountCountry(SteamLoginState loginState, string currencyCode);

    /// <summary>
    /// 获取当前可设置的区域列表
    /// </summary>
    Task<List<CurrencyData>?> GetSteamAccountCountryCodes(SteamLoginState loginState);
}
