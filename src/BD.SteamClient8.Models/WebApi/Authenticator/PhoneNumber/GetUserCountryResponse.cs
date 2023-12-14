namespace BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

#pragma warning disable SA1600 // Elements should be documented

public class GetUserCountryResponse
{
    [SystemTextJsonProperty("response")]
    public GetUserCountryResponseResponse? Response { get; set; }
}

public class GetUserCountryResponseResponse
{
    [SystemTextJsonProperty("country")]
    public string? Country { get; set; }
}