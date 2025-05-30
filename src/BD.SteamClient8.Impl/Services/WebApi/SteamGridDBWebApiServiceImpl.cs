using BD.Common8.Helpers;
using BD.Common8.Http.ClientFactory.Models;
using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Constants;
using BD.SteamClient8.Enums.WebApi.SteamGridDB;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.WebApi.SteamGridDB;
using BD.SteamClient8.Services.Abstractions.WebApi;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamGridDBWebApiServiceImpl"/> SteamGridDB WebApi 服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamGridDBWebApiServiceImpl"/> 类的新实例
/// </remarks>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
class SteamGridDBWebApiServiceImpl(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(
        loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamGridDBWebApiServiceImpl
{
    const string TAG = "SteamGridDBWebApiS";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override JsonSerializerOptions GetJsonSerializerOptions()
    {
        var o = DefaultJsonSerializerContext_.Default.Options;
        return o;
    }

    protected virtual string? ApiKey => ISteamGridDBWebApiServiceImpl.ApiKey;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        try
        {
            var client = CreateClient();
            var apiKeySteamGridDB = ApiKey;
            apiKeySteamGridDB.ThrowIsNull();

            using var sendArgs = new WebApiClientSendArgs(requestUri)
            {
                Method = HttpMethod.Get,
                ConfigureRequestMessage = (req, args, token) =>
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKeySteamGridDB);
                    req.Headers.Accept.ParseAdd(accept);

                    return Task.CompletedTask;
                },
            };
            sendArgs.SetHttpClient(client);
            return await SendAsync<T>(sendArgs, cancellationToken);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId, CancellationToken cancellationToken = default)
    {
        var url = string.Format(SteamGridDBApiUrls.RetrieveGameBySteamAppId_Url, appId);
        var rsp = await GetAsync<SteamGridAppData>(url, MediaTypeNames.JSON, cancellationToken);
        if (rsp != null)
        {
            if (rsp.Success == true)
            {
                return rsp.Data;
            }
            else
            {
                Log.Error(nameof(GetSteamGridAppBySteamAppId), string.Join(',', rsp.Errors));
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<List<SteamGridItem>?> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid, CancellationToken cancellationToken = default)
    {
        var url = type switch
        {
            SteamGridItemType.Hero => string.Format(SteamGridDBApiUrls.RetrieveHeros_Url, gameId),
            SteamGridItemType.Logo => string.Format(SteamGridDBApiUrls.RetrieveLogos_Url, gameId),
            SteamGridItemType.Icon => string.Format(SteamGridDBApiUrls.RetrieveIcons_Url, gameId),
            SteamGridItemType.Header => string.Format(SteamGridDBApiUrls.RetrieveGrids_Url, gameId),
            _ => string.Format(SteamGridDBApiUrls.RetrieveGrids_Url, gameId),
        };

        //url += "?nsfw=any&humor=any";

        url += "?types=static,animated";

        if (type == SteamGridItemType.Header)
        {
            url += "&dimensions=460x215,920x430";
        }
        else if (type == SteamGridItemType.Grid)
        {
            url += "&dimensions=600x900";
        }

        var rsp = await GetAsync<SteamGridItemData>(url, MediaTypeNames.JSON, cancellationToken);

        if (rsp != null)
        {
            if (rsp.Success == true)
            {
                foreach (var item in rsp.Data)
                {
                    item.GridType = type;
                }
                return rsp.Data;
            }
            else
            {
                logger.LogError("GetSteamGridItemsByGameId fail, errors: {errors}",
                    new LogStrJoin(rsp.Errors));
            }
        }
        return null;
    }

    readonly struct LogStrJoin(IEnumerable<string> strings)
    {
        IEnumerable<string> Strings { get; } = strings;

        public override string ToString() => string.Join(", ", Strings);
    }
}