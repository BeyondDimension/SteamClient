using BD.Common8.Models;
using BD.SteamClient8.Models.WebApi.Logins;
using BD.SteamClient8.Models.WebApi.Markets;

namespace BD.SteamClient8.Services.Abstractions.WebApi;

/// <summary>
/// Steam 市场交易相关服务
/// </summary>
public interface ISteamMarketService
{
    /// <summary>
    /// 获取市场物品价格概述
    /// </summary>
    /// <param name="appId">应用 Id</param>
    /// <param name="marketHashName">物品市场名称</param>
    /// <param name="currency">Steam 货币代码</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MarketItemPriceOverviewResponse?> GetMarketItemPriceOverview(string appId, string marketHashName, int currency = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取市场订单柱状图数据
    /// </summary>
    /// <param name="marketItemNameId">市场物品 Id</param>
    /// <param name="country"></param>
    /// <param name="currency"></param>
    /// <param name="language"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MarketItemOrdersHistogramResponse?> GetMarketItemOrdersHistogram(long marketItemNameId, string country = "CN", int currency = 23, string language = "schinese", CancellationToken cancellationToken = default);

    /// <summary>
    /// 出售物品到市场
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <param name="appId">应用 Id</param>
    /// <param name="contextId">上下文 Id</param>
    /// <param name="assetId">库存物品 Id</param>
    /// <param name="amount">数量</param>
    /// <param name="price">价格 (货币为当前账号钱包货币,单位:分)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SellItemToMarketResponse?> SellItemToMarket(SteamLoginState loginState, string appId, string contextId, long assetId, int amount, int price, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取市场交易历史
    /// </summary>
    /// <param name="loginState">登录状态</param>
    /// <param name="start">从多少条开始 (跳过条数)</param>
    /// <param name="count" >获取多少条 (最大:500)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<MarketTradingHistoryRenderItem> GetMarketTradingHistory(SteamLoginState loginState, int start = 0, int count = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取市场出售信息
    /// </summary>
    /// <param name="loginState"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MarketListings?> GetMarketListing(SteamLoginState loginState, CancellationToken cancellationToken = default);
}
