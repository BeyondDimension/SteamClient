#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Enums;

/// <summary>
/// 购买结果详情代码
/// </summary>
public enum PurchaseResultDetail
{
    NoDetail = 0,

    /// <summary>
    /// AVS 验证失败
    /// </summary>
    AVSFailure = 1,

    /// <summary>
    /// 资金不足
    /// </summary>
    InsufficientFunds = 2,

    /// <summary>
    /// 联系客服
    /// </summary>
    ContactSupport = 3,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 4,

    /// <summary>
    /// 无效的程序包
    /// </summary>
    InvalidPackage = 5,

    /// <summary>
    /// 无效的支付方式
    /// </summary>
    InvalidPaymentMethod = 6,

    /// <summary>
    /// 无效的数据
    /// </summary>
    InvalidData = 7,

    /// <summary>
    ///  其他操作正在进行中
    /// </summary>
    OthersInProgress = 8,

    /// <summary>
    /// 已经购买过
    /// </summary>
    AlreadyPurchased = 9,

    /// <summary>
    /// 价格错误
    /// </summary>
    WrongPrice = 10,

    /// <summary>
    /// 诈骗检查失败
    /// </summary>
    FraudCheckFailed = 11,

    /// <summary>
    /// 用户取消
    /// </summary>
    CancelledByUser = 12,

    /// <summary>
    /// 受限制的国家
    /// </summary>
    RestrictedCountry = 13,

    /// <summary>
    /// 激活码无效
    /// </summary>
    BadActivationCode = 14,

    /// <summary>
    /// 重复的激活码
    /// </summary>
    DuplicateActivationCode = 15,

    /// <summary>
    /// 使用其他支付方式
    /// </summary>
    UseOtherPaymentMethod = 16,

    /// <summary>
    /// 使用其他功能来源
    /// </summary>
    UseOtherFunctionSource = 17,

    /// <summary>
    /// 无效的收货地址
    /// </summary>
    InvalidShippingAddress = 18,

    /// <summary>
    /// 不支持的地区
    /// </summary>
    RegionNotSupported = 19,

    /// <summary>
    /// 帐户被封锁
    /// </summary>
    AcctIsBlocked = 20,

    /// <summary>
    /// 帐户未验证
    /// </summary>
    AcctNotVerified = 21,

    /// <summary>
    /// 无效的帐户
    /// </summary>
    InvalidAccount = 22,

    /// <summary>
    /// 商店与账单所在国家不匹配
    /// </summary>
    StoreBillingCountryMismatch = 23,

    /// <summary>
    /// 没有所需应用程序
    /// </summary>
    DoesNotOwnRequiredApp = 24,

    /// <summary>
    /// 被新交易取消
    /// </summary>
    CanceledByNewTransaction = 25,

    /// <summary>
    /// 强制取消挂起
    /// </summary>
    ForceCanceledPending = 26,

    /// <summary>
    /// 货币转换提供商失败
    /// </summary>
    FailCurrencyTransProvider = 27,

    FailedCyberCafe = 28,

    /// <summary>
    /// 需要预批准
    /// </summary>
    NeedsPreApproval = 29,

    /// <summary>
    /// 预批准被拒绝
    /// </summary>
    PreApprovalDenied = 30,

    /// <summary>
    /// 钱包货币不匹配
    /// </summary>
    WalletCurrencyMismatch = 31,

    /// <summary>
    /// 邮箱未验证
    /// </summary>
    EmailNotValidated = 32,

    /// <summary>
    /// 卡片过期
    /// </summary>
    ExpiredCard = 33,

    /// <summary>
    /// 交易过期
    /// </summary>
    TransactionExpired = 34,

    /// <summary>
    /// 将超出最大钱包金额
    /// </summary>
    WouldExceedMaxWallet = 35,

    /// <summary>
    /// 必须登录 PS3 应用程序才能购买
    /// </summary>
    MustLoginPS3AppForPurchase = 36,

    /// <summary>
    /// 无法发货至邮政信箱
    /// </summary>
    CannotShipToPOBox = 37,

    /// <summary>
    /// 库存不足
    /// </summary>
    InsufficientInventory = 38,

    /// <summary>
    /// 无法赠送已发货商品
    /// </summary>
    CannotGiftShippedGoods = 39,

    /// <summary>
    /// 无法国际发货
    /// </summary>
    CannotShipInternationally = 40,

    /// <summary>
    /// 计费协议已取消
    /// </summary>
    BillingAgreementCancelled = 41,

    /// <summary>
    /// 无效的优惠券
    /// </summary>
    InvalidCoupon = 42,

    /// <summary>
    /// 过期的优惠券
    /// </summary>
    ExpiredCoupon = 43,

