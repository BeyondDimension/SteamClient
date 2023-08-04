namespace BD.SteamClient.Models;

public class SteamSession
{
    public string SteamId { get; set; } = string.Empty;

    public CookieContainer CookieContainer { get; set; } = new();

    public string OAuthToken { get; set; } = string.Empty;

    public string UmqId { get; set; } = string.Empty;

    public string MessageId { get; set; } = string.Empty;

    public HttpClient? HttpClient { get; set; }
}
