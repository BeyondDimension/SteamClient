namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

public class RemoveAuthenticatorResponse
{
    [SystemTextJsonProperty("response")]
    public RemoveAuthenticatorResponseResponse? Response { get; set; }
}

public class RemoveAuthenticatorResponseResponse
{
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    [SystemTextJsonProperty("revocation_attempts_remaining")]
    public int RevocationAttemptsRemaining { get; set; }
}