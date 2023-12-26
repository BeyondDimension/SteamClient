namespace BD.SteamClient8.Primitives.Enums.WebApi;

/// <summary>
/// Steam 错误结果代码
/// </summary>
public enum SteamResult
{
    Invalid = 0,

    /// <summary>
    /// 成功
    /// </summary>
    OK = 1,

    /// <summary>
    /// 一般故障
    /// </summary>
    Fail = 2,

    /// <summary>
    ///  Steam 客户端没有连接后端
    /// </summary>
    NoConnection = 3,

    /// <summary>
    /// 密码/票证无效
    /// </summary>
    InvalidPassword = 5,

    /// <summary>
    /// 同一用户在其他地方登录
    /// </summary>
    LoggedInElsewhere = 6,

    /// <summary>
    /// 协议版本不正确
    /// </summary>
    InvalidProtocolVer = 7,

    /// <summary>
    /// 参数不正确
    /// </summary>
    InvalidParam = 8,

    /// <summary>
    /// 未找到文件
    /// </summary>
    FileNotFound = 9,

    /// <summary>
    /// 调用的方法 Busy - 未执行操作
    /// </summary>
    Busy = 10,

    /// <summary>
    /// 被调用对象处于无效状态
    /// </summary>
    InvalidState = 11,

    /// <summary>
    /// 名称无效
    /// </summary>
    InvalidName = 12,

    /// <summary>
    /// 电子邮件无效
    /// </summary>
    InvalidEmail = 13,

    /// <summary>
    /// 名称不是唯一的
    /// </summary>
    DuplicateName = 14,

    /// <summary>
    /// 访问被拒绝
    /// </summary>
    AccessDenied = 15,

    /// <summary>
    /// 操作超时
    /// </summary>
    Timeout = 16,

    /// <summary>
    /// VAC2 被禁止
    /// </summary>
    Banned = 17,

    /// <summary>
    /// 未找到帐户
    /// </summary>
    AccountNotFound = 18,

    /// <summary>
    /// steamID 无效
    /// </summary>
    InvalidSteamID = 19,

    /// <summary>
    /// 请求的服务当前不可用
    /// </summary>
    ServiceUnavailable = 20,

    /// <summary>
    /// 用户未登录
    /// </summary>
    NotLoggedOn = 21,

    /// <summary>
    /// 请求正在等待（可能正在处理中，或正在等待第三方）
    /// </summary>
    Pending = 22,

    /// <summary>
    /// 加密或解密失败
    /// </summary>
    EncryptionFailure = 23,

    /// <summary>
    /// 权限不足
    /// </summary>
    InsufficientPrivilege = 24,

    /// <summary>
    /// 过量
    /// </summary>
    LimitExceeded = 25,

    /// <summary>
    /// 访问权限已被撤销（用于已撤销的访客通行证）
    /// </summary>
    Revoked = 26,

    /// <summary>
    /// 用户尝试访问的许可证/访客通行证已过期
    /// </summary>
    Expired = 27,

    /// <summary>
    /// 访客通行证已通过帐户兑换，无法再次确认
    /// </summary>
    AlreadyRedeemed = 28,

    /// <summary>
    /// 该请求是重复的，并且该操作在过去已经发生过，这次被忽略
    /// </summary>
    DuplicateRequest = 29,

    /// <summary>
    /// 此访客通行证兑换请求中的所有游戏都已归用户所有
    /// </summary>
    AlreadyOwned = 30,

    /// <summary>
    /// 未找到 IP 地址
    /// </summary>
    IPNotFound = 31,

    /// <summary>
    /// 无法将更改写入数据存储
    /// </summary>
    PersistFailed = 32,

    /// <summary>
    /// 为此操作获取访问锁定失败
    /// </summary>
    LockingFailed = 33,

    /// <summary>
    /// 登录会话已被替换
    /// </summary>
    LogonSessionReplaced = 34,

