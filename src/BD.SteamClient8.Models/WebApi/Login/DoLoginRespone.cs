#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// 登录返回
/// </summary>
public sealed class DoLoginResponse : JsonModel
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
    /// 是否登录完成
    /// </summary>
    [SystemTextJsonProperty("login_complete")]
    public bool LoginComplete { get; set; }

    /// <summary>
    /// 跳转 Urls
    /// </summary>
    [SystemTextJsonProperty("transfer_urls")]
    public List<string>? TransferUrls { get; set; }

    /// <summary>
    /// 跳转参数
    /// </summary>
    [SystemTextJsonProperty("transfer_parameters")]
    public TransferParameters? TransferParameters { get; set; }
}
