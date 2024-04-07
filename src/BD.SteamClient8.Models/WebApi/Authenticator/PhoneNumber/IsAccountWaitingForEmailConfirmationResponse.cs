namespace BD.SteamClient8.Models;

/// <summary>
/// AccountWaitingForEmailConfirmation 接口返回模型类
/// </summary>
public class IsAccountWaitingForEmailConfirmationResponse
{
    /// <summary>
    /// <see cref="IsAccountWaitingForEmailConfirmationResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public IsAccountWaitingForEmailConfirmationResponseResponse? Response { get; set; }
}

/// <summary>
/// <see cref="IsAccountWaitingForEmailConfirmationResponse.Response"/> 模型类
/// </summary>
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