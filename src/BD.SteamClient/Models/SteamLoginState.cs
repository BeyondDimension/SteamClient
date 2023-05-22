using System.Runtime.Serialization.Formatters;

namespace BD.SteamClient.Models;

public sealed class SteamLoginState : JsonModel<SteamLoginState>
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

    [MessagePackFormatter(typeof(CookieFormatter))]
    [CookieCollectionFormatter]
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

    public void ResetStatus()
    {
        Requires2FA = false;
        RequiresCaptcha = false;
        RequiresEmailAuth = false;
        Success = false;
        ClientId = null;
        RequestId = null;
        SeesionId = null;
    }
}