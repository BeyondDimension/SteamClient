namespace BD.SteamClient8.WinAuth.Models.Abstractions;

/// <summary>
/// AddAuthenticatorAsync 令牌添加接口返回模型接口
/// </summary>
public interface ISteamConvertSteamDataJsonStruct
{
    /// <summary>
    /// 状态
    /// </summary>
    int Status { get; set; }

    /// <summary>
    /// 令牌共享密钥
    /// </summary>
    string SharedSecret { get; set; }

    /// <summary>
    /// 序列号
    /// </summary>
    string SerialNumber { get; set; }

    /// <summary>
    /// 令牌恢复码
    /// </summary>
    string RevocationCode { get; set; }

    /// <summary>
    /// Steam 用户 Id
    /// </summary>
    long SteamId { get; set; }

    /// <summary>
    /// 防护策略
    /// </summary>
    string SteamGuardScheme { get; set; }

    /// <summary>
    /// 服务器时间
    /// </summary>
    long ServerTime { get; set; }

    /// <summary>
    /// 动态口令地址
    /// </summary>
    string Uri { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    string AccountName { get; set; }

    /// <summary>
    /// Token Id
    /// </summary>
    string TokenGid { get; set; }

    /// <summary>
    /// 身份认证密钥
    /// </summary>
    string IdentitySecret { get; set; }

    /// <summary>
    /// 密钥
    /// </summary>
    string Secret_1 { get; set; }

    /// <summary>
    /// 手机尾号
    /// </summary>
    string PhoneNumberHint { get; set; }
}
