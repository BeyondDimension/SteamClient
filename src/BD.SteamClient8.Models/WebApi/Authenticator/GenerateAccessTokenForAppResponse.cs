namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

public class GenerateAccessTokenForAppResponse
{
    /// <summary>
    /// <see cref="GenerateAccessTokenForAppResponse"/> Response
    /// </summary>
    [SystemTextJsonProperty("response")]
    public GenerateAccessTokenForAppResponseResponse? Response;
}

public class GenerateAccessTokenForAppResponseResponse
{
    /// <summary>
    /// JWT AccessToken
    /// </summary>
    [SystemTextJsonProperty("access_token")]
    public string? AccessToken { get; set; }
}
