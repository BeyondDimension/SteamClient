using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Models.Converters;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// 旧版获取是否绑定手机接口返回模型类
/// </summary>
public sealed class SteamDoLoginHasPhoneJsonStruct : JsonModel<SteamDoLoginHasPhoneJsonStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否绑定手机
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("has_phone")]
    public bool HasPhone { get; set; }

    /// <summary>
    /// 失败
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("fatal")]
    public bool Fatal { get; set; }

    /// <summary>
    /// 成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("phoneTimeMinutesOff")]
    public int PhoneTimeMinutesOff { get; set; }
}

/// <summary>
/// AddAuthenticatorAsync 令牌添加接口返回模型类
/// </summary>
public sealed class SteamDoLoginTfaJsonStruct : JsonModel<SteamDoLoginTfaJsonStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="SteamDoLoginTfaJsonStruct"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public SteamConvertSteamDataJsonStruct? Response { get; set; }
}

/// <summary>
/// FinalizeAddAuthenticatorAsync 令牌激活接口返回模型类
/// </summary>
public sealed class SteamDoLoginFinalizeJsonStruct : JsonModel<SteamDoLoginFinalizeJsonStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="SteamDoLoginFinalizeJsonStruct"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public SteamDoLoginFinalizeResponseJsonStruct? Response { get; set; }
}

/// <summary>
/// <see cref="RemoveAuthenticatorResponse.Response"/> 令牌激活接口返回详细信息模型类
/// </summary>
public sealed class SteamDoLoginFinalizeResponseJsonStruct : JsonModel<SteamDoLoginFinalizeResponseJsonStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 状态
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// 服务器时间
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("server_time")]
    public string ServerTime { get; set; } = string.Empty;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 是否需要更多
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("want_more")]
    public bool WantMore { get; set; }
}

/// <summary>
/// <see cref="SteamDoLoginTfaJsonStruct.Response"/> 返回详细信息
/// </summary>
public sealed class SteamConvertSteamDataJsonStruct : JsonModel<SteamConvertSteamDataJsonStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 状态
    /// </summary>
    [global::System.Text.Json.Serialization.JsonConverter(typeof(SteamDataInt32Converter))]
    [global::System.Text.Json.Serialization.JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// 令牌共享密钥
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("shared_secret")]
    public string SharedSecret { get; set; } = string.Empty;

    /// <summary>
    /// 序列号
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("serial_number")]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 令牌恢复码
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("revocation_code")]
    public string RevocationCode { get; set; } = string.Empty;

    /// <summary>
    /// 用户 steamId
    /// </summary>
    [global::System.Text.Json.Serialization.JsonConverter(typeof(SteamDataInt64Converter))]
    [global::System.Text.Json.Serialization.JsonPropertyName("steamid")]
    public long SteamId { get; set; }

    /// <summary>
    /// 防护策略
    /// </summary>
    [global::System.Text.Json.Serialization.JsonConverter(typeof(SteamDataStringConverter))]
    [global::System.Text.Json.Serialization.JsonPropertyName("steamguard_scheme")]
    public string SteamGuardScheme { get; set; } = string.Empty;

    /// <summary>
    /// 服务器时间
    /// </summary>
    [global::System.Text.Json.Serialization.JsonConverter(typeof(SteamDataInt64Converter))]
    [global::System.Text.Json.Serialization.JsonPropertyName("server_time")]
    public long ServerTime { get; set; }

    /// <summary>
    /// 动态口令地址
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("account_name")]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// TokenId
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("token_gid")]
    public string TokenGid { get; set; } = string.Empty;

    /// <summary>
    /// 身份认证密钥
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("identity_secret")]
    public string IdentitySecret { get; set; } = string.Empty;

    /// <summary>
    /// 密钥
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("secret_1")]
    public string Secret_1 { get; set; } = string.Empty;

    /// <summary>
    /// 手机尾号
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("phone_number_hint")]
    public string PhoneNumberHint { get; set; } = string.Empty;
}
