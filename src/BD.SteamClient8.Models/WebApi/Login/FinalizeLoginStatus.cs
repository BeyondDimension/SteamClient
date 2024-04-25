namespace BD.SteamClient8.Models;

/// <summary>
/// 完成登录状态
/// </summary>
public sealed class FinalizeLoginStatus : JsonModel<FinalizeLoginStatus>
{
    /// <summary>
    /// SteamId
    /// </summary>
    [SystemTextJsonProperty("steamID")]
    public string? SteamId { get; set; }

    /// <summary>
    /// 重定向地址
    /// </summary>
    [SystemTextJsonProperty("redir")]
    public string? Redir { get; set; }

    /// <summary>
    /// 会话信息
    /// </summary>
    [SystemTextJsonProperty("transfer_info")]
    public List<TransferInfo>? TransferInfo { get; set; }

    /// <summary>
    /// 主要域名
    /// </summary>
    [SystemTextJsonProperty("primary_domain")]
    public string? PrimaryDomain { get; set; }
}