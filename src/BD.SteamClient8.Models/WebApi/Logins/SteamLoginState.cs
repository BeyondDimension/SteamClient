using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BD.SteamClient8.Models.WebApi.Logins;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[global::MessagePack.MessagePackObject, global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class SteamLoginState : JsonModel<SteamLoginState>, IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext IJsonSerializerContext.Default => DefaultJsonSerializerContext_.Default;

    /// <summary>
    /// 将 <see cref="SteamLoginState"/> 转换为 <see href="byte[]"/>
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToBytes() => Serializable.SMP2(this);

    /// <summary>
    /// 将 <see href="byte[]"/> 转换为 <see cref="SteamLoginState"/>
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // MemoryPack 不会访问此模型类动态成员
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
    public static SteamLoginState? Parse(byte[] buffer) => Serializable.DMP2<SteamLoginState>(buffer);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

    /// <summary>
    /// 语言
    /// </summary>
    [global::MessagePack.Key(0), global::MemoryPack.MemoryPackOrder(0), global::System.Text.Json.Serialization.JsonPropertyOrder(0)]
    public string? Language { get; set; }

    string? _Username;

    /// <summary>
    /// 用户名
    /// </summary>
    [global::MessagePack.Key(1), global::MemoryPack.MemoryPackOrder(1), global::System.Text.Json.Serialization.JsonPropertyOrder(1)]
    public string? Username
    {
        get => _Username;
        set => _Username = value == null ? null :
            String2.SteamUNPWDRegex().Replace(value, string.Empty);
    }

    string? _Password;

    /// <summary>
    /// 密码
    /// </summary>
    [global::MessagePack.Key(2), global::MemoryPack.MemoryPackOrder(2), global::System.Text.Json.Serialization.JsonPropertyOrder(2)]
    public string? Password
    {
        get => _Password;
        set => _Password = value == null ? null :
            String2.SteamUNPWDRegex().Replace(value, string.Empty);
    }

    /// <summary>
    /// 验证码 Id
    /// </summary>
    [global::MessagePack.Key(3), global::MemoryPack.MemoryPackOrder(3), global::System.Text.Json.Serialization.JsonPropertyOrder(3)]
    public string? CaptchaId { get; set; }

    /// <summary>
    /// 验证码链接
    /// </summary>
    [global::MessagePack.Key(4), global::MemoryPack.MemoryPackOrder(4), global::System.Text.Json.Serialization.JsonPropertyOrder(4)]
    public string? CaptchaUrl { get; set; }

    /// <summary>
    /// 验证码图片 Base64
    /// </summary>
    [global::MessagePack.Key(5), global::MemoryPack.MemoryPackOrder(5), global::System.Text.Json.Serialization.JsonPropertyOrder(5)]
    public string? CaptchaImageBase64 { get; set; }

    /// <summary>
    /// 验证码内容文本
    /// </summary>
    [global::MessagePack.Key(6), global::MemoryPack.MemoryPackOrder(6), global::System.Text.Json.Serialization.JsonPropertyOrder(6)]
    public string? CaptchaText { get; set; }

    /// <summary>
    /// 主要邮箱
    /// </summary>
    [global::MessagePack.Key(7), global::MemoryPack.MemoryPackOrder(7), global::System.Text.Json.Serialization.JsonPropertyOrder(7)]
    public string? EmailDomain { get; set; }

    //public string? EmailAuthText { get; set; }

    /// <summary>
    /// 邮箱验证码
    /// </summary>
    [global::MessagePack.Key(8), global::MemoryPack.MemoryPackOrder(8), global::System.Text.Json.Serialization.JsonPropertyOrder(8)]
    public string? EmailCode { get; set; }

    /// <summary>
    /// 2FA 验证码
    /// </summary>
    [global::MessagePack.Key(9), global::MemoryPack.MemoryPackOrder(9), global::System.Text.Json.Serialization.JsonPropertyOrder(9)]
    public string? TwofactorCode { get; set; }

    static string GetRandomHexNumber(int digits)
    {
        var buffer = new byte[digits / 2];
        Random.Shared.NextBytes(buffer);
        string result = string.Concat(buffer.Select(x => x.ToString("X2")));
        if (digits % 2 == 0)
            return result;
        return result + Random.Shared.Next(16).ToString("X");
    }

    static IEnumerable<Cookie> GenerateCookies(string accessToken, string steamId)
    {
        var steamLoginSecure = steamId + "%7C%7C" + accessToken;
        var sessionid = GetRandomHexNumber(32);

        yield return new Cookie("sessionid", sessionid, "/", SteamApiUrls.STEAM_STORE_HOST);
        yield return new Cookie("steamLoginSecure", steamLoginSecure, "/", SteamApiUrls.STEAM_STORE_HOST);

        yield return new Cookie("sessionid", sessionid, "/", SteamApiUrls.STEAM_COMMUNITY_HOST);
        yield return new Cookie("steamLoginSecure", steamLoginSecure, "/", SteamApiUrls.STEAM_COMMUNITY_HOST);
    }

    internal static CookieCollection? GetCookieCollection(
        CookieCollection? cookieCollection,
        string? accessToken,
        string steamId)
    {
        if (cookieCollection == null)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var result = new CookieCollection();
                foreach (var cookie in GenerateCookies(accessToken, steamId))
                {
                    result.Add(cookie);
                }
                return result;
            }
            else
            {
                return null;
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                foreach (var cookie in GenerateCookies(accessToken, steamId))
                {
                    var oldCookie = cookieCollection.FirstOrDefault(
                        x => x.Domain == cookie.Domain && x.Name == cookie.Name);
                    if (oldCookie == null || oldCookie.Expired)
                    {
                        cookieCollection.Add(cookie);
                    }
                }
            }
            return cookieCollection;
        }
    }

    /// <summary>
    /// Cookie 信息集合
    /// </summary>
    [global::MessagePack.MessagePackFormatter(typeof(global::System.Runtime.Serialization.Formatters.CookieFormatter))]
    [global::System.Runtime.Serialization.Formatters.CookieCollectionFormatter]
    [global::MessagePack.Key(10), global::MemoryPack.MemoryPackOrder(10), global::System.Text.Json.Serialization.JsonPropertyOrder(10)]
    public CookieCollection? Cookies
    {
        get => GetCookieCollection(field, AccessToken, SteamId.ToString());
        set => field = value;
    }

    /// <summary>
    /// SteamId 字符串
    /// </summary>
    [global::MessagePack.Key(11), global::MemoryPack.MemoryPackOrder(11), global::System.Text.Json.Serialization.JsonPropertyOrder(11)]
    public string? SteamIdString { get; set; }

    /// <summary>
    /// SteamId
    /// </summary>
    [global::MessagePack.Key(12), global::MemoryPack.MemoryPackOrder(12), global::System.Text.Json.Serialization.JsonPropertyOrder(12)]
    public ulong SteamId { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [global::MessagePack.Key(13), global::MemoryPack.MemoryPackOrder(13), global::System.Text.Json.Serialization.JsonPropertyOrder(13)]
    public string? Email { get; set; }

    //public string? OAuthToken { get; set; }

    //public bool RequiresLogin { get; set; }

    /// <summary>
    /// 是否需要验证码
    /// </summary>
    [global::MessagePack.Key(14), global::MemoryPack.MemoryPackOrder(14), global::System.Text.Json.Serialization.JsonPropertyOrder(14)]
    public bool RequiresCaptcha { get; set; }

    /// <summary>
    /// 是否需要2FA验证
    /// </summary>
    [global::MessagePack.Key(15), global::MemoryPack.MemoryPackOrder(15), global::System.Text.Json.Serialization.JsonPropertyOrder(15)]
    public bool Requires2FA { get; set; }

    /// <summary>
    /// 需要邮箱认证
    /// </summary>
    [global::MessagePack.Key(16), global::MemoryPack.MemoryPackOrder(16), global::System.Text.Json.Serialization.JsonPropertyOrder(16)]
    public bool RequiresEmailAuth { get; set; }

    /// <summary>
    /// 是否登录成功
    /// </summary>
    [global::MessagePack.Key(17), global::MemoryPack.MemoryPackOrder(17), global::System.Text.Json.Serialization.JsonPropertyOrder(17)]
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [global::MessagePack.Key(18), global::MemoryPack.MemoryPackOrder(18), global::System.Text.Json.Serialization.JsonPropertyOrder(18)]
    public string? Message { get; set; }

    /// <summary>
    /// 钱包余额
    /// </summary>
    [global::MessagePack.Key(19), global::MemoryPack.MemoryPackOrder(19), global::System.Text.Json.Serialization.JsonPropertyOrder(19)]
    public string? WalletBalanceString { get; set; }

    /// <summary>
    /// 疑似未定区账号
    /// </summary>
    [global::MessagePack.Key(20), global::MemoryPack.MemoryPackOrder(20), global::System.Text.Json.Serialization.JsonPropertyOrder(20)]
    public bool IsUndeterminedArea { get; set; }

    /// <summary>
    /// 货币 ISO
    /// </summary>
    [global::MessagePack.Key(21), global::MemoryPack.MemoryPackOrder(21), global::System.Text.Json.Serialization.JsonPropertyOrder(21)]
    public string? CurrencySymbol { get; set; }

    /// <summary>
    /// 国家地区
    /// </summary>
    [global::MessagePack.Key(22), global::MemoryPack.MemoryPackOrder(22), global::System.Text.Json.Serialization.JsonPropertyOrder(22)]
    public string? SteamCountry { get; set; }

    /// <summary>
    /// 账户的历史记录数量
    /// </summary>
    [global::MessagePack.Key(23), global::MemoryPack.MemoryPackOrder(23), global::System.Text.Json.Serialization.JsonPropertyOrder(23)]
    public int AccountHistoryCount { get; set; }

    /// <summary>
    /// 登录会话 Id
    /// </summary>
    [global::MessagePack.Key(24), global::MemoryPack.MemoryPackOrder(24), global::System.Text.Json.Serialization.JsonPropertyOrder(24)]
    public string? SeesionId { get; set; }

    /// <summary>
    /// 客户端唯一标识符
    /// </summary>
    [global::MessagePack.Key(25), global::MemoryPack.MemoryPackOrder(25), global::System.Text.Json.Serialization.JsonPropertyOrder(25)]
    public ulong? ClientId { get; set; }

    /// <summary>
    /// 请求的唯一标识符
    /// </summary>
    [global::MessagePack.Key(26), global::MemoryPack.MemoryPackOrder(26), global::System.Text.Json.Serialization.JsonPropertyOrder(26)]
    public byte[]? RequestId { get; set; }

    /// <summary>
    /// Token 信息
    /// </summary>
    [global::MessagePack.Key(27), global::MemoryPack.MemoryPackOrder(27), global::System.Text.Json.Serialization.JsonPropertyOrder(27)]
    public string? AccessToken { get; set; }

    /// <summary>
    /// 刷新 Token 所需密钥
    /// </summary>
    [global::MessagePack.Key(28), global::MemoryPack.MemoryPackOrder(28), global::System.Text.Json.Serialization.JsonPropertyOrder(28)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 重新初始状态
    /// </summary>
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