    /// <summary>
    /// 连接失败
    /// </summary>
    ConnectFailed = 35,

    /// <summary>
    /// 验证握手失败
    /// </summary>
    HandshakeFailed = 36,

    /// <summary>
    /// 出现一般 IO 失败
    /// </summary>
    IOFailure = 37,

    /// <summary>
    /// 远程服务器已断开连接
    /// </summary>
    RemoteDisconnect = 38,

    /// <summary>
    /// 未能找到请求的购物车
    /// </summary>
    ShoppingCartNotFound = 39,

    /// <summary>
    /// 用户不允许
    /// </summary>
    Blocked = 40,

    /// <summary>
    /// 目标忽略发件人
    /// </summary>
    Ignored = 41,

    /// <summary>
    /// 未找到与请求匹配的内容
    /// </summary>
    NoMatch = 42,

    /// <summary>
    /// 帐户已禁用
    /// </summary>
    AccountDisabled = 43,

    /// <summary>
    /// 此服务目前不接受内容更改
    /// </summary>
    ServiceReadOnly = 44,

    /// <summary>
    /// 帐户没有价值，因此此功能不可用
    /// </summary>
    AccountNotFeatured = 45,

    /// <summary>
    /// 允许执行此操作，但仅因为请求者是管理员
    /// </summary>
    AdministratorOK = 46,

    /// <summary>
    /// 在 Steam 协议中传输的内容版本不匹配
    /// </summary>
    ContentVersion = 47,

    /// <summary>
    /// 当前 CM 无法为发出请求的用户提供服务，用户应尝试另一个
    /// </summary>
    TryAnotherCM = 48,

    /// <summary>
    /// 您已在其他地方登录，此缓存的凭据登录失败
    /// </summary>
    PasswordRequiredToKickSession = 49,

    /// <summary>
    /// 您已经在其他地方登录，您必须等待
    /// </summary>
    AlreadyLoggedInElsewhere = 50,

    /// <summary>
    /// 长时间运行的操作（内容下载）暂停/暂停
    /// </summary>
    Suspended = 51,

    /// <summary>
    /// 操作已取消（通常由用户：内容下载）
    /// </summary>
    Cancelled = 52,

    /// <summary>
    /// 操作已取消，因为数据格式不正确或无法恢复
    /// </summary>
    DataCorruption = 53,

    /// <summary>
    /// 操作已取消 - 磁盘空间不足
    /// </summary>
    DiskFull = 54,

    /// <summary>
    /// 远程呼叫或 IPC 呼叫失败
    /// </summary>
    RemoteCallFailed = 55,

    /// <summary>
    /// 无法验证密码，因为它未设置服务器端
    /// </summary>
    PasswordUnset = 56,

    /// <summary>
    /// 外部帐户（PSN、Facebook 等）未链接到 Steam 帐户
    /// </summary>
    ExternalAccountUnlinked = 57,

    /// <summary>
    /// PSN门票无效
    /// </summary>
    PSNTicketInvalid = 58,

    /// <summary>
    /// 外部帐户（PSN、Facebook 等）已链接到其他帐户，必须先明确请求替换/删除链接
    /// </summary>
    ExternalAccountAlreadyLinked = 59,

    /// <summary>
    /// 由于本地文件和远程文件之间的冲突，无法恢复同步
    /// </summary>
    RemoteFileConflict = 60,

    /// <summary>
    /// 请求的新密码不合法
    /// </summary>
    IllegalPassword = 61,

    /// <summary>
    /// 新值与旧值相同（秘密问答）
    /// </summary>
    SameAsPreviousValue = 62,

    /// <summary>
    /// 由于第二因素身份验证失败，帐户登录被拒绝
    /// </summary>
    AccountLogonDenied = 63,

    /// <summary>
    /// 请求的新密码不合法
    /// </summary>
    CannotUseOldPassword = 64,

    /// <summary>
    /// 由于授权码无效，帐户登录被拒绝
    /// </summary>
    InvalidLoginAuthCode = 65,

