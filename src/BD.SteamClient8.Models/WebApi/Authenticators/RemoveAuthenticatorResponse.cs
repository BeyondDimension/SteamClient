using BD.Common8.Models.Abstractions;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// RemoveAuthenticatorAsync 接口返回模型类
/// </summary>
public class RemoveAuthenticatorResponse : JsonModel<RemoveAuthenticatorResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="RemoveAuthenticatorResponse"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public RemoveAuthenticatorResponseResponse? Response { get; set; }
}

/// <summary>
/// <see cref="RemoveAuthenticatorResponse.Response"/> 模型类
/// </summary>
public class RemoveAuthenticatorResponseResponse : JsonModel<RemoveAuthenticatorResponseResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否成功
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 剩余尝试次数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("revocation_attempts_remaining")]
    public int RevocationAttemptsRemaining { get; set; }
}