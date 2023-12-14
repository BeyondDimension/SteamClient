namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

public class SteamMobileDologinJsonStruct
{
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    [SystemTextJsonProperty("requires_twofactor")]
    public bool RequiresTwofactor { get; set; }

    [SystemTextJsonProperty("login_complete")]
    public bool LoginComplete { get; set; }

    [SystemTextJsonProperty("transfer_urls")]
    public string[]? TransferUrls { get; set; }

    [SystemTextJsonProperty("transfer_parameters")]
    public Transfer_Parameters? TransferParameters { get; set; }
}

public class Transfer_Parameters
{
    [SystemTextJsonProperty("steamid")]
    public string? Steamid { get; set; }

    [SystemTextJsonProperty("token_secure")]
    public string? TokenSecure { get; set; }

    [SystemTextJsonProperty("auth")]
    public string? Auth { get; set; }

    [SystemTextJsonProperty("remember_login")]
    public bool RememberLogin { get; set; }

    [SystemTextJsonProperty("webcookie")]
    public string? WebCookie { get; set; }
}
