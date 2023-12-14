namespace BD.SteamClient8.Models.WebApi.Authenticator;

#pragma warning disable SA1600 // Elements should be documented

public sealed class SteamGetRsaKeyJsonStruct
{
    [SystemTextJsonProperty("success")]
    public bool Success { get; set; }

    [SystemTextJsonProperty("publickey_mod")]
    public string PublicKeyMod { get; set; } = string.Empty;

    [SystemTextJsonProperty("publickey_exp")]
    public string PublicKeyExp { get; set; } = string.Empty;

    [SystemTextJsonProperty("timestamp")]
    public string TimeStamp { get; set; } = string.Empty;

    [SystemTextJsonProperty("token_gid")]
    public string TokenGId { get; set; } = string.Empty;
}
