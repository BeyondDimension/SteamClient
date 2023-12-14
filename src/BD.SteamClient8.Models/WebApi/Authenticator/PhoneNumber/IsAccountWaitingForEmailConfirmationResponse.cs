namespace BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

#pragma warning disable SA1600 // Elements should be documented

public class IsAccountWaitingForEmailConfirmationResponse
{
    [SystemTextJsonProperty("response")]
    public IsAccountWaitingForEmailConfirmationResponseResponse? Response { get; set; }
}

public class IsAccountWaitingForEmailConfirmationResponseResponse
{
    [SystemTextJsonProperty("awaiting_email_confirmation")]
    public bool AwaitingEmailConfirmation { get; set; }

    [SystemTextJsonProperty("seconds_to_wait")]
    public int SecondsToWait { get; set; }
}