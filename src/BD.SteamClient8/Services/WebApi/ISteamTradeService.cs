namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// Steam 交易报价相关服务
/// </summary>
public interface ISteamTradeService
{
    /// <summary>
    /// 获取当前服务实例
    /// </summary>
    static ISteamTradeService Instance => Ioc.Get<ISteamTradeService>();

    #region Tasks 后台任务

    /// <summary>
    /// 启动交易报价后台任务
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="interval"></param>
    /// <param name="tradeTaskEnum"></param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl> StartTradeTask(string steam_id, TimeSpan interval, TradeTaskEnum tradeTaskEnum, CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止交易报价后台任务
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="tradeTaskEnum"></param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl> StopTask(string steam_id, TradeTaskEnum tradeTaskEnum, CancellationToken cancellationToken = default);

    #endregion

    #region Trade 交易报价

    /// <summary>
    /// 接受所有礼品类型报价
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> AcceptAllGiftTradeOfferAsync(string steam_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 接受交易报价
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="trade_offer_id"></param>
    /// <param name="tradeInfo"></param>
    /// <param name="confirmations"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> AcceptTradeOfferAsync(string steam_id, string trade_offer_id, TradeOffersInfo? tradeInfo, IEnumerable<TradeConfirmation>? confirmations = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送交易报价（需要好友关系）
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="my_items">将失去的物品</param>
    /// <param name="them_items">将获得的物品</param>
    /// <param name="target_steam_id"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> SendTradeOfferAsync(string steam_id, IEnumerable<TradeAsset> my_items, IEnumerable<TradeAsset> them_items, string target_steam_id, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用交易连接发送报价
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="trade_offer_url"></param>
    /// <param name="my_items">将失去的物品</param>
    /// <param name="them_items">将获得的物品</param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> SendTradeOfferWithUrlAsync(string steam_id, string trade_offer_url, IEnumerable<TradeAsset> my_items, IEnumerable<TradeAsset> them_items, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送方取消交易报价
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="trade_offer_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> CancelTradeOfferAsync(string steam_id, string trade_offer_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 接收方拒绝交易报价
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="trade_offer_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> DeclineTradeOfferAsync(string steam_id, string trade_offer_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取发送或接受的交易报价的列表
    /// </summary>
    /// <param name="api_key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<TradeOffersResponse?>> GetTradeOffersAsync(string api_key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据交易报价 ID 获取报价信息
    /// </summary>
    /// <param name="api_key"></param>
    /// <param name="trade_offer_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<TradeOffersInfo?>> GetTradeOfferAsync(string api_key, string trade_offer_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取待处理的交易报价和新交易报价的数量
    /// </summary>
    /// <param name="api_key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<TradeSummary?>> GetTradeOffersSummaryAsync(string api_key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取交易历史记录
    /// </summary>
    /// <param name="api_key">用户 API 密钥</param>
    /// <param name="maxTrades">获取交易最大数量</param>
    /// <param name="startTradeId">要查询列的开始的交易id</param>
    /// <param name="getDescriptions">是否获取交易物品描述</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<TradeHistory.TradeHistoryResponseDetail?>> GetTradeHistory(string api_key, int maxTrades = 500, string? startTradeId = null, bool getDescriptions = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 过滤出有状态活跃的 交易报价进行操作
    /// </summary>
    static TradeOffersResponse FilterNonActiveOffers(TradeOffersResponse tradeResponse)
    {
        if (tradeResponse?.Response?.TradeOffersSent != null)
            tradeResponse.Response.TradeOffersSent = tradeResponse.Response.TradeOffersSent.Where(x => x.TradeOfferState == TradeOfferState.Active).ToList();

        if (tradeResponse?.Response?.TradeOffersReceived != null)
            tradeResponse.Response.TradeOffersReceived = tradeResponse.Response.TradeOffersReceived.Where(x => x.TradeOfferState == TradeOfferState.Active).ToList();

        return tradeResponse!;
    }

    #endregion

    #region Confirmation 交易确认

    /// <summary>
    /// 获取交易确认列表
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<IEnumerable<TradeConfirmation>>> GetConfirmations(string steam_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取交易确认详细信息
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="confirmation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<(string[] my_items, string[] them_items)>> GetConfirmationImages(string steam_id, TradeConfirmation confirmation, CancellationToken cancellationToken = default);

    /// <summary>
    /// 交易确认发送
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="confirmation"></param>
    /// <param name="accept"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> SendConfirmation(string steam_id, TradeConfirmation confirmation, bool accept, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量处理交易确认
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="trades"></param>
    /// <param name="accept"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> BatchSendConfirmation(string steam_id, Dictionary<string, string> trades, bool accept, CancellationToken cancellationToken = default);

    #endregion
}
