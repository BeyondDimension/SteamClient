using BD.Common8.Models.Abstractions;

namespace BD.SteamClient8.Models.WebApi.Logins;

/// <summary>
/// 登录接口返回跳转参数
/// </summary>
public sealed class TransferParameters : JsonModel<TransferParameters>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// SteamId
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("steamid")]
    public string? Steamid { get; set; }

    /// <summary>
    /// 加密令牌
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("token_secure")]
    public string? TokenSecure { get; set; }

    /// <summary>
    /// 认证 Token
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("auth")]
    public string? Auth { get; set; }

    /// <summary>
    /// 是否记住登录状态
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("remember_login")]
    public bool RememberLogin { get; set; }

    /// <summary>
    /// 登录 Cookie 信息
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("webcookie")]
    public string? Webcookie { get; set; }
}