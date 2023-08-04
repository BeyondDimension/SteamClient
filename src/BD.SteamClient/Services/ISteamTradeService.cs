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
    /// <param name="api_key"></param>
    /// <param name="identity_secret"></param>
    /// <param name="interval"></param>
    /// <param name="tradeTaskEnum"></param>
    void StartTradeTask(SteamSession steamSession, string api_key, string identity_secret, int interval, TradeTaskEnum tradeTaskEnum);

    /// <summary>
    /// 停止交易报价后台任务
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="tradeTaskEnum"></param>
    void StopTask(SteamSession steamSession, TradeTaskEnum tradeTaskEnum);
    #endregion

    /// <summary>
    /// 接受所有礼品类型报价
    /// </summary>
    /// <returns></returns>
    Task<bool> AcceptAllGiftTradeOfferAsync(SteamSession steamSession, string api_key, string identity_secret);

    /// <summary>
    /// 批量处理交易报价
    /// </summary>
    /// <param name="trades"></param>
    /// <param name="accept"></param>
    /// <param name="steam_id"></param>
    /// <param name="identity_secret"></param>
    /// <returns></returns>
    Task<bool> BatchHandleTradeOfferAsync(SteamSession steamSession, string identity_secret, Dictionary<string, string> trades, bool accept);

    /// <summary>
    /// 接受交易报价
    /// </summary>
    /// <returns></returns>
    Task<bool> AcceptTradeOfferAsync(SteamSession steamSession, string identity_secret, string trade_offer_id, TradeInfo? tradeInfo);

    /// <summary>
    /// 发送交易报价
    /// </summary>
    /// <returns></returns>
    Task<bool> SendTradeOfferAsync(SteamSession steamSession, string identity_secret, List<Asset> my_itmes, List<Asset> them_items, string target_steam_id, string message);

    /// <summary>
    /// 发送方取消交易报价
    /// </summary>
    /// <returns></returns>
    Task<bool> CancelTradeOfferAsync(SteamSession steamSession);

    /// <summary>
    /// 接收方拒绝交易报价
    /// </summary>
    /// <returns></returns>
    Task<bool> DeclineTradeOfferAsync(SteamSession steamSession);

    /// <summary>
    /// 获取发送或接受的交易报价的列表
    /// </summary>
    Task<HttpResponseMessage> GetTradeOffersAsync(string api_key);

    /// <summary>
    /// 获取待处理的交易报价和新交易报价的数量
    /// </summary>
    /// <returns></returns>
    Task<HttpResponseMessage> GetTradeOffersSummaryAsync(string api_key);

}
