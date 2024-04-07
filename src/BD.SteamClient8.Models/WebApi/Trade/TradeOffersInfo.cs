namespace BD.SteamClient8.Models;

// https://partner.steamgames.com/doc/webapi/IEconService

/// <summary>
/// 交易详情
/// </summary>
public record class TradeOffersInfo
{
    /// <summary>
    /// 交易报价 Id
    /// </summary>
    [SystemTextJsonProperty("tradeofferid")]
    public string TradeOfferId { get; set; } = string.Empty;

    /// <summary>
    /// 交易对象 Id
    /// </summary>
    [SystemTextJsonProperty("accountid_other")]
    public long AccountIdOther { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [SystemTextJsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 交易截至日期
    /// </summary>
    [SystemTextJsonProperty("expiration_time")]
    public long ExpirationTime { get; set; }

    /// <summary>
    /// 交易状态
    /// </summary>
    [SystemTextJsonProperty("trade_offer_state")]
    public TradeOfferState TradeOfferState { get; set; }

    /// <summary>
    /// 失去的物品集合
    /// </summary>
    [SystemTextJsonProperty("items_to_give")]
    public List<TradeOffersItem>? ItemsToGive { get; set; }

    /// <summary>
    /// 接收到的物品集合
    /// </summary>
    [SystemTextJsonProperty("items_to_receive")]
    public List<TradeOffersItem>? ItemsToReceive { get; set; }

    /// <summary>
    /// 是否自己的交易报价
    /// </summary>
    [SystemTextJsonProperty("is_our_offer")]
    public bool IsOurOffer { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SystemTextJsonProperty("time_created")]
    public long TimeCreated { get; set; }

    /// <summary>
    /// 日期更新时间
    /// </summary>
    [SystemTextJsonProperty("time_updated")]
    public long TimeUpdated { get; set; }

    /// <summary>
    /// 真实交易日期
    /// </summary>
    [SystemTextJsonProperty("from_real_time_trade")]
    public bool FromRealTimeTrade { get; set; }

    /// <summary>
    /// 第三方保管金额截至日期
    /// </summary>
    [SystemTextJsonProperty("escrow_end_date")]
    public int EscrowEndDate { get; set; }

    /// <summary>
    /// 确认方式
    /// </summary>
    [SystemTextJsonProperty("confirmation_method")]
    public int ConfirmationMethod { get; set; }

    /// <summary>
    /// 结果
    /// </summary>
    [SystemTextJsonProperty("eresult")]
    public int Eresult { get; set; }
}

/// <summary>
/// 交易报价返回
/// </summary>
public partial class TradeOffersResponse : JsonModel
{
    /// <summary>
    /// 交易报价返回信息详情
    /// </summary>
    [SystemTextJsonProperty("response")]
    public TradeOffersResponseInfo? Response { get; set; }
}

/// <summary>
/// 交易报价返回信息详情
/// </summary>
public record class TradeOffersResponseInfo
{
    /// <summary>
    /// 失去的物品集合
    /// </summary>
    [SystemTextJsonProperty("trade_offers_sent")]
    public List<TradeOffersInfo>? TradeOffersSent { get; set; }

    /// <summary>
    /// 收到的物品集合
    /// </summary>
    [SystemTextJsonProperty("trade_offers_received")]
    public List<TradeOffersInfo>? TradeOffersReceived { get; set; }

    /// <summary>
    /// 下一个光标
    /// </summary>
    [SystemTextJsonProperty("next_cursor")]
    public int NextCursor { get; set; }
}
