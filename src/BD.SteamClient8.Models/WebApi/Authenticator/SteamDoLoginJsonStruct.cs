#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

/// <summary>
/// 旧版获取是否绑定手机接口返回模型类
/// </summary>
public sealed class SteamDoLoginHasPhoneJsonStruct
{
    /// <summary>
    /// 是否绑定手机
    /// </summary>
    [JsonPropertyName("has_phone")]
    public bool HasPhone { get; set; }

    /// <summary>
    /// 失败
    /// </summary>
    [JsonPropertyName("fatal")]
    public bool Fatal { get; set; }

    /// <summary>
    /// 成功
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    [JsonPropertyName("phoneTimeMinutesOff")]
    public int PhoneTimeMinutesOff { get; set; }
}

/// <summary>
/// AddAuthenticatorAsync 令牌添加接口返回模型类
/// </summary>
public sealed class SteamDoLoginTfaJsonStruct
{
    /// <summary>
    /// <see cref="SteamDoLoginTfaJsonStruct"/> Response
    /// </summary>
    [JsonPropertyName("response")]
    public SteamConvertSteamDataJsonStruct? Response { get; set; }
}

/// <summary>
/// FinalizeAddAuthenticatorAsync 令牌激活接口返回模型类
/// </summary>
public sealed class SteamDoLoginFinalizeJsonStruct
{
    /// <summary>
    /// <see cref="SteamDoLoginFinalizeJsonStruct"/> Response
    /// </summary>
    [JsonPropertyName("response")]
    public SteamDoLoginFinalizeResponseJsonStruct? Response { get; set; }
}

/// <summary>
/// <see cref="RemoveAuthenticatorResponse.Response"/> 令牌激活接口返回详细信息模型类
/// </summary>
public sealed class SteamDoLoginFinalizeResponseJsonStruct
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// 服务器时间
    /// </summary>
    [JsonPropertyName("server_time")]
    public string ServerTime { get; set; } = string.Empty;

    /// <summary>
    /// 是否成功
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 是否需要更多
    /// </summary>
    [JsonPropertyName("want_more")]
    public bool WantMore { get; set; }
}

/// <summary>
/// <see cref="SteamDoLoginTfaJsonStruct.Response"/> 返回详细信息
/// </summary>
public sealed class SteamConvertSteamDataJsonStruct
{
    /// <summary>
    /// 状态
    /// </summary>
    [SystemTextJsonConverter(typeof(SteamDataInt32Converter))]
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// 令牌共享密钥
    /// </summary>
    [JsonPropertyName("shared_secret")]
    public string SharedSecret { get; set; } = string.Empty;

    /// <summary>
    /// 序列号
    /// </summary>
    [JsonPropertyName("serial_number")]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 令牌恢复码
    /// </summary>
    [JsonPropertyName("revocation_code")]
    public string RevocationCode { get; set; } = string.Empty;

    /// <summary>
    /// 用户 steamId
    /// </summary>
    [SystemTextJsonConverter(typeof(SteamDataInt64Converter))]
    [JsonPropertyName("steamid")]
    public long SteamId { get; set; }

    /// <summary>
    /// 防护策略
    /// </summary>
    [SystemTextJsonConverter(typeof(SteamDataStringConverter))]
    [JsonPropertyName("steamguard_scheme")]
    public string SteamGuardScheme { get; set; } = string.Empty;

    /// <summary>
    /// 服务器时间
    /// </summary>
    [SystemTextJsonConverter(typeof(SteamDataInt64Converter))]
    [JsonPropertyName("server_time")]
    public long ServerTime { get; set; }

    /// <summary>
    /// 动态口令地址
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("account_name")]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// TokenId
    /// </summary>
    [JsonPropertyName("token_gid")]
    public string TokenGid { get; set; } = string.Empty;

    /// <summary>
    /// 身份认证密钥
    /// </summary>
    [JsonPropertyName("identity_secret")]
    public string IdentitySecret { get; set; } = string.Empty;

    /// <summary>
    /// 密钥
    /// </summary>
    [JsonPropertyName("secret_1")]
    public string Secret_1 { get; set; } = string.Empty;

    /// <summary>
    /// 手机尾号
    /// </summary>
    [JsonPropertyName("phone_number_hint")]
    public string PhoneNumberHint { get; set; } = string.Empty;
}
