#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// 完成登录状态
/// </summary>
public sealed class FinalizeLoginStatus : JsonModel
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