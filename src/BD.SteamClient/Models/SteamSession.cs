namespace BD.SteamClient.Models;

public sealed partial class SteamSession
{
    [MPKey(0), JsonPropertyName("steamid")]
    public string SteamId { get; set; } = string.Empty;

    [MPKey(1), JsonPropertyName("CookieContainer")]
    public CookieContainer CookieContainer { get; set; } = new();

    [MPKey(2), JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [MPKey(3), JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonIgnore]
    public string UmqId { get; set; } = string.Empty;

    [JsonIgnore]
    public string MessageId { get; set; } = string.Empty;

    [JsonIgnore]
    public string APIKey { get; set; } = string.Empty;

    [JsonIgnore]
    public string IdentitySecret { get; set; } = string.Empty;

    [JsonIgnore]
    public long ServerTimeDiff { get; set; }

    [JsonIgnore]
    public HttpClient? HttpClient { get; set; }
}
