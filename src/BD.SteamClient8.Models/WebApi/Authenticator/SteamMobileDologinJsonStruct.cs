namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

/// <summary>
/// 登录接口返回
/// </summary>
public class SteamMobileDologinJsonStruct
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 是否需要 2FA 验证码
    /// </summary>
    [SystemTextJsonProperty("requires_twofactor")]
    public bool RequiresTwofactor { get; set; }

    /// <summary>
    /// 登录是否完成
    /// </summary>
    [SystemTextJsonProperty("login_complete")]
    public bool LoginComplete { get; set; }

    /// <summary>
    /// 跳转的地址列表
    /// </summary>
    [SystemTextJsonProperty("transfer_urls")]
    public string[]? TransferUrls { get; set; }

    /// <summary>
    /// 跳转的参数
    /// </summary>
    [SystemTextJsonProperty("transfer_parameters")]
    public Transfer_Parameters? TransferParameters { get; set; }
}

/// <summary>
/// 跳转参数详情
/// </summary>
public class Transfer_Parameters
{
    /// <summary>
    /// SteamId
    /// </summary>
    [SystemTextJsonProperty("steamid")]
    public string? Steamid { get; set; }

    /// <summary>
    /// 安全令牌
    /// </summary>
    [SystemTextJsonProperty("token_secure")]
    public string? TokenSecure { get; set; }

    /// <summary>
    /// JWT token
    /// </summary>
    [SystemTextJsonProperty("auth")]
    public string? Auth { get; set; }

    /// <summary>
    /// 是否记住登录
    /// </summary>
    [SystemTextJsonProperty("remember_login")]
    public bool RememberLogin { get; set; }

    /// <summary>
    /// 携带的 Cookie 信息
    /// </summary>
    [SystemTextJsonProperty("webcookie")]
    public string? WebCookie { get; set; }
}
