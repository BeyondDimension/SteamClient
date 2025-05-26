using BD.Common8.Models.Abstractions;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// 登录接口返回
/// </summary>
public class SteamMobileDologinJsonStruct : JsonModel<SteamMobileDologinJsonStruct>, IJsonSerializerContext
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
    /// 登录是否完成
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("login_complete")]
    public bool LoginComplete { get; set; }

    /// <summary>
    /// 跳转的地址列表
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("transfer_urls")]
    public string[]? TransferUrls { get; set; }

    /// <summary>
    /// 跳转的参数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("transfer_parameters")]
    public Transfer_Parameters? TransferParameters { get; set; }
}

/// <summary>
/// 跳转参数详情
/// </summary>
public class Transfer_Parameters : JsonModel<Transfer_Parameters>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// SteamId
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("steamid")]
    public string? Steamid { get; set; }

    /// <summary>
    /// 安全令牌
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("token_secure")]
    public string? TokenSecure { get; set; }

    /// <summary>
    /// JWT token
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("auth")]
    public string? Auth { get; set; }

    /// <summary>
    /// 是否记住登录
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("remember_login")]
    public bool RememberLogin { get; set; }

    /// <summary>
    /// 携带的 Cookie 信息
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("webcookie")]
    public string? WebCookie { get; set; }
}
