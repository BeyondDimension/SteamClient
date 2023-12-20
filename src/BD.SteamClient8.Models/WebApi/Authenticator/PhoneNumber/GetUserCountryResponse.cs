namespace BD.SteamClient8.Models.WebApi.Authenticator.PhoneNumber;

#pragma warning disable SA1600 // Elements should be documented

public class GetUserCountryResponse
{
    /// <summary>
    /// <see cref="GetUserCountryResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public GetUserCountryResponseResponse? Response { get; set; }
}

public class GetUserCountryResponseResponse
{
    /// <summary>
    /// 国家或地区
    /// </summary>
    [SystemTextJsonProperty("country")]
    public string? Country { get; set; }
}