    /// <summary>
    /// 由于第二因素身份验证失败，帐户登录被拒绝 - 并且没有发送任何邮件
    /// </summary>
    AccountLogonDeniedNoMail = 66,

    /// <summary>
    /// 用户硬件不支持英特尔身份保护技术（IPT）
    /// </summary>
    HardwareNotCapableOfIPT = 67,

    /// <summary>
    /// 英特尔身份保护技术（IPT）初始化失败
    /// </summary>
    IPTInitError = 68,

    /// <summary>
    /// 由于当前用户的家长控制限制，操作失败
    /// </summary>
    ParentalControlRestricted = 69,

    /// <summary>
    /// Facebook 查询返回错误
    /// </summary>
    FacebookQueryError = 70,

    /// <summary>
    /// 由于授权码过期，帐户登录被拒绝
    /// </summary>
    ExpiredLoginAuthCode = 71,

    /// <summary>
    /// 由于 IP 限制，登录失败
    /// </summary>
    IPLoginRestrictionFailed = 72,

    /// <summary>
    /// 当前用户帐户目前被锁定，无法使用。 通常是因为帐户劫持和待处理的所有权验证
    /// </summary>
    AccountLockedDown = 73,

    /// <summary>
    /// 由于帐户电子邮件未能验证，登录失败
    /// </summary>
    AccountLogonDeniedVerifiedEmailRequired = 74,

    /// <summary>
    /// 无与提供的值匹配的 URL
    /// </summary>
    NoMatchingURL = 75,

    /// <summary>
    /// 解析失败、缺少字段等
    /// </summary>
    BadResponse = 76,

    /// <summary>
    /// 用户在重新输入密码之前无法完成操作
    /// </summary>
    RequirePasswordReEntry = 77,

    /// <summary>
    /// 输入的值超出了可接受的范围
    /// </summary>
    ValueOutOfRange = 78,

    /// <summary>
    /// 发生了一些我们没想到的事情
    /// </summary>
    UnexpectedError = 79,

    /// <summary>
    /// 请求的服务已配置为不可用
    /// </summary>
    Disabled = 80,

    /// <summary>
    /// 提交给 CEG 服务器的一组文件无效！
    /// </summary>
    InvalidCEGSubmission = 81,

    /// <summary>
    /// 不允许正在使用的设备执行此操作
    /// </summary>
    RestrictedDevice = 82,

    /// <summary>
    /// 该操作无法完成，因为它受区域限制
    /// </summary>
    RegionLocked = 83,

    /// <summary>
    /// 超过临时速率限制，请稍后重试，与可能是永久性的k_EResultLimitExceeded不同
    /// </summary>
    RateLimitExceeded = 84,

    /// <summary>
    /// 需要双因素代码才能登录
    /// </summary>
    AccountLoginDeniedNeedTwoFactor = 85,

    /// <summary>
    /// 我们尝试访问的物品已被删除
    /// </summary>
    ItemDeleted = 86,

    /// <summary>
    /// 登录尝试失败，尝试限制对可能攻击者的响应
    /// </summary>
    AccountLoginDeniedThrottle = 87,

    /// <summary>
    /// 双重验证（Steam 令牌）码不正确
    /// </summary>
    TwoFactorCodeMismatch = 88,

    /// <summary>
    /// 双因素验证（Steam 令牌）激活码不匹配
    /// </summary>
    TwoFactorActivationCodeMismatch = 89,

    /// <summary>
    /// 帐户已与多个合作伙伴关联
    /// </summary>
    AccountAssociatedToMultiplePartners = 90,

    /// <summary>
    /// 数据未修改
    /// </summary>
    NotModified = 91,

    /// <summary>
    /// 该帐户没有与之关联的移动设备
    /// </summary>
    NoMobileDevice = 92,

    /// <summary>
    /// 显示的时间超出范围或容差
    /// </summary>
    TimeNotSynced = 93,

