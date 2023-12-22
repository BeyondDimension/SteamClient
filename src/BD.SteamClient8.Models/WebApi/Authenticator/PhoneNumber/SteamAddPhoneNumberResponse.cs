namespace BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

#pragma warning disable SA1600 // Elements should be documented

/// <summary>
/// AddPhoneNumberAsync 接口返回模型类
/// </summary>
public class SteamAddPhoneNumberResponse
{
    /// <summary>
    /// <see cref="SteamAddPhoneNumberResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public SteamAddPhoneNumberResponseResponse? Response { get; set; }
}

/// <summary>
/// <see cref="SteamAddPhoneNumberResponse.Response"/> 模型类
/// </summary>
public class SteamAddPhoneNumberResponseResponse
{
    /// <summary>
    /// 确认邮箱的地址
    /// </summary>
    [SystemTextJsonProperty("confirmation_email_address")]
    public string? ConfirmationEmailAddress { get; set; }

    /// <summary>
    /// 格式化后的手机号码
    /// </summary>
    [SystemTextJsonProperty("phone_number_formatted")]
    public string? PhoneNumberFormatted { get; set; }
}