using BD.Common8.Models.Abstractions;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators;

/// <summary>
/// 刷新 AccessToken 接口返回模型类
/// </summary>
public class GenerateAccessTokenForAppResponse : JsonModel<GenerateAccessTokenForAppResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="GenerateAccessTokenForAppResponse"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public GenerateAccessTokenForAppResponseResponse? Response;
}

/// <summary>
/// <see cref="GenerateAccessTokenForAppResponse.Response"/> 模型类
/// </summary>
public class GenerateAccessTokenForAppResponseResponse : JsonModel<GenerateAccessTokenForAppResponseResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// JWT AccessToken
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
}
