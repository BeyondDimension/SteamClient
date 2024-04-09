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

    /// <summary>
    /// <see cref="SteamApiUrls.STEAM_COMMUNITY_URL"/> Steam 社区 家庭监护凭证与 sessionid 绑定，不同域名凭证独立不共享
    /// </summary>
    [MPKey(3), JsonPropertyName("steam_parental")]
    public string SteamParental { get; set; } = string.Empty;

    /// <summary>
    /// 本地储存的 sessionid，不是最新的 
    /// </summary>
    [MPKey(4), JsonPropertyName("sessionid")]
    public string SaveSessionId { get; set; } = string.Empty;

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

    public void SavingBefore()
    {
        var cookies = this.CookieContainer.GetCookies(new Uri(SteamApiUrls.STEAM_COMMUNITY_URL));
        this.SteamParental = cookies["steamparental"]?.Value ?? string.Empty;
        this.SaveSessionId = cookies["sessionid"]?.Value ?? string.Empty;
    }

    public bool GenerateSetCookie()
    {
        if (string.IsNullOrEmpty(this.AccessToken))
        {
            return false;
        }

        var steamLoginSecure = this.SteamId + "%7C%7C" + this.AccessToken;
        var sessionid = string.IsNullOrEmpty(this.SaveSessionId) ? GetRandomHexNumber(32) : this.SaveSessionId;

        if (!string.IsNullOrEmpty(SteamParental))
        {
            CookieContainer.Add(new Cookie("steamparental", this.SteamParental, "/", "steamcommunity.com"));
            CookieContainer.Add(new Cookie("steamparental", this.SteamParental, "/", "steampowered.com"));
        }
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
