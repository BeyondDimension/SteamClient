using BD.Common8.Models.Abstractions;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Models.WebApi.Authenticators.PhoneNumber;

/// <summary>
/// AddPhoneNumberAsync 接口返回模型类
/// </summary>
public class SteamAddPhoneNumberResponse : JsonModel<SteamAddPhoneNumberResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// <see cref="SteamAddPhoneNumberResponse"/> Response
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("response")]
    public SteamAddPhoneNumberResponseResponse? Response { get; set; }
}

/// <summary>
/// <see cref="SteamAddPhoneNumberResponse.Response"/> 模型类
/// </summary>
public class SteamAddPhoneNumberResponseResponse : JsonModel<SteamAddPhoneNumberResponseResponse>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 确认邮箱的地址
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("confirmation_email_address")]
    public string? ConfirmationEmailAddress { get; set; }

    /// <summary>
    /// 格式化后的手机号码
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("phone_number_formatted")]
    public string? PhoneNumberFormatted { get; set; }
}