    /// <summary>
    /// 帐户被锁定
    /// </summary>
    AccountLocked = 44,

    /// <summary>
    /// 其他可终止操作正在进行中
    /// </summary>
    OtherAbortableInProgress = 45,

    /// <summary>
    /// 超过 Steam 限制
    /// </summary>
    ExceededSteamLimit = 46,

    /// <summary>
    /// 购物车中有重叠的包裹
    /// </summary>
    OverlappingPackagesInCart = 47,

    /// <summary>
    /// 没有钱包
    /// </summary>
    NoWallet = 48,

    /// <summary>
    /// 没有缓存的支付方式
    /// </summary>
    NoCachedPaymentMethod = 49,

    /// <summary>
    /// 无法从客户端兑换代码
    /// </summary>
    CannotRedeemCodeFromClient = 50,

    /// <summary>
    /// 供应商不支持购买金额
    /// </summary>
    PurchaseAmountNoSupportedByProvider = 51,

    /// <summary>
    /// 待处理交易中存在重叠的包裹
    /// </summary>
    OverlappingPackagesInPendingTransaction = 52,

    /// <summary>
    /// 速率限制
    /// </summary>
    RateLimited = 53,

    /// <summary>
    /// 拥有被排除的应用程序
    /// </summary>
    OwnsExcludedApp = 54,

    /// <summary>
    /// 信用卡 BIN 与类型不匹配
    /// </summary>
    CreditCardBinMismatchesType = 55,

    /// <summary>
    /// 购物车价值过高
    /// </summary>
    CartValueTooHigh = 56,

    /// <summary>
    /// 计费协议已存在
    /// </summary>
    BillingAgreementAlreadyExists = 57,

    /// <summary>
    /// POSA 代码未激活
    /// </summary>
    POSACodeNotActivated = 58,

    /// <summary>
    /// 无法发货至该国家
    /// </summary>
    CannotShipToCountry = 59,

    /// <summary>
    /// 挂起的交易已取消
    /// </summary>
    HungTransactionCancelled = 60,

    /// <summary>
    /// PayPal 内部错误
    /// </summary>
    PaypalInternalError = 61,

    /// <summary>
    /// 未知的 GlobalCollect 错误
    /// </summary>
    UnknownGlobalCollectError = 62,

    /// <summary>
    /// 无效的税务地址
    /// </summary>
    InvalidTaxAddress = 63,

    /// <summary>
    /// 超过实物产品限制
    /// </summary>
    PhysicalProductLimitExceeded = 64,

    /// <summary>
    /// 无法重播购买
    /// </summary>
    PurchaseCannotBeReplayed = 65,

    /// <summary>
    /// 延迟完成
    /// </summary>
    DelayedCompletion = 66,

    /// <summary>
    /// 无法赠送的捆绑类型
    /// </summary>
    BundleTypeCannotBeGifted = 67,

    /// <summary>
    /// 被美国政府封锁
    /// </summary>
    BlockedByUSGov = 68,

    /// <summary>
    /// 商品已预留用于商业用途
    /// </summary>
    ItemsReservedForCommercialUse = 69,

    /// <summary>
    /// 赠品已拥有
    /// </summary>
    GiftAlreadyOwned = 70,

    /// <summary>
    /// 赠品对于接收者地区无效
    /// </summary>
    GiftInvalidForRecipientRegion = 71,

    /// <summary>
    /// 赠品定价不平衡
    /// </summary>
    GiftPricingImbalance = 72,

    /// <summary>
    /// 未指定赠品接收者
    /// </summary>
    GiftRecipientNotSpecified = 73,

    /// <summary>
    /// 商品不允许用于商业用途
    /// </summary>
    ItemsNotAllowedForCommercialUse = 74,

    /// <summary>
    /// 商店国家代码不匹配
    /// </summary>
    BusinessStoreCountryCodeMismatch = 75,

    UserAssociatedWithManyCafes = 76,

    UserNotAssociatedWithCafe = 77,

    /// <summary>
    /// 地址无效
    /// </summary>
    AddressInvalid = 78,

    /// <summary>
    /// 信用卡卡号无效
    /// </summary>
    CreditCardNumberInvalid = 79,

    /// <summary>
    /// 无法发货至军事邮局
    /// </summary>
    CannotShipToMilitaryPostOffice = 80,

    /// <summary>
    /// 支付姓名无效，类似信用卡
    /// </summary>
    BillingNameInvalidResemblesCreditCard = 81,

    /// <summary>
    /// 支付方式暂时不可用
    /// </summary>
    PaymentMethodTemporarilyUnavailable = 82,

    /// <summary>
    /// 支付方式不支持该产品
    /// </summary>
    PaymentMethodNotSupportedForProduct = 83,
}
