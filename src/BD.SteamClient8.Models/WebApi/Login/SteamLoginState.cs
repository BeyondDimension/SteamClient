#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

[MPObj, MP2Obj(MP2SerializeLayout.Explicit)]
public sealed partial class SteamLoginState : JsonModel<SteamLoginState>
{
    /// <summary>
    /// 将 <see cref="SteamLoginState"/> 转换为 <see href="byte[]"/>
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToBytes() => Serializable.SMP2(this);

    /// <summary>
    /// 将 <see href="byte[]"/> 转换为 <see cref="SteamLoginState"/>
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SteamLoginState? Parse(byte[] buffer) => Serializable.DMP2<SteamLoginState>(buffer);

    /// <summary>
    /// 语言
    /// </summary>
    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public string? Language { get; set; }

    /// <summary>
    /// Steam 会从用户名和密码中删除所有非 ASCII 字符
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("[^\\u0000-\\u007F]")]
    public static partial Regex SteamUNPWDRegex();

    string? _Username;

    /// <summary>
    /// 用户名
    /// </summary>
    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string? Username
    {
        get => _Username;
        set => _Username = value == null ? null :
            SteamUNPWDRegex().Replace(value, string.Empty);
    }

    string? _Password;

    /// <summary>
    /// 密码
    /// </summary>
    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public string? Password
    {
        get => _Password;
        set => _Password = value == null ? null :
            SteamUNPWDRegex().Replace(value, string.Empty);
    }

    /// <summary>
    /// 验证码 Id
    /// </summary>
    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public string? CaptchaId { get; set; }

    /// <summary>
    /// 验证码链接
    /// </summary>
    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public string? CaptchaUrl { get; set; }

    /// <summary>
    /// 验证码图片 Base64
    /// </summary>
    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public string? CaptchaImageBase64 { get; set; }

    /// <summary>
    /// 验证码内容文本
    /// </summary>
    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public string? CaptchaText { get; set; }

    /// <summary>
    /// 主要邮箱
    /// </summary>
    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public string? EmailDomain { get; set; }

    //public string? EmailAuthText { get; set; }

    /// <summary>
    /// 邮箱验证码
    /// </summary>
    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public string? EmailCode { get; set; }

    /// <summary>
    /// 2FA 验证码
    /// </summary>
    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public string? TwofactorCode { get; set; }

    /// <summary>
    /// Cookie信息集合
    /// </summary>
    [MessagePackFormatter(typeof(CookieFormatter))]
    [CookieCollectionFormatter]
    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public CookieCollection? Cookies { get; set; }

    /// <summary>
    /// SteamId 字符串
    /// </summary>
    [MPKey(11), MP2Key(11), JsonPropertyOrder(11)]
    public string? SteamIdString { get; set; }

    /// <summary>
    /// SteamId
    /// </summary>
    [MPKey(12), MP2Key(12), JsonPropertyOrder(12)]
    public ulong SteamId { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [MPKey(13), MP2Key(13), JsonPropertyOrder(13)]
    public string? Email { get; set; }

    //public string? OAuthToken { get; set; }

    //public bool RequiresLogin { get; set; }

    /// <summary>
    /// 是否需要验证码
    /// </summary>
    [MPKey(14), MP2Key(14), JsonPropertyOrder(14)]
    public bool RequiresCaptcha { get; set; }

    /// <summary>
    /// 是否需要2FA验证
    /// </summary>
    [MPKey(15), MP2Key(15), JsonPropertyOrder(15)]
    public bool Requires2FA { get; set; }

    /// <summary>
    /// 需要邮箱认证
    /// </summary>
    [MPKey(16), MP2Key(16), JsonPropertyOrder(16)]
    public bool RequiresEmailAuth { get; set; }

    /// <summary>
    /// 是否登录成功
    /// </summary>
    [MPKey(17), MP2Key(17), JsonPropertyOrder(17)]
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [MPKey(18), MP2Key(18), JsonPropertyOrder(18)]
    public string? Message { get; set; }

    /// <summary>
    /// 钱包余额
    /// </summary>
    [MPKey(19), MP2Key(19), JsonPropertyOrder(19)]
    public string? WalletBalanceString { get; set; }

    /// <summary>
    /// 疑似未定区账号
    /// </summary>
    [MPKey(20), MP2Key(20), JsonPropertyOrder(20)]
    public bool IsUndeterminedArea { get; set; }

    /// <summary>
    /// 货币 ISO
    /// </summary>
    [MPKey(21), MP2Key(21), JsonPropertyOrder(21)]
    public string? CurrencySymbol { get; set; }

    /// <summary>
    /// 国家地区
    /// </summary>
    [MPKey(22), MP2Key(22), JsonPropertyOrder(22)]
    public string? SteamCountry { get; set; }

    /// <summary>
    /// 账户的历史记录数量
    /// </summary>
    [MPKey(23), MP2Key(23), JsonPropertyOrder(23)]
    public int AccountHistoryCount { get; set; }

    /// <summary>
    /// 登录会话 Id
    /// </summary>
    [MPKey(24), MP2Key(24), JsonPropertyOrder(24)]
    public string? SeesionId { get; set; }

    /// <summary>
    /// 客户端唯一标识符
    /// </summary>
    [MPKey(25), MP2Key(25), JsonPropertyOrder(25)]
    public ulong? ClientId { get; set; }

    /// <summary>
    /// 请求的唯一标识符
    /// </summary>
    [MPKey(26), MP2Key(26), JsonPropertyOrder(26)]
    public byte[]? RequestId { get; set; }

    /// <summary>
    /// Token 信息
    /// </summary>
    [MPKey(27), MP2Key(27), JsonPropertyOrder(27)]
    public string? AccessToken { get; set; }

    /// <summary>
    /// 刷新 Token 所需密钥
    /// </summary>
    [MPKey(28), MP2Key(28), JsonPropertyOrder(28)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 重新初始状态
    /// </summary>
    public void ResetStatus()
    {
        Requires2FA = false;
        RequiresCaptcha = false;
        RequiresEmailAuth = false;
        Success = false;
        ClientId = null;
        RequestId = null;
        SeesionId = null;
    }
}