using BD.Common.Models.Abstractions;
using BD.SteamClient.Enums;

namespace BD.SteamClient.Models;

public sealed class SteamLoginResponse
{
    public string? Language { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? CaptchaId { get; set; }

    public string? CaptchaUrl { get; set; }

    public string? CaptchaImageBase64 { get; set; }

    public string? CaptchaText { get; set; }

    public string? EmailDomain { get; set; }

    //public string? EmailAuthText { get; set; }

    public string? EmailCode { get; set; }

    public string? TwofactorCode { get; set; }

    public CookieCollection? Cookies { get; set; }

    public string? SteamIdString { get; set; }

    public ulong SteamId { get; set; }

    public string? Email { get; set; }

    //public string? OAuthToken { get; set; }

    //public bool RequiresLogin { get; set; }

    public bool RequiresCaptcha { get; set; }

    public bool Requires2FA { get; set; }

    public bool RequiresEmailAuth { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? WalletBalanceString { get; set; }

    /// <summary>
    /// 疑似未定区账号
    /// </summary>
    public bool IsUndeterminedArea { get; set; }

    public string? CurrencySymbol { get; set; }

    public string? SteamCountry { get; set; }

    public int AccountHistoryCount { get; set; }

    public string? SeesionId { get; set; }

    public ulong? ClientId { get; set; }

    public byte[]? RequestId { get; set; }
}

public sealed class FinalizeLoginStatus : JsonModel
{
    [JsonPropertyName("steamID")]
    public string? SteamId { get; set; }

    [JsonPropertyName("redir")]
    public string? Redir { get; set; }

    [JsonPropertyName("transfer_info")]
    public List<TransferInfo>? TransferInfo { get; set; }

    [JsonPropertyName("primary_domain")]
    public string? PrimaryDomain { get; set; }
}

public sealed class TransferInfo : JsonModel
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("params")]
    public Params? Params { get; set; }
}

public sealed class Params : JsonModel
{
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }

    [JsonPropertyName("auth")]
    public string? Auth { get; set; }
}

public sealed class RedeemWalletResponse : JsonModel
{
    [JsonPropertyName("success")]
    public SteamResult Result { get; private set; }

    [JsonPropertyName("detail")]
    public PurchaseResultDetail Detail { get; set; }
}

public sealed class DoLoginRespone : JsonModel
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("requires_twofactor")]
    public bool RequiresTwofactor { get; set; }

    [JsonPropertyName("login_complete")]
    public bool LoginComplete { get; set; }

    [JsonPropertyName("transfer_urls")]
    public List<string>? TransferUrls { get; set; }

    [JsonPropertyName("transfer_parameters")]
    public TransferParameters? TransferParameters { get; set; }
}

public sealed class TransferParameters : JsonModel
{
    [JsonPropertyName("steamid")]
    public string? Steamid { get; set; }

    [JsonPropertyName("token_secure")]
    public string? TokenSecure { get; set; }

    [JsonPropertyName("auth")]
    public string? Auth { get; set; }

    [JsonPropertyName("remember_login")]
    public bool RememberLogin { get; set; }

    [JsonPropertyName("webcookie")]
    public string? Webcookie { get; set; }
}

[JsonSerializable(typeof(FinalizeLoginStatus))]
public partial class FinalizeLoginStatus_ : JsonSerializerContext
{

}

[JsonSerializable(typeof(RedeemWalletResponse))]
public partial class RedeemWalletResponse_ : JsonSerializerContext
{

}

[JsonSerializable(typeof(DoLoginRespone))]
public partial class DoLoginRespone_ : JsonSerializerContext
{

}
