namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

public class GenerateAccessTokenForAppResponse
{
    [SystemTextJsonProperty("response")]
    public GenerateAccessTokenForAppResponseResponse? Response;
}

public class GenerateAccessTokenForAppResponseResponse
{
    [SystemTextJsonProperty("access_token")]
    public string? AccessToken { get; set; }
}
