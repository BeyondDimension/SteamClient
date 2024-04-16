using System.Net;

namespace BD.SteamClient.Models;

public sealed partial class SteamSession
{
    [MPKey(0), JsonPropertyName("steamid")]
    public string SteamId { get; set; } = string.Empty;

    [MPKey(1), JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [MPKey(2), JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonIgnore]
    public CookieContainer CookieContainer { get; set; } = new();

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

    public bool GenerateSetCookie()
    {
        if (string.IsNullOrEmpty(this.AccessToken))
        {
            return false;
        }

        var steamLoginSecure = this.SteamId + "%7C%7C" + this.AccessToken;
        var sessionid = GetRandomHexNumber(32);
        CookieContainer.Add(new Cookie("steamLoginSecure", steamLoginSecure, "/", "steamcommunity.com"));
        CookieContainer.Add(new Cookie("sessionid", sessionid, "/", "steamcommunity.com"));
        CookieContainer.Add(new Cookie("steamLoginSecure", steamLoginSecure, "/", "steampowered.com"));
        CookieContainer.Add(new Cookie("sessionid", sessionid, "/", "steampowered.com"));

        return true;
    }

    private static string GetRandomHexNumber(int digits)
    {
        Random random = new Random();
        byte[] buffer = new byte[digits / 2];
        random.NextBytes(buffer);
        string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        if (digits % 2 == 0)
            return result;
        return result + random.Next(16).ToString("X");
    }
}
