using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Enums.WebApi;

namespace BD.SteamClient8.Models.WebApi;

/// <summary>
/// 兑换钱包返回信息
/// </summary>
public sealed class RedeemWalletResponse : JsonModel<RedeemWalletResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 成功结果
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public SteamResult Result { get; private set; }

    /// <summary>
    /// 详情
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("detail")]
    public PurchaseResultDetail Detail { get; set; }
}