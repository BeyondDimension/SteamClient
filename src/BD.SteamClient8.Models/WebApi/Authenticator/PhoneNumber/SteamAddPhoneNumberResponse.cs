namespace BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

#pragma warning disable SA1600 // Elements should be documented

public class SteamAddPhoneNumberResponse
{
    [SystemTextJsonProperty("response")]
    public SteamAddPhoneNumberResponseResponse? Response { get; set; }
}

public class SteamAddPhoneNumberResponseResponse
{
    [SystemTextJsonProperty("confirmation_email_address")]
    public string? ConfirmationEmailAddress { get; set; }

    [SystemTextJsonProperty("phone_number_formatted")]
    public string? PhoneNumberFormatted { get; set; }
}