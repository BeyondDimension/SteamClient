using BD.SteamClient.Models.Profile;

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

    /// <summary>
    /// 获取 api key
    /// </summary>
    /// <param name="steamLoginState">登录状态</param>
    /// <returns></returns>
    Task<string?> GetApiKey(SteamLoginState steamLoginState);

    /// <summary>
    /// 注册api key
    /// </summary>
    /// <param name="steamLoginState">登录状态</param>
    /// <returns></returns>
    Task<string?> RegisterApiKey(SteamLoginState steamLoginState, string? domain = null);

    /// <summary>
    /// 获取库存信息 (有频率限制 间隔3分钟)
    /// </summary>
    /// <param name="steamId">用户 Steam Id</param>
    /// <param name="appId">应用 Id</param>
    /// <param name="contextId">上下文 Id</param>
    /// <param name="count">获取数量</param>
    /// <param name="startAssetId">开始物品 Id(用于分页)</param>
    /// <param name="language">语言(默认简体中文)</param>
    /// <returns></returns>
    Task<InventoryPageResponse> GetInventories(ulong steamId, string appId, string contextId, int count = 100, string? startAssetId = null, string language = "schinese");

    /// <summary>
    /// 获取库存交易历史
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <param name="appFilter">筛选的appId列表</param>
    /// <param name="cursor">分页游标(响应会返回下一次请求的游标)</param>
    /// <returns></returns>
    Task<InventoryTradeHistoryRenderPageResponse> GetInventoryTradeHistory(SteamLoginState loginState, int[]? appFilter = null, InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null);

    /// <summary>
    /// 解析库存交易历史html
    /// </summary>
    /// <param name="html">交易历史html</param>
    /// <param name="cultureInfo">网页语言信息(目前只有时间按照语言解析)</param>
    /// <returns></returns>
    IAsyncEnumerable<InventoryTradeHistoryRow> ParseInventoryTradeHistory(string html, CultureInfo? cultureInfo = null);

    /// <summary>
    /// 获取发送礼物记录
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <returns></returns>
    Task<IEnumerable<SendGiftHisotryItem>> GetSendGiftHisotries(SteamLoginState loginState);

    /// <summary>
    /// 获取登录历史记录
    /// </summary>
    /// <param name="loginState"></param>
    /// <returns></returns>
    IAsyncEnumerable<LoginHistoryItem>? GetLoginHistory(SteamLoginState loginState);

    /// <summary>
    /// 效验当前登录的accesstoken是否有效
    /// </summary>
    /// <param name="accesstoken"></param>
    /// <returns></returns>
    Task<bool> CheckAccessTokenValidation(string accesstoken);
}
