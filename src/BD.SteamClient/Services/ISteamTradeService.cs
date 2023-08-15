using BD.SteamClient.Models.Trade;

namespace BD.SteamClient.Services;

public interface ISteamTradeService
{
    static ISteamTradeService Instance => Ioc.Get<ISteamTradeService>();

    #region Tasks

    /// <summary>
    /// 启动交易报价后台任务
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="interval"></param>
    /// <param name="tradeTaskEnum"></param>
    void StartTradeTask(string steam_id, int interval, TradeTaskEnum tradeTaskEnum);

    /// <summary>
    /// 停止交易报价后台任务
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="tradeTaskEnum"></param>
    void StopTask(string steam_id, TradeTaskEnum tradeTaskEnum);
    #endregion

    /// <summary>
    /// 接受所有礼品类型报价
    /// </summary>
    /// <returns></returns>
    Task<bool> AcceptAllGiftTradeOfferAsync(string steam_id);

    /// <summary>
    /// 批量处理交易报价
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trades"></param>
    /// <param name="accept"></param>
    /// <returns></returns>
    Task<bool> BatchHandleTradeOfferAsync(string steam_id, Dictionary<string, string> trades, bool accept);

    /// <summary>
    /// 接受交易报价
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trade_offer_id"></param>
    /// <param name="tradeInfo"></param>
    /// <param name="confirmations"></param>
    /// <returns></returns>
    Task<bool> AcceptTradeOfferAsync(string steam_id, string trade_offer_id, TradeInfo? tradeInfo, IEnumerable<Confirmation>? confirmations = null);

    /// <summary>
    /// 发送交易报价（需要好友关系）
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="my_itmes">将失去的物品</param>
    /// <param name="them_items">将获得的物品</param>
    /// <param name="target_steam_id"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> SendTradeOfferAsync(string steam_id, List<Asset> my_itmes, List<Asset> them_items, string target_steam_id, string message);

    /// <summary>
    /// 使用交易连接发送报价
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trade_offer_url"></param>
    /// <param name="my_itmes">将失去的物品</param>
    /// <param name="them_items">将获得的物品</param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> SendTradeOfferWithUrlAsync(string steam_id, string trade_offer_url, List<Asset> my_itmes, List<Asset> them_items, string message);

    /// <summary>
    /// 发送方取消交易报价
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trade_offer_id"></param>
    /// <returns></returns>
    Task<bool> CancelTradeOfferAsync(string steam_id, string trade_offer_id);

    /// <summary>
    /// 接收方拒绝交易报价
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trade_offer_id"></param>
    /// <returns></returns>
    Task<bool> DeclineTradeOfferAsync(string steam_id, string trade_offer_id);

    /// <summary>
    /// 获取发送或接受的交易报价的列表
    /// </summary>
    /// <param name="api_key"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetTradeOffersAsync(string api_key);

    /// <summary>
    /// 根据交易报价 ID 获取报价信息
    /// </summary>
    /// <param name="api_key"></param>
    /// <param name="trade_offer_id"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetTradeOfferAsync(string api_key, string trade_offer_id);

    /// <summary>
    /// 获取待处理的交易报价和新交易报价的数量
    /// </summary>
    /// <param name="api_key"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetTradeOffersSummaryAsync(string api_key);

}
