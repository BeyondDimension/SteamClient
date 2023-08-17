namespace BD.SteamClient;

public record class SellItemToMarketResponse
{
    [S_JsonProperty("needs_mobile_confirmation")]
    public bool NeedsMobileConfirmation { get; set; }

    [S_JsonProperty("needs_email_confirmation")]
    public bool NeedsEmailConfirmationConfirmed { get; set; }

    [S_JsonProperty("email_domain")]
    public string EmailDomain { get; set; } = string.Empty;

    [S_JsonProperty("requires_confirmation")]
    public int RequiresConfirmation { get; set; }

    [S_JsonProperty("success")]
    public bool Success { get; set; }

    [S_JsonProperty("message")]
    public string? Message { get; set; }
}