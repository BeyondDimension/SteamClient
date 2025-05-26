using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Logins;

/// <summary>
/// 登录返回
/// </summary>
public sealed class DoLoginResponse : JsonModel<DoLoginResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 是否需要 2FA 验证码
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("requires_twofactor")]
    public bool RequiresTwofactor { get; set; }

    /// <summary>
    /// 是否登录完成
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("login_complete")]
    public bool LoginComplete { get; set; }

    /// <summary>
    /// 跳转 Urls
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("transfer_urls")]
    public List<string>? TransferUrls { get; set; }

    /// <summary>
    /// 跳转参数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("transfer_parameters")]
    public TransferParameters? TransferParameters { get; set; }
}
