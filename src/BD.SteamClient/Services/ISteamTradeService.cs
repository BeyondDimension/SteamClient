using BD.SteamClient.Models.Trade;

namespace BD.SteamClient.Services;

public interface ISteamTradeService
{
    static ISteamTradeService Instance => Ioc.Get<ISteamTradeService>();

    #region Tasks 后台任务

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

    #region Trade 交易报价

    /// <summary>
    /// 接受所有礼品类型报价
    /// </summary>
    /// <returns></returns>
    Task<bool> AcceptAllGiftTradeOfferAsync(string steam_id);

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
    Task<TradeResponse?> GetTradeOffersAsync(string api_key);

    /// <summary>
    /// 根据交易报价 ID 获取报价信息
    /// </summary>
    /// <param name="api_key"></param>
    /// <param name="trade_offer_id"></param>
    /// <returns></returns>
    Task<TradeInfo?> GetTradeOfferAsync(string api_key, string trade_offer_id);

    /// <summary>
    /// 获取待处理的交易报价和新交易报价的数量
    /// </summary>
    /// <param name="api_key"></param>
    /// <returns></returns>
    Task<TradeSummary?> GetTradeOffersSummaryAsync(string api_key);

    /// <summary>
    /// 获取交易历史记录
    /// </summary>
    /// <param name="maxTrades">获取交易最大数量</param>
    /// <param name="startTradeId">要查询列的开始的交易id</param>
    /// <param name="getDescriptions">是否获取交易物品描述</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<TradeHistory.TradeHistoryResponseDetail?> GetTradeHistory(string api_key, int maxTrades = 500, string? startTradeId = null, bool getDescriptions = false);
    #endregion

    #region Confirmation 交易确认

    /// <summary>
    /// 获取交易确认列表
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    Task<IEnumerable<Confirmation>> GetConfirmations(string steam_id);

    /// <summary>
    /// 获取交易确认详细信息
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="confirmation"></param>
    /// <returns></returns>
    Task<(string[] my_items, string[] them_items)> GetComfirmationImages(string steam_id, Confirmation confirmation);

    /// <summary>
    /// 交易确认发送
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="confirmation"></param>
    /// <param name="accept"></param>
    /// <returns></returns>
    Task<bool> SendConfirmation(string steam_id, Confirmation confirmation, bool accept);

    /// <summary>
    /// 批量处理交易确认
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trades"></param>
    /// <param name="accept"></param>
    /// <returns></returns>
    Task<bool> BatchSendConfirmation(string steam_id, Dictionary<string, string> trades, bool accept);
    #endregion
}
