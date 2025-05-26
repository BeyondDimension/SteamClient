using BD.Common8.Http.ClientFactory.Models;
using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Constants;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;
using BD.SteamClient8.Services.Abstractions.WebApi;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamDbWebApiService"/> SteamDb WebApi 服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamDbWebApiServiceImpl"/> 类的新实例
/// </remarks>
/// <param name="serviceProvider"></param>
/// <param name="http_helper"></param>
/// <param name="loggerFactory"></param>
class SteamDbWebApiServiceImpl(
    IServiceProvider serviceProvider,
    IHttpPlatformHelperService http_helper,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(
        loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamDbWebApiService
{
    const string TAG = "SteamDbWebApiS";

    /// <summary>
    /// HttpClient 标识名称
    /// </summary>
    protected override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override JsonSerializerOptions GetJsonSerializerOptions()
    {
        var o = DefaultJsonSerializerContext_.Default.Options;
        return o;
    }

    private readonly IHttpPlatformHelperService http_helper = http_helper;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        try
        {
            var client = CreateClient();
            using var sendArgs = new WebApiClientSendArgs(requestUri)
            {
                Method = HttpMethod.Get,
                ConfigureRequestMessage = (req, args, token) =>
                {
                    req.Headers.Accept.ParseAdd(accept);
                    req.Headers.TryAddWithoutValidation("User-Agent", http_helper.UserAgent);

                    return Task.CompletedTask;
                },
            };
            sendArgs.SetHttpClient(client);
            return await SendAsync<T>(sendArgs, cancellationToken);
        }
        catch
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamUser>> GetUserInfo(long steamId64, CancellationToken cancellationToken = default)
    {
        var requestUri = string.Format(SteamApiUrls.STEAMDB_USERINFO_URL, steamId64);
        var user = await GetAsync<SteamUser>(requestUri, cancellationToken: cancellationToken) ?? new SteamUser() { SteamId64 = steamId64 };
        return user!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<List<SteamUser>>> GetUserInfos(IEnumerable<long> steamId64s, CancellationToken cancellationToken = default)
    {
        List<SteamUser> users = [];
        foreach (var i in steamId64s)
            users.Add((await GetUserInfo(i, cancellationToken: cancellationToken)).Content!);
        return users!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamApp>> GetAppInfo(int appId, CancellationToken cancellationToken = default)
    {
        var requestUri = string.Format(SteamApiUrls.STEAMDB_APPINFO_URL, appId);
        var app = await GetAsync<SteamApp>(requestUri, cancellationToken: cancellationToken) ?? new SteamApp() { AppId = (uint)appId };
        return app!;
    }
}