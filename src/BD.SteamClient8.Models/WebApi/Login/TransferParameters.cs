namespace BD.SteamClient8.Models;

/// <summary>
/// 登录接口返回跳转参数
/// </summary>
public sealed class TransferParameters : JsonModel
{
    /// <summary>
    /// SteamId
    /// </summary>
    [SystemTextJsonProperty("steamid")]
    public string? Steamid { get; set; }

    /// <summary>
    /// 加密令牌
    /// </summary>
    [SystemTextJsonProperty("token_secure")]
    public string? TokenSecure { get; set; }

    /// <summary>
    /// 认证 Token
    /// </summary>
    [SystemTextJsonProperty("auth")]
    public string? Auth { get; set; }

    /// <summary>
    /// 是否记住登录状态
    /// </summary>
    [SystemTextJsonProperty("remember_login")]
    public bool RememberLogin { get; set; }

    /// <summary>
    /// 登录 Cookie 信息
    /// </summary>
    [SystemTextJsonProperty("webcookie")]
    public string? Webcookie { get; set; }
}