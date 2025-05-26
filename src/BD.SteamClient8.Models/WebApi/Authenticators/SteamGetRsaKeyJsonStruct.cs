using BD.Common8.Models.Abstractions;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// SteamGetRsaKey 接口返回类型
/// </summary>
public sealed class SteamGetRsaKeyJsonStruct : JsonModel<SteamGetRsaKeyJsonStruct>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 公钥 Mod
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("publickey_mod")]
    public string PublicKeyMod { get; set; } = string.Empty;

    /// <summary>
    /// 公钥 Exp
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("publickey_exp")]
    public string PublicKeyExp { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("timestamp")]
    public string TimeStamp { get; set; } = string.Empty;

    /// <summary>
    /// Token Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("token_gid")]
    public string TokenGId { get; set; } = string.Empty;
}