    /// <summary>
    /// 短信验证码失败（不匹配、无待处理等）
    /// </summary>
    SMSCodeFailed = 94,

    /// <summary>
    /// 访问此资源的帐户过多
    /// </summary>
    AccountLimitExceeded = 95,

    /// <summary>
    /// 此帐户的更改过多
    /// </summary>
    AccountActivityLimitExceeded = 96,

    /// <summary>
    /// 此手机号码的更改过多
    /// </summary>
    PhoneActivityLimitExceeded = 97,

    /// <summary>
    /// 无法退款至原支付手段，必须使用 Steam 钱包
    /// </summary>
    RefundToWallet = 98,

    /// <summary>
    /// 无法发送电子邮件
    /// </summary>
    EmailSendFailure = 99,

    /// <summary>
    /// 在付款结算之前无法执行操作
    /// </summary>
    NotSettled = 100,

    /// <summary>
    /// 需要提供有效的验证码
    /// </summary>
    NeedCaptcha = 101,

    /// <summary>
    /// 该令牌所有者拥有的游戏服务器登录令牌已被禁止
    /// </summary>
    GSLTDenied = 102,

    /// <summary>
    /// 游戏服务器所有者因其他原因被拒绝（帐户锁定、社区封禁、VAC 封禁、手机丢失）
    /// </summary>
    GSOwnerDenied = 103,

    /// <summary>
    /// 我们被要求采取行动的类型是无效的
    /// </summary>
    InvalidItemType = 104,

    /// <summary>
    /// 该 IP 地址已被禁止执行此操作
    /// </summary>
    IPBanned = 105,

    /// <summary>
    /// 此令牌已过期，不再使用;可重置使用
    /// </summary>
    GSLTExpired = 106,

    /// <summary>
    /// 用户没有足够的钱包资金来完成操作
    /// </summary>
    InsufficientFunds = 107,

    /// <summary>
    /// 已有过多待处理物品
    /// </summary>
    TooManyPending = 108,

    /// <summary>
    /// 未找到站点许可证
    /// </summary>
    NoSiteLicensesFound = 109,

    /// <summary>
    /// 工作组无法发送响应，超出了最大网络发送大小
    /// </summary>
    WGNetworkSendExceeded = 110,

    /// <summary>
    /// 用户不是彼此的朋友
    /// </summary>
    AccountNotFriends = 111,

    /// <summary>
    /// 用户受到限制
    /// </summary>
    LimitedUserAccount = 112,

    /// <summary>
    /// 无法删除项目
    /// </summary>
    CantRemoveItem = 113,

    /// <summary>
    /// 帐户已删除
    /// </summary>
    AccountDeleted = 114,

    /// <summary>
    /// 此许可证已存在，但已取消
    /// </summary>
    ExistingUserCancelledLicense = 115,

    /// <summary>
    /// 由于社区冷却时间（可能来自支持配置文件数据重置），访问被拒绝
    /// </summary>
    CommunityCooldown = 116,

    /// <summary>
    /// 没有指定启动器，但需要启动器来选择正确的操作领域
    /// </summary>
    NoLauncherSpecified = 117,

    /// <summary>
    /// 用户在登录前必须同意中国 SSA 或全球 SSA
    /// </summary>
    MustAgreeToSSA = 118,

    /// <summary>
    /// 不再支持指定的启动器类型;用户应被定向到其他位置
    /// </summary>
    LauncherMigrated = 119,

    /// <summary>
    /// 用户的领域与所请求资源的领域不匹配
    /// </summary>
    SteamRealmMismatch = 120,

    /// <summary>
    /// 签名检查不匹配
    /// </summary>
    InvalidSignature = 121,

    /// <summary>
    /// 无法解析输入
    /// </summary>
    ParseFailure = 122,

    /// <summary>
    /// 帐户没有经过验证的电话号码
    /// </summary>
    NoVerifiedPhone = 123,
}
