using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Logins;

/// <summary>
/// 完成登录接口跳转参数
/// </summary>
public sealed class TransferInfoParams : JsonModel<TransferInfoParams>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 随机数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("nonce")]
    public string? Nonce { get; set; }

    /// <summary>
    /// 认证密钥
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("auth")]
    public string? Auth { get; set; }
}