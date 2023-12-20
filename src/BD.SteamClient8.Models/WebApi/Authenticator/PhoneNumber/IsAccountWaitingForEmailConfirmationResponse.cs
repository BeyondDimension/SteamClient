namespace BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

#pragma warning disable SA1600 // Elements should be documented

public class IsAccountWaitingForEmailConfirmationResponse
{
    /// <summary>
    /// <see cref="IsAccountWaitingForEmailConfirmationResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public IsAccountWaitingForEmailConfirmationResponseResponse? Response { get; set; }
}

public class IsAccountWaitingForEmailConfirmationResponseResponse
{
    /// <summary>
    /// 是否等待邮箱确认
    /// </summary>
    [SystemTextJsonProperty("awaiting_email_confirmation")]
    public bool AwaitingEmailConfirmation { get; set; }

    /// <summary>
    /// 等待秒数
    /// </summary>
    [SystemTextJsonProperty("seconds_to_wait")]
    public int SecondsToWait { get; set; }
}