namespace BD.SteamClient8.Models.WebApi;

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

[SystemTextJsonSerializable(typeof(RedeemWalletResponse))]
public partial class RedeemWalletResponse_ : SystemTextJsonSerializerContext
{
}