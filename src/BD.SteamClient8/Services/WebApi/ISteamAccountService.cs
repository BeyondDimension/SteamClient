#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Services;

/// <summary>
/// Steam 账号相关接口服务
/// </summary>
public interface ISteamAccountService
{
    /// <summary>
    /// 获取新版 Steam 登录所需的 RSAkey 来加密 password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<(string encryptedPassword64, ulong timestamp)>> GetRSAkeyV2Async(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行新版请求 Steam 登录，返回登录状态
    /// </summary>
    /// <param name="loginState"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl> DoLoginV2Async(SteamLoginState loginState, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 Steam 账号消费历史记录
    /// </summary>
    Task<ApiRspImpl<(bool IsSuccess, string? Message, HistoryParseResponse? History)>> GetAccountHistoryDetail(SteamLoginState loginState, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 Steam 账号余额货币类型及账号区域信息，并检测账号是否定区
    /// <param name="loginState">登录状态</param>
    /// <param name="cancellationToken"></param>
    /// </summary>
    Task<ApiRspImpl<bool>> GetWalletBalance(SteamLoginState loginState, CancellationToken cancellationToken = default);

    /// <summary>
    /// 调用 Steam 充值卡充值接口
    /// </summary>
    /// <param name="loginState"></param>
    /// <param name="walletCode"></param>
    /// <param name="isRetry">是否重试，在尝试定区的时候可以重试，真正充值时不应重试</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<(SteamResult Result, PurchaseResultDetail? Detail)?>> RedeemWalletCode(
        SteamLoginState loginState,
        string walletCode,
        bool isRetry = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置 Steam 账号区域到指定位置
    /// <param name="loginState">登录状态</param>
    /// <param name="currencyCode">货币</param>
    /// <param name="cancellationToken"></param>
    /// </summary>
    Task<ApiRspImpl<bool>> SetSteamAccountCountry(SteamLoginState loginState, string currencyCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前可设置的区域列表
    /// <param name="loginState">登录状态</param>
    /// <param name="cancellationToken"></param>
    /// </summary>
    Task<ApiRspImpl<List<CurrencyData>?>> GetSteamAccountCountryCodes(SteamLoginState loginState, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 api key
    /// </summary>
    /// <param name="steamLoginState">登录状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<string?>> GetApiKey(SteamLoginState steamLoginState, CancellationToken cancellationToken = default);

    /// <summary>
    /// 注册 api key
    /// </summary>
    /// <param name="steamLoginState">登录状态</param>
    /// <param name="domain"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<string?>> RegisterApiKey(SteamLoginState steamLoginState, string? domain = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取库存信息 (有频率限制 间隔 3 分钟)
    /// </summary>
    /// <param name="steamId">用户 Steam Id</param>
    /// <param name="appId">应用 Id</param>
    /// <param name="contextId">上下文 Id</param>
    /// <param name="count">获取数量</param>
    /// <param name="startAssetId">开始物品 Id(用于分页)</param>
    /// <param name="language">语言(默认简体中文)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<InventoryPageResponse?>> GetInventories(ulong steamId, string appId, string contextId, int count = 100, string? startAssetId = null, string language = "schinese", CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取库存交易历史
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <param name="appFilter">筛选的 appId 列表</param>
    /// <param name="cursor">分页游标(响应会返回下一次请求的游标)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<InventoryTradeHistoryRenderPageResponse>> GetInventoryTradeHistory(SteamLoginState loginState, int[]? appFilter = null, InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解析库存交易历史 html
    /// </summary>
    /// <param name="html">交易历史 html</param>
    /// <param name="cultureInfo">网页语言信息(目前只有时间按照语言解析)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<InventoryTradeHistoryRow> ParseInventoryTradeHistory(string html, CultureInfo? cultureInfo = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取发送礼物记录
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<IEnumerable<SendGiftHistoryItem>?>> GetSendGiftHistories(SteamLoginState loginState, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取登录历史记录
    /// </summary>
    /// <param name="loginState"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<LoginHistoryItem>? GetLoginHistory(SteamLoginState loginState, CancellationToken cancellationToken = default);

    /// <summary>
    /// 效验当前登录的 accesstoken 是否有效
    /// </summary>
    /// <param name="accesstoken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> CheckAccessTokenValidation(string accesstoken, CancellationToken cancellationToken = default);
}
