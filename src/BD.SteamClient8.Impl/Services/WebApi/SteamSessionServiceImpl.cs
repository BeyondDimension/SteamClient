using BD.Common8.Helpers;
using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Constants;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.WebApi.Logins;
using BD.SteamClient8.Services.Abstractions.WebApi;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Extensions;
using System.Security;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamSessionService"/> Steam 登录会话信息服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamSessionServiceImpl"/> 类的新实例
/// </remarks>
public sealed class SteamSessionServiceImpl(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamSessionService
{
    const string TAG = "SteamSessionS";

    /// <inheritdoc/>
    protected sealed override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override JsonSerializerOptions GetJsonSerializerOptions()
    {
        var o = DefaultJsonSerializerContext_.Default.Options;
        return o;
    }

    readonly ConcurrentDictionary<string, SteamSession> _sessions = new();

    bool AddOrSetSessionCore(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.SteamId))
        {
            return false;
        }

        var tag = SpecialTag(steamSession.SteamId);
        steamSession.HttpClient = CreateClient(tag);
        var container = GetCookieContainer(tag);
        var c = steamSession.Cookies;
        if (c != null)
        {
            container.Add(c);
        }
        _sessions[tag] = steamSession;
        return true;
    }

    /// <inheritdoc/>
    public Task<bool> AddOrSetSession(SteamSession steamSession, CancellationToken cancellationToken = default)
    {
        var result = AddOrSetSessionCore(steamSession);
        return Task.FromResult(result);
    }

    SteamSession? RentSessionCore(string steam_id)
    {
        _sessions.TryGetValue(SpecialTag(steam_id), out var steamSession);
        return steamSession;
    }

    /// <inheritdoc/>
    public Task<SteamSession?> RentSession(string steam_id, CancellationToken cancellationToken = default)
    {
        var result = RentSessionCore(steam_id);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<SteamSession?> LoadSession(CancellationToken cancellationToken = default)
    {
        // 延时加载服务
        var secureStorage = ISecureStorage.Instance;
        var currentSteamSessionJsonStr = await secureStorage.GetAsync(ISteamSessionService.CurrentSteamUserKey);
        if (!string.IsNullOrWhiteSpace(currentSteamSessionJsonStr))
        {
            var currentSteamSession = JsonSerializer.Deserialize(currentSteamSessionJsonStr,
                SteamSessionServiceImpl_SteamSession_JsonSerializerContext_.Default.SteamSession);
            if (currentSteamSession != null)
            {
                // 延时加载服务
                var steamAccountService = Ioc.Get<ISteamAccountService>();
                var checkAccessTokenValidResult = await steamAccountService.CheckAccessTokenValidation(currentSteamSession.AccessToken, cancellationToken);
                if (checkAccessTokenValidResult)
                {
                    AddOrSetSessionCore(currentSteamSession);
                    return currentSteamSession;
                }
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task SaveSession(SteamSession steamSession, CancellationToken cancellationToken = default)
    {
        var currentSteamSessionJsonStr = JsonSerializer.Serialize(steamSession,
                SteamSessionServiceImpl_SteamSession_JsonSerializerContext_.Default.SteamSession);
        await ISecureStorage.Instance.SetAsync(ISteamSessionService.CurrentSteamUserKey, currentSteamSessionJsonStr);
    }

    /// <inheritdoc/>
    public Task RemoveSession(string steam_id, CancellationToken cancellationToken = default)
    {
        _sessions.TryRemove(SpecialTag(steam_id), out var steamSession);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> UnlockParental(SteamSession? steamSession, string pinCode, CancellationToken cancellationToken = default)
    {
        if (steamSession?.HttpClient == null)
        {
            return false;
        }

        var tag = SpecialTag(steamSession.SteamId);
        var container = GetCookieContainer(tag);

        foreach (var unlock_url in Unlock_urls())
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, unlock_url)
            {
                Content = new MultipartFormDataContent()
                {
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(pinCode)), "pin" },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(container.GetCookies(new Uri(unlock_url, UriKind.Absolute))["sessionid"]?.Value ?? string.Empty)), "sessionid" },
                }
            };
            using (await steamSession.HttpClient.UseDefaultSendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
            }
        }

        static IEnumerable<string> Unlock_urls()
        {
            yield return SteamApiUrls.STEAM_PARENTAL_UNLOCK_STORE;
            yield return SteamApiUrls.STEAM_PARENTAL_UNLOCK_COMMUNITY;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UnlockParental(string steam_id, string pinCode, CancellationToken cancellationToken = default)
    {
        var steamSession = RentSessionCore(steam_id);
        var result = await UnlockParental(steamSession, pinCode, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> LockParental(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = RentSessionCore(steam_id);
        var result = await LockParental(steamSession, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> LockParental(SteamSession? steamSession, CancellationToken cancellationToken = default)
    {
        if (steamSession?.HttpClient == null)
        {
            return false;
        }

        foreach (var lock_url in lock_urls())
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, lock_url);
            using (await steamSession.HttpClient.UseDefaultSendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
            }
        }

        static IEnumerable<string> lock_urls()
        {
            yield return SteamApiUrls.STEAM_PARENTAL_LOCK_STORE;
            yield return SteamApiUrls.STEAM_PARENTAL_LOCK_COMMUNITY;
        }

        return true;
    }

    static string SpecialTag(string steam_id) => $"SteamSession_{steam_id}";
}

[JsonSerializable(typeof(SteamSession))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
sealed partial class SteamSessionServiceImpl_SteamSession_JsonSerializerContext_ : JsonSerializerContext
{
    static SteamSessionServiceImpl_SteamSession_JsonSerializerContext_()
    {
        // https://github.com/dotnet/runtime/issues/94135
        s_defaultOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,
        };
        Default = new SteamSessionServiceImpl_SteamSession_JsonSerializerContext_(new JsonSerializerOptions(s_defaultOptions));
    }
}