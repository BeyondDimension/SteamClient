#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Models;

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
        if (string.IsNullOrEmpty(AccessToken))
        {
            return false;
        }

        var steamLoginSecure = SteamId + "%7C%7C" + AccessToken;
        var sessionid = GetRandomHexNumber(32);
        Cookies.Add(new Cookie("steamLoginSecure", steamLoginSecure, "/", new Uri(SteamApiUrls.STEAM_COMMUNITY_URL).Host));
        Cookies.Add(new Cookie("sessionid", sessionid, "/", new Uri(SteamApiUrls.STEAM_COMMUNITY_URL).Host));
        Cookies.Add(new Cookie("steamLoginSecure", steamLoginSecure, "/", new Uri(SteamApiUrls.STEAM_STORE_URL).Host));
        Cookies.Add(new Cookie("sessionid", sessionid, "/", new Uri(SteamApiUrls.STEAM_STORE_URL).Host));

        // Cookie 去重保留最新
        var deduplicated = new CookieCollection();
        Cookies.Cast<Cookie>()
            .GroupBy(cookie => new { cookie.Domain, cookie.Name })
            .SelectMany(group =>
            {
                return group.OrderByDescending(cookie => cookie.TimeStamp).Take(1);
            }).ForEach(deduplicated.Add);
        Cookies = deduplicated;
        return true;
    }

    static string GetRandomHexNumber(int digits)
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
