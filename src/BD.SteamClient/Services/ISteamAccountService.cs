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
    /// <param name="platformType">显式选择 MobileApp 时仅获取移动令牌，不创建网页登录 Cookie；默认保留现有行为。</param>
    /// <param name="cancellationToken">移动端账号验证的取消令牌。</param>
    /// <returns></returns>
    Task DoLoginV2Async(SteamLoginState loginState, EAuthTokenPlatformType? platformType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询已扫描的登录会话信息。调用会让远程会话感知当前账号，必须先获得用户的扫码和账号选择意图。
    /// </summary>
    Task<CAuthentication_GetAuthSessionInfo_Response> GetQrAuthSessionInfoAsync(
        string accessToken,
        SteamLoginQrChallenge challenge,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用 Steam Guard shared_secret 确认登录；只可在用户检查会话信息并明确同意后调用。
    /// </summary>
    Task ApproveQrAuthSessionAsync(
        string accessToken,
        SteamLoginQrChallenge challenge,
        ulong steamId,
        byte[] sharedSecret,
        bool persistent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// QRCode 登录
    /// </summary>
    /// <param name="loginState">登陆状态</param>
    /// <param name="processSessionFunc">QRCode 登录会话处理委托,可通过会话信息生成二维码 返回 true 表示开始确认登陆状态 false 表示取消</param>
    /// <param name="deviceFriendlyName"></param>
    /// <param name="gamingDeviceType"></param>
    /// <param name="osType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DoLoginWithQRCodeAsync(
            SteamLoginState loginState,
            Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>> processSessionFunc,
            string? deviceFriendlyName = null,
            uint gamingDeviceType = default,
            int osType = 20,
            CancellationToken cancellationToken = default
        );

    /// <summary>
    /// QRCode 登录并显式选择 WebBrowser 或 MobileApp 令牌；两种平台均完成网页登录 Cookie 传输。
    /// </summary>
    /// <param name="loginState">登录状态。</param>
    /// <param name="processSessionFunc">显示二维码并返回是否继续等待；二维码更新时会再次调用。</param>
    /// <param name="platformType">WebBrowser 用于网页登录，MobileApp 用于需要移动端权限的令牌操作。</param>
    /// <param name="deviceFriendlyName"></param>
    /// <param name="gamingDeviceType"></param>
    /// <param name="osType"></param>
    /// <param name="cancellationToken"></param>
    Task DoLoginWithQRCodeAsync(
            SteamLoginState loginState,
            Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>> processSessionFunc,
            EAuthTokenPlatformType platformType,
            string? deviceFriendlyName = null,
            uint gamingDeviceType = default,
            int osType = 20,
            CancellationToken cancellationToken = default
        );

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

    /// <summary>
    /// 检查账号是否绑定了手机号
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    Task<bool?> CheckAccountPhoneStatus(string accessToken);

    //Task<string?> RefreshAccessToken(ulong steamId, string refreshToken);
    Task<string?> GetAccessToken(
            string sessionId,
            string steamLoginSecure,
            string timezoneOffset);

    Task<string?> RefreshAccessToken(ulong steamId, string refreshToken);

    bool IsAccessTokenValid(string accessToken);
}
