namespace BD.SteamClient8.Models.WebApi.Logins;

/// <summary>
/// 完成登录接口跳转参数
/// </summary>
public sealed class TransferInfoParams : JsonModel<TransferInfoParams>
{
    /// <summary>
    /// 随机数
    /// </summary>
    [SystemTextJsonProperty("nonce")]
    public string? Nonce { get; set; }

    /// <summary>
    /// 认证密钥
    /// </summary>
    [SystemTextJsonProperty("auth")]
    public string? Auth { get; set; }
}