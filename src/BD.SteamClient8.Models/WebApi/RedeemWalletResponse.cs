#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// 兑换钱包返回信息
/// </summary>
public sealed class RedeemWalletResponse : JsonModel
{
    /// <summary>
    /// 成功结果
    /// </summary>
    [SystemTextJsonProperty("success")]
    public SteamResult Result { get; private set; }

    /// <summary>
    /// 详情
    /// </summary>
    [SystemTextJsonProperty("detail")]
    public PurchaseResultDetail Detail { get; set; }
}