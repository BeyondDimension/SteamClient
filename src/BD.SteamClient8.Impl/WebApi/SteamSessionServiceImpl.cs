#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace BD.SteamClient8.Impl;

/// <summary>
/// <see cref="ISteamSessionService"/> Steam 登录会话信息服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamSessionServiceImpl"/> 类的新实例
/// </remarks>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
public class SteamSessionServiceImpl(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamSessionService
{
    const string TAG = "SteamSessionS";

    /// <inheritdoc/>
    protected sealed override SystemTextJsonSerializerOptions JsonSerializerOptions =>
        DefaultJsonSerializerContext_.Default.Options;

    /// <inheritdoc/>
    protected sealed override string ClientName => TAG;

    readonly ConcurrentDictionary<string, SteamSession> _sessions = new();

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> AddOrSetSession(SteamSession steamSession, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (string.IsNullOrEmpty(steamSession.SteamId))
            return false;

        var tag = SpecialTag(steamSession.SteamId);
        steamSession.HttpClient = CreateClient(tag);
        var container = GetCookieContainer(tag);
        container.Add(steamSession.Cookies);
        _sessions[tag] = steamSession;
        return true;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamSession?>> RentSession(string steam_id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        _ = _sessions.TryGetValue(SpecialTag(steam_id), out var steamSession);
        return steamSession;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> RemoveSession(string steam_id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (_sessions.TryRemove(SpecialTag(steam_id), out var steamSession))
        {
            steamSession?.HttpClient?.Dispose();
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> SaveSession(SteamSession steamSession, CancellationToken cancellationToken = default)
    {
        try
        {
            var temp = SystemTextJsonSerializer.Serialize(steamSession,
                SteamSessionServiceImpl_SteamSession_JsonSerializerContext_.Default.SteamSession);
            await ISecureStorage.Instance.SetAsync("CurrentSteamUserSession", temp);
            return true;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamSession?>> LoadSession(CancellationToken cancellationToken = default)
    {
        try
        {
            var text = await ISecureStorage.Instance.GetAsync("CurrentSteamUserSession");
            if (text != null)
            {
                var session = SystemTextJsonSerializer.Deserialize(text,
                    SteamSessionServiceImpl_SteamSession_JsonSerializerContext_.Default.SteamSession);
                if (session != null)
                {
                    var steamAccountService = Ioc.Get<ISteamAccountService>();
                    var checkAccessTokenValidResult = await steamAccountService.CheckAccessTokenValidation(session.AccessToken, cancellationToken);
                    if (checkAccessTokenValidResult.IsSuccess && checkAccessTokenValidResult.Content)
                    {
                        session.GenerateSetCookie();
                        await AddOrSetSession(session, cancellationToken);
                        return session;
                    }
                }
            }
            return ApiRspHelper.Ok<SteamSession>(null);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    static string SpecialTag(string steam_id) => $"SteamSession_{steam_id}";
}

[SystemTextJsonSerializable(typeof(SteamSession))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
sealed partial class SteamSessionServiceImpl_SteamSession_JsonSerializerContext_ : SystemTextJsonSerializerContext
{
}