namespace BD.SteamClient8.Models.WebApi.Login;

public sealed partial class SteamSession
{
    /// <summary>
    /// SteamId
    /// </summary>
    [MPKey(0), SystemTextJsonProperty("steamid")]
    public string SteamId { get; set; } = string.Empty;

    /// <summary>
    /// 登录 Token
    /// </summary>
    [MPKey(1), SystemTextJsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新 Token 所需密钥
    /// </summary>
    [MPKey(2), SystemTextJsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Cookie容器
    /// </summary>
    [SystemTextJsonIgnore]
    public CookieCollection Cookies { get; set; } = [];

    /// <summary>
    /// 聊天消息队列唯一标识符
    /// </summary>
    [SystemTextJsonIgnore]
    public string UmqId { get; set; } = string.Empty;

    /// <summary>
    /// 聊天消息唯一标识符
    /// </summary>
    [SystemTextJsonIgnore]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// 用户 APIKey
    /// </summary>
    [SystemTextJsonIgnore]
    public string APIKey { get; set; } = string.Empty;

    /// <summary>
    /// 令牌认证密钥
    /// </summary>
    [SystemTextJsonIgnore]
    public string IdentitySecret { get; set; } = string.Empty;

    /// <summary>
    /// 与 Steam 通用时间的间隔
    /// </summary>
    [SystemTextJsonIgnore]
    public long ServerTimeDiff { get; set; }

    /// <summary>
    /// 存储用户登录状态的 httpClient
    /// </summary>
    [SystemTextJsonIgnore]
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// 设置 Cookie 信息
    /// </summary>
    /// <returns></returns>
    public bool GenerateSetCookie()
    {
        if (string.IsNullOrEmpty(this.AccessToken))
        {
            return false;
        }

        var steamLoginSecure = this.SteamId + "%7C%7C" + this.AccessToken;
        var sessionid = GetRandomHexNumber(32);
        Cookies.Add(new Cookie("steamLoginSecure", steamLoginSecure, "/", "steamcommunity.com"));
        Cookies.Add(new Cookie("sessionid", sessionid, "/", "steamcommunity.com"));
        Cookies.Add(new Cookie("steamLoginSecure", steamLoginSecure, "/", "steampowered.com"));
        Cookies.Add(new Cookie("sessionid", sessionid, "/", "steampowered.com"));

        return true;
    }

    private static string GetRandomHexNumber(int digits)
    {
        Random random = new Random();
        byte[] buffer = new byte[digits / 2];
        random.NextBytes(buffer);
        string result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        if (digits % 2 == 0)
            return result;
        return result + random.Next(16).ToString("X");
    }
}
