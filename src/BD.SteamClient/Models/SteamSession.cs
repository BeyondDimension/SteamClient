namespace BD.SteamClient.Models;

public class SteamSession
{
    public string SteamId { get; set; } = string.Empty;

    public CookieContainer CookieContainer { get; set; } = new();

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string UmqId { get; set; } = string.Empty;

    public string MessageId { get; set; } = string.Empty;

    public string APIKey { get; set; } = string.Empty;

    public string IdentitySecret { get; set; } = string.Empty;

    public long ServerTimeDiff { get; set; }

    public HttpClient? HttpClient { get; set; }
}
