namespace BD.SteamClient.Enums;

public enum TradeOfferState
{
    /// <summary>
    /// 已失效
    /// </summary>
    Invalid = 1,

    /// <summary>
    /// 有效的
    /// </summary>
    Active = 2,

    /// <summary>
    /// 已接受
    /// </summary>
    Accepted = 3,

    /// <summary>
    /// 交易遭到调整
    /// </summary>
    Countered = 4,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 5,

    /// <summary>
    /// 已取消
    /// </summary>
    Canceled = 6,

    /// <summary>
    /// 拒绝
    /// </summary>
    Declined = 7,

    /// <summary>
    /// 报价物品已失效
    /// </summary>
    InvalidItems = 8,

    /// <summary>
    /// 待确认
    /// </summary>
    ConfirmationNeed = 9,

    /// <summary>
    /// 按次要因素取消
    /// </summary>
    CanceledBySecondaryFactor = 10,

    /// <summary>
    /// 托管状态
    /// </summary>
    StateInEscrow = 11
}
