using System.Text.RegularExpressions;
using static System.String2;

namespace BD.SteamClient8.Services.Abstractions.PInvoke;

/// <summary>
/// Steam 相关助手、工具类服务
/// </summary>
public partial interface ISteamService
{
    protected const string url_localhost_auth_public = Prefix_HTTP + "127.0.0.1:27060/auth/?u=public";
    const string url_steamcommunity_ = "steamcommunity.com";
    const string url_store_steampowered_ = "store.steampowered.com";
    const string url_steamcommunity = Prefix_HTTPS + url_steamcommunity_;
    const string url_store_steampowered = Prefix_HTTPS + url_store_steampowered_;
    const string url_store_steampowered_checkclientautologin = url_store_steampowered + "/login/checkclientautologin";
    const string url_steamcommunity_checkclientautologin = url_steamcommunity + "/login/checkclientautologin";

    static ISteamService Instance => Ioc.Get<ISteamService>();

    /// <summary>
    /// 从任意文本中匹配批量提取 SteamKey
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    static IEnumerable<string> ExtractKeysFromString(string source)
    {
        var m = ExtractKeysFromStringRegex().Matches(source);
        var keys = new List<string>();
        if (m.Count > 0)
        {
            foreach (Match v in m.Cast<Match>())
            {
                keys.Add(v.Value);
            }
        }
        return keys!;
    }

    [GeneratedRegex("([0-9A-Z]{5})(?:\\-[0-9A-Z]{5}){2,4}", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ExtractKeysFromStringRegex();
}
