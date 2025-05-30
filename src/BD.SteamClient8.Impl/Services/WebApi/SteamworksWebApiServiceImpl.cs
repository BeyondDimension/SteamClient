using BD.Common8.Helpers;
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
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamworksWebApiService"/> Steamworks WebApi 服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamworksWebApiServiceImpl"/> 类的新实例
/// </remarks>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
sealed class SteamworksWebApiServiceImpl(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(
        loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamworksWebApiService
{
    const string TAG = "SteamworksWebApiS";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override JsonSerializerOptions GetJsonSerializerOptions()
    {
        var o = DefaultJsonSerializerContext_.Default.Options;
        return o;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        try
        {
            var client = CreateClient();
            using var sendArgs = new WebApiClientSendArgs(requestUri)
            {
                ConfigureRequestMessage = (req, args, token) =>
                {
                    req.Headers.TryAddWithoutValidation("accept", accept);

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
    public async Task<ApiRspImpl<string?>> GetAllSteamAppsString(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<string>(SteamApiUrls.STEAMAPP_LIST_URL, cancellationToken: cancellationToken);
        return ApiRspHelper.Ok(result);
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<List<SteamApp>?>> GetAllSteamAppList(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<SteamApps>(SteamApiUrls.STEAMAPP_LIST_URL, cancellationToken: cancellationToken);
        return result?.AppList?.Apps;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamUser?>> GetUserInfo(long steamId64, CancellationToken cancellationToken = default)
    {
        // 因为某些原因放弃从社区页链接获取详细资料
        //var requestUri = string.Format(SteamApiUrls.STEAM_USERINFO_XML_URL, steamId64);

        var steamUser = new SteamUser()
        {
            SteamId64 = steamId64,
        };
        var userMiniProfile = await GetUserMiniProfileCore(steamUser.SteamId32, cancellationToken: cancellationToken);
        if (userMiniProfile != null)
        {
            steamUser.MiniProfile = userMiniProfile;
            steamUser.SteamID = WebUtility.HtmlDecode(userMiniProfile.PersonaName);
            steamUser.AvatarFull = userMiniProfile.AvatarUrl;
            steamUser.AvatarMedium = userMiniProfile.AvatarUrl;
        }
        return steamUser!;
    }

    async Task<SteamMiniProfile?> GetUserMiniProfileCore(long steamId3, CancellationToken cancellationToken = default)
    {
        var requestUri = string.Format(SteamApiUrls.STEAM_MINIPROFILE_URL, steamId3);
        var result = await GetAsync<SteamMiniProfile>(requestUri, cancellationToken: cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SteamMiniProfile?>> GetUserMiniProfile(long steamId3, CancellationToken cancellationToken = default)
    {
        var result = await GetUserMiniProfileCore(steamId3, cancellationToken: cancellationToken);
        return result;
    }
}