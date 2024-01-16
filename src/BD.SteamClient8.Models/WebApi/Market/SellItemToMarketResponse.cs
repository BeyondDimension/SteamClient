#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 市场出售物品返回
/// </summary>
public record class SellItemToMarketResponse
{
    /// <summary>
    /// 需要手机确认
    /// </summary>
    [SystemTextJsonProperty("needs_mobile_confirmation")]
    public bool NeedsMobileConfirmation { get; set; }

    /// <summary>
    /// 需要邮箱确认
    /// </summary>
    [SystemTextJsonProperty("needs_email_confirmation")]
    public bool NeedsEmailConfirmationConfirmed { get; set; }

    /// <summary>
    /// 邮件域名
    /// </summary>
    [SystemTextJsonProperty("email_domain")]
    public string EmailDomain { get; set; } = string.Empty;

    /// <summary>
    /// 需要确认
    /// </summary>
    [SystemTextJsonProperty("requires_confirmation")]
    public int RequiresConfirmation { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 信息
    /// </summary>
    [SystemTextJsonProperty("message")]
    public string? Message { get; set; }
}