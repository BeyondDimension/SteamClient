using BD.Common8.Models.Abstractions;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators.PhoneNumber;

/// <summary>
/// AccountWaitingForEmailConfirmation 接口返回模型类
/// </summary>
public class IsAccountWaitingForEmailConfirmationResponse : JsonModel<IsAccountWaitingForEmailConfirmationResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="IsAccountWaitingForEmailConfirmationResponse"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public IsAccountWaitingForEmailConfirmationResponseResponse? Response { get; set; }
}

/// <summary>
/// <see cref="IsAccountWaitingForEmailConfirmationResponse.Response"/> 模型类
/// </summary>
public class IsAccountWaitingForEmailConfirmationResponseResponse : JsonModel<IsAccountWaitingForEmailConfirmationResponseResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 是否等待邮箱确认
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("awaiting_email_confirmation")]
    public bool AwaitingEmailConfirmation { get; set; }

    /// <summary>
    /// 等待秒数
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("seconds_to_wait")]
    public int SecondsToWait { get; set; }
}