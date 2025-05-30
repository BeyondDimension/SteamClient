using AngleSharp.Html.Parser;
using BD.Common8.Helpers;
using BD.Common8.Http.ClientFactory.Models;
using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Constants;
using BD.SteamClient8.Enums.WebApi.Trades;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.WebApi.Logins;
using BD.SteamClient8.Models.WebApi.Trades;
using BD.SteamClient8.Services.Abstractions.WebApi;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Extensions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamTradeService"/> Steam 交易报价相关服实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamTradeServiceImpl"/> 类的新实例
/// </remarks>
/// <param name="s"></param>
/// <param name="sessions"></param>
/// <param name="loggerFactory"></param>
public sealed partial class SteamTradeServiceImpl(
    IServiceProvider s,
    ISteamSessionService sessions,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(
        loggerFactory.CreateLogger(TAG),
        s), ISteamTradeService
{
    const string TAG = "SteamTradeWebApiS";

    /// <inheritdoc/>
    protected override string ClientName => TAG;

    /// <inheritdoc/>
    protected sealed override JsonSerializerOptions GetJsonSerializerOptions()
    {
        var o = DefaultJsonSerializerContext_.Default.Options;
        return o;
    }

    readonly ConcurrentDictionary<string, CancellationTokenSource> _tasks = new();
}

partial class SteamTradeServiceImpl // 后台任务
{
    readonly Lock lockStartTradeTaskSync = new();

    async void RunTask(Action action, TimeSpan interval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // 执行后台任务
                action();

                // 等待指定的时间间隔
                await Task.Delay(interval, cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred in background task.");
            }
        }
    }

    /// <inheritdoc/>
    public ApiRspImpl StartTradeTaskSync(string steam_id, TimeSpan interval, TradeTaskEnum tradeTaskEnum, CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            lock (lockStartTradeTaskSync)
            {
                try
                {
                    var taskName = $"{steam_id}_{tradeTaskEnum}";
                    if (!_tasks.ContainsKey(taskName))
                    {
                        Action action;
                        switch (tradeTaskEnum)
                        {
                            case TradeTaskEnum.None:
                                return ApiRspHelper.Fail();
                            case TradeTaskEnum.AutoAcceptGitTrade:
                                action = async () =>
                                {
                                    await AcceptAllGiftTradeOfferAsync(steam_id);
                                };
                                break;
                            default:
                                return ApiRspHelper.Fail();
                        }
                        var cancellationTokenSource = new CancellationTokenSource();
                        _tasks.TryAdd(taskName, cancellationTokenSource);
                        Task2.InBackground(() =>
                        {
                            RunTask(action, interval, cancellationTokenSource.Token);
                        }, longRunning: true);
                        return ApiRspHelper.Ok();
                    }
                }
                catch (Exception ex)
                {
                    return ApiRspHelper.Exception(ex);
                }
            }
        }
        return ApiRspHelper.Fail();
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> StartTradeTask(string steam_id, TimeSpan interval, TradeTaskEnum tradeTaskEnum, CancellationToken cancellationToken = default)
    {
        var result = StartTradeTaskSync(steam_id, interval, tradeTaskEnum, cancellationToken);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public ApiRspImpl StopTaskSync(string steam_id, TradeTaskEnum tradeTaskEnum, CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var taskName = $"{steam_id}_{tradeTaskEnum}";
                if (_tasks.TryGetValue(taskName, out var cancellationTokenSource))
                {
                    cancellationTokenSource.Cancel();
                    _tasks.TryRemove(taskName, out _);
                }
                return ApiRspHelper.Ok();
            }
            catch (Exception ex)
            {
                return ApiRspHelper.Exception(ex);
            }
        }
        return ApiRspHelper.Fail();
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> StopTask(string steam_id, TradeTaskEnum tradeTaskEnum, CancellationToken cancellationToken = default)
    {
        var result = StopTaskSync(steam_id, tradeTaskEnum, cancellationToken);
        return Task.FromResult(result);
    }
}

partial class SteamTradeServiceImpl // 交易报价
{
    /// <inheritdoc/>
    public async Task<bool> AcceptAllGiftTradeOfferAsync(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        if (string.IsNullOrEmpty(steamSession.APIKey))
        {
            return false;
        }

        var trade_summary = await GetTradeOffersSummaryAsync(steamSession.APIKey, cancellationToken);
        if (trade_summary != null && trade_summary.PendingReceivedCount > 0)
        {
            var trade_offers_rsp = await GetTradeOffersAsync(steamSession.APIKey, cancellationToken);
            var trade_offers = ISteamTradeService.FilterNonActiveOffers(trade_offers_rsp)?.Response?.TradeOffersReceived; // 获取活跃状态的交易报价
            if (trade_offers != null && trade_offers.Count > 0)
            {
                var confirmations = await GetConfirmations(steam_id, cancellationToken);
                foreach (var trade_offer in trade_offers)
                {
                    if (trade_offer != null)
                    {
                        var give_count = trade_offer.ItemsToGive?.Count ?? -1; // 支出物品数
                        //var receive_count = trade_offer.ItemsToReceive?.Count ?? -1; // 接收物品数
                        if (give_count == 0)
                        {
                            await AcceptTradeOfferAsync(steam_id, trade_offer.TradeOfferId, trade_offer, confirmations, cancellationToken); // 接受报价
                        }
                    }
                }
            }
        }
        return false;
    }

    static string GetTradeOfferUrl(string trade_offer_id) => string.Format(SteamApiUrls.STEAM_TRADEOFFER_URL, trade_offer_id);

    /// <summary>
    /// 获取交易报价 PartnerId
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trade_offer_id"></param>
    /// <returns></returns>
    async Task<string?> FetchTradePartnerId(SteamSession steamSession, string trade_offer_id)
    {
        var url = GetTradeOfferUrl(trade_offer_id);
        using var sendArgs = new WebApiClientSendArgs(url);
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var page_text = await SendAsync<string>(sendArgs) ?? string.Empty;
        if (page_text.Contains("You have logged in from a new device. In order to protect the items"))
            return null;

        try
        {
            var find_text = "var g_ulTradePartnerSteamID = '";
            var start = page_text.IndexOf(find_text) + find_text.Length;
            var end = page_text.IndexOf("';", start);
            return page_text[start..end];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取 SessionId
    /// </summary>
    async Task<string?> FetchSessionId(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.Cookies?["sessionid"]?.Value))
        {
            using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_LOGIN_URL);
            sendArgs.SetHttpClient(steamSession.HttpClient!);
            await SendAsync<string>(sendArgs);
        }
        return steamSession.Cookies?["sessionid"]?.Value;
    }

    /// <inheritdoc/>
    public async Task<bool> AcceptTradeOfferAsync(string steam_id, string trade_offer_id, TradeOffersInfo? tradeInfo = null, IEnumerable<TradeConfirmation>? confirmations = null, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        if (tradeInfo == null)
        {
            tradeInfo = await GetTradeOfferAsync(steamSession.APIKey, trade_offer_id, cancellationToken);
            if (tradeInfo == null)
                return false;
        }

        if (tradeInfo.TradeOfferState != TradeOfferState.Active)
            return false;

        var partner = await FetchTradePartnerId(steamSession, trade_offer_id);
        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>()
        {
            { "sessionid", sessionid.ThrowIsNull() },
            { "tradeofferid", trade_offer_id },
            { "serverid", "1" },
            { "partner", partner.ThrowIsNull() },
            { "captcha", "" },
        };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_ACCPET.Format(trade_offer_id))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.Referrer = new Uri(GetTradeOfferUrl(trade_offer_id));

                return Task.CompletedTask;
            },
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param, cancellationToken);
        return response?.IsSuccessStatusCode ?? false;
    }

    static string GenerateJsonTradeOffer(IEnumerable<TradeAsset> my_items, IEnumerable<TradeAsset> them_items)
    {
        TradeOffer offer = new()
        {
            Newversion = true,
            Version = 4,
            Me = new()
            {
                Assets = my_items,
                Currency = [],
                Ready = false,
            },
            Them = new()
            {
                Assets = them_items,
                Currency = [],
                Ready = false,
            },
        };
        var result = JsonSerializer.Serialize(offer, SteamTradeServiceImpl_TradeOffer_JsonSerializerContext_.Default.TradeOffer);
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> SendTradeOfferAsync(string steam_id, IEnumerable<TradeAsset> my_items, IEnumerable<TradeAsset> them_items, string target_steam_id, string message, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var offer_string = GenerateJsonTradeOffer(my_items, them_items);
        var sessionid = await FetchSessionId(steamSession);
        var server_id = 1;
        var param = new Dictionary<string, string>
        {
            { "sessionid", sessionid.ThrowIsNull() },
            { "serverid", server_id.ToString() },
            { "partner", target_steam_id },
            { "tradeoffermessage", message },
            { "json_tradeoffer", offer_string },
            { "captcha", "" },
            { "trade_offer_create_params", "{}" },
        };

        var partner_account_id = new SteamIdConvert(target_steam_id).Id32;
        var tradeoffer_url = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/tradeoffer/new/?partner={partner_account_id}";

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_SEND)
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("Referer", tradeoffer_url);
                req.Headers.TryAddWithoutValidation("Origin", SteamApiUrls.STEAM_COMMUNITY_URL);

                return Task.CompletedTask;
            },
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param, cancellationToken);
        return response?.IsSuccessStatusCode ?? false;
    }

    /// <inheritdoc/>
    public async Task<bool> SendTradeOfferWithUrlAsync(string steam_id, string trade_offer_url, IEnumerable<TradeAsset> my_items, IEnumerable<TradeAsset> them_items, string message, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var uri = new Uri(trade_offer_url);
        var querys = HttpUtility.ParseQueryString(uri.Query);
        var partner = querys["partner"];
        var token = querys["token"];
        if (string.IsNullOrEmpty(partner) && string.IsNullOrEmpty(token))
            return false;

        var target_steam64_id = new SteamIdConvert(partner!).Id64;
        var offer_string = GenerateJsonTradeOffer(my_items, them_items);
        var sessionid = await FetchSessionId(steamSession);
        var server_id = 1;

        var param = new Dictionary<string, string>
        {
            { "sessionid", sessionid.ThrowIsNull() },
            { "serverid", server_id.ToString() },
            { "partner", target_steam64_id.ToString() },
            { "tradeoffermessage", message },
            { "json_tradeoffer", offer_string },
            { "captcha", "" },
            { "trade_offer_create_params", $"{{ \"trade_offer_access_token\" : \"{token}\"}}" },
        };

        var tradeoffer_url = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/tradeoffer/new/{uri.Query}";

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_SEND)
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("Referer", tradeoffer_url);
                req.Headers.TryAddWithoutValidation("Origin", SteamApiUrls.STEAM_COMMUNITY_URL);

                return Task.CompletedTask;
            },
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param, cancellationToken);
        return response?.IsSuccessStatusCode ?? false;
    }

    /// <inheritdoc/>
    public async Task<bool> CancelTradeOfferAsync(string steam_id, string trade_offer_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>() { { "sessionid", sessionid.ThrowIsNull() } };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_CANCEL.Format(trade_offer_id))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param, cancellationToken);
        return response?.IsSuccessStatusCode ?? false;
    }

    /// <inheritdoc/>
    public async Task<bool> DeclineTradeOfferAsync(string steam_id, string trade_offer_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>() { { "sessionid", sessionid.ThrowIsNull() } };

        using var sendArgs = new WebApiClientSendArgs(string.Format(SteamApiUrls.STEAM_TRADEOFFER_DECLINE, trade_offer_id))
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param, cancellationToken);
        return response?.IsSuccessStatusCode ?? false;
    }

    /// <inheritdoc/>
    public async Task<TradeOffersResponse?> GetTradeOffersAsync(string api_key, CancellationToken cancellationToken = default)
    {
        var queryString = new NameValueCollection()
        {
            { "key", api_key },
            { "get_sent_offers", "1" },
            { "get_received_offers", "1" },
            { "get_descriptions", "1" },
            { "language", "english" },
            { "active_only", "1" },
            { "historical_only", "0" },
            { "time_historical_cutoff", "" },
        };
        var query = string.Join("&", queryString.AllKeys.Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));
        var builder = new UriBuilder(SteamApiUrls.STEAM_TRADEOFFER_GET_OFFERS)
        {
            Query = query,
        };

        using var sendArgs = new WebApiClientSendArgs(builder.Uri) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        var result = await SendAsync<TradeOffersResponse>(sendArgs, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<TradeOffersInfo?> GetTradeOfferAsync(string api_key, string trade_offer_id, CancellationToken cancellationToken = default)
    {
        var queryString = new NameValueCollection()
        {
            { "key", api_key },
            { "tradeofferid", trade_offer_id },
            { "language", "english" },
        };
        var query = string.Join("&", queryString.AllKeys
            .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));
        var builder = new UriBuilder(SteamApiUrls.STEAM_TRADEOFFER_GET_OFFER)
        {
            Query = query,
        };

        using var sendArgs = new WebApiClientSendArgs(builder.Uri) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        using var rsp = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        using var jsonStream = await rsp!.Content.ReadAsStreamAsync(cancellationToken);
        using var document = JsonDocument.Parse(jsonStream);
        return JsonSerializer.Deserialize(document.RootElement
            .GetProperty("response")
            .GetProperty("offer"), DefaultJsonSerializerContext_.Default.TradeOffersInfo);
    }

    /// <summary>
    /// 检验 APIKey 是否有效
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    static bool InvalidAPIKey(string content) => content.Contains("Access is denied. Retrying will not help. Please verify your <pre>key=</pre> parameter");

    /// <inheritdoc/>
    public async Task<TradeSummary?> GetTradeOffersSummaryAsync(string api_key, CancellationToken cancellationToken = default)
    {
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_GET_SUMMARY.Format(api_key)) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        var response = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);
        if (response.ThrowIsNull().IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (InvalidAPIKey(contentString))
            {
                throw new Exception("the steam api_key is invalid!");
            }

            var result = await ReadFromSJsonAsync<TradeSummaryResponse>(response.Content, cancellationToken);
            return result?.Response;
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<TradeHistory.TradeHistoryResponseDetail?> GetTradeHistory(string api_key, int maxTrades = 500, string? startTradeId = null, bool getDescriptions = false, CancellationToken cancellationToken = default)
    {
        const string Language = "schinese";

        const int MaxTrades = 500;
        const int MaxTradesWithDescriptions = 100;

        // 当获取详情时 Steam Api 最多获取 100 条信息 否则会无返回,普通情况 500 条
        if (getDescriptions)
        {
            maxTrades = maxTrades > MaxTradesWithDescriptions ? MaxTradesWithDescriptions : maxTrades;
        }
        else
        {
            maxTrades = maxTrades > MaxTrades ? MaxTrades : maxTrades;
        }

        var queryString = new NameValueCollection()
        {
            { "key", api_key },
            { "include_total", "true" },
            { "start_after_tradeid", startTradeId ??= string.Empty },
            { "max_trades", maxTrades.ToString() },
            { "get_descriptions", getDescriptions ? "1" : "0" },
            { "language", Language },
        };
        var query = string.Join("&", queryString.AllKeys.Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));
        var builder = new UriBuilder(SteamApiUrls.STEAM_TRADEOFFER_GET_HISTORY)
        {
            Query = query,
        };

        using var sendArgs = new WebApiClientSendArgs(builder.Uri) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        var result = await SendAsync<TradeHistory>(sendArgs, cancellationToken);
        return result?.Response;
    }
}

partial class SteamTradeServiceImpl // 交易确认
{
    ///// <summary>
    ///// html 解析出 TradeOfferId
    ///// </summary>
    ///// <param name="confirmation_details_page"></param>
    ///// <returns></returns>
    //static string GetConfirmationTradeOfferId(string confirmation_details_page)
    //{
    //    var parser = new HtmlParser();
    //    var document = parser.ParseDocument(confirmation_details_page);
    //    var full_id = document.QuerySelectorAll(".tradeoffer").Select(s => s.Id).FirstOrDefault();
    //    document.Dispose();
    //    return full_id?.Split('_')[1] ?? string.Empty;
    //}

    static string GenerateDeviceId(string steamId)
    {
        byte[] steamIdBytes = Encoding.ASCII.GetBytes(steamId);
        byte[] hashBytes = SHA1.HashData(steamIdBytes);
        string hexedSteamId = hashBytes.ToHexString(isLower: true);

        const string f = "android:";
        var len = f.Length + 36;
        var chars = ArrayPool<char>.Shared.Rent(len);
        try
        {
            f.CopyTo(chars);
            var s = chars.AsSpan(f.Length);
            var g = hexedSteamId.AsSpan(0, 8);
            g.CopyTo(s);
            s = s[g.Length..];
            s[0] = '-';
            g = hexedSteamId.AsSpan(8, 4);
            g.CopyTo(s);
            s = s[g.Length..];
            s[0] = '-';
            s = s[1..];
            g = hexedSteamId.AsSpan(12, 4);
            g.CopyTo(s);
            s = s[g.Length..];
            s[0] = '-';
            s = s[1..];
            g = hexedSteamId.AsSpan(16, 4);
            g.CopyTo(s);
            s = s[g.Length..];
            s[0] = '-';
            s = s[1..];
            g = hexedSteamId.AsSpan(20, 12);
            g.CopyTo(s);
            return new string(chars, 0, len);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(chars);
        }
        //return $"android:{hexedSteamId[..8]}-" +
        //    $"{hexedSteamId.Substring(8, 4)}-" +
        //    $"{hexedSteamId.Substring(12, 4)}-" +
        //    $"{hexedSteamId.Substring(16, 4)}-" +
        //    $"{hexedSteamId.Substring(20, 12)}";
    }

    static string CreateTimeHash(string identitySecret, string tag, long timestamp)
    {
        long bigEndianTimestamp = IPAddress.HostToNetworkOrder(timestamp);
        byte[] timestampBytes = BitConverter.GetBytes(bigEndianTimestamp);
        byte[] tagBytes = Encoding.ASCII.GetBytes(tag);
        byte[] buffer = new byte[timestampBytes.Length + tagBytes.Length];
        Array.Copy(timestampBytes, buffer, timestampBytes.Length);
        Array.Copy(tagBytes, 0, buffer, timestampBytes.Length, tagBytes.Length);

        byte[] identitySecretBytes = Convert.FromBase64String(identitySecret);
        using HMACSHA1 hmac = new HMACSHA1(identitySecretBytes);
        byte[] hashedData = hmac.ComputeHash(buffer);
        return Convert.ToBase64String(hashedData);
    }

    /// <summary>
    /// 生成确认参数集合
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    static Dictionary<string, string> CreateConfirmationParams(string tag, SteamSession steamSession)
    {
        var servertime = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + steamSession.ServerTimeDiff) / 1000L;
        var timehash = CreateTimeHash(steamSession.IdentitySecret, tag, servertime);
        var android_id = GenerateDeviceId(steamSession.SteamId);
        return new Dictionary<string, string>
        {
            { "p", android_id },
            { "a", steamSession.SteamId },
            { "k", timehash },
            { "t", servertime.ToString() },
            { "m", "android" },
            { "tag", tag },
        };
    }

    /// <inheritdoc/>
    public async Task<List<TradeConfirmation>> GetConfirmations(string steam_id, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        const string tag = "conf";/*TradeTag.CONF.GetDescription()*/;
        var queryString = CreateConfirmationParams(tag, steamSession);
        var query = string.Join("&", queryString.Keys
            .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

        var builder = new UriBuilder(SteamApiUrls.STEAM_MOBILECONF_GET_CONFIRMATIONS)
        {
            Query = query,
        };

        using var sendArgs = new WebApiClientSendArgs(builder.Uri)
        {
            Method = HttpMethod.Get,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("X-Requested-With", "'com.valvesoftware.android.steam.community'");

                return Task.CompletedTask;
            },
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);

        List<TradeConfirmation> confirmations = [];
        if (response != null && response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var confirmations_page = JsonDocument.Parse(contentString);
            if (contentString.Contains("Steam Guard Mobile Authenticator is providing incorrect Steam Guard codes."))
                return confirmations!;

            if (confirmations_page.RootElement.TryGetProperty("conf", out var confs))
            {
                foreach (var conf in confs.EnumerateArray())
                {
                    var confirmation = JsonSerializer.Deserialize(conf, DefaultJsonSerializerContext_.Default.TradeConfirmation);
                    if (confirmation is not null)
                        confirmations.Add(confirmation);
                }
            }
        }
        return confirmations!;
    }

    // public async Task<(string[] my_items, string[] them_items)> GetConfirmationImages(string steam_id, Confirmation confirmation)
    // {
    //     var steamSession = _sessionService.RentSession(steam_id);
    //     if (steamSession == null)
    //         throw new Exception($"Unable to find session for {steam_id}, please login first");

    //     var tag = "details" + confirmation.Id;
    //     var queryString = CreateConfirmationParams(tag, steamSession);
    //     var query = string.Join("&", queryString.Keys
    //     .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

    //     using var httpRequestMessage = new HttpRequestMessage();
    //     var builder = new UriBuilder(STEAM_MOBILECONF_GET_CONFIRMATION_DETAILS.Format(confirmation.Id));
    //     builder.Query = query;
    //     httpRequestMessage.RequestUri = builder.Uri;
    //     var response = await steamSession.HttpClient.SendAsync(httpRequestMessage);
    //     var html = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("html").ToString();

    //     var parser = new HtmlParser();
    //     var document = await parser.ParseDocumentAsync(html);
    //     var tradeoffer_items_list = document.QuerySelectorAll("div.tradeoffer_items");
    //     var my_items = new List<string>();
    //     var them_items = new List<string>();
    //     foreach (var trade_offer_items in tradeoffer_items_list)
    //     {
    //         var header = HttpUtility.UrlDecode(trade_offer_items.QuerySelector("div.tradeoffer_items_header")?.TextContent)?.Trim() ?? string.Empty;
    //         if (header.Contains("You offered") || header.Contains("你的报价"))
    //         {
    //             foreach (var trade_item in trade_offer_items.QuerySelectorAll("div.trade_item"))
    //             {
    //                 var img = trade_item.QuerySelector("img")?.Attributes["src"]?.Value.ToString() ?? string.Empty;
    //                 if (!string.IsNullOrEmpty(img))
    //                     my_items.Add(img);
    //             }
    //         }
    //         else
    //         {
    //             foreach (var trade_item in trade_offer_items.QuerySelectorAll("div.trade_item"))
    //             {
    //                 var img = trade_item.QuerySelector(".img")?.Attributes["src"]?.Value.ToString() ?? string.Empty;
    //                 if (!string.IsNullOrEmpty(img))
    //                     them_items.Add(img);
    //             }
    //         }
    //     }
    //     return (my_items.ToArray(), them_items.ToArray());
    // }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<string> my_items, IReadOnlyList<string> them_items)> GetConfirmationImages(string steam_id, TradeConfirmation confirmation, CancellationToken cancellationToken = default)
    {
        // 获取登陆状态
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        // 构建请求参数
        var tag = $"details{confirmation.Id}";
        var queryParams = CreateConfirmationParams(tag, steamSession);
        var query = string.Join("&",
            queryParams.Keys
                .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryParams[key]!)}"));

        var builder = new UriBuilder(SteamApiUrls.STEAM_MOBILECONF_GET_CONFIRMATION_DETAILS.Format(confirmation.Id))
        {
            Query = query,
        };

        using var sendArgs = new WebApiClientSendArgs(builder.Uri);
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        using var resp = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);

        resp.ThrowIsNull().EnsureSuccessStatusCode();
        using var jsonStream = await resp.Content.ReadAsStreamAsync(cancellationToken);

        var html = JsonDocument.Parse(jsonStream).RootElement.GetProperty("html").GetString();

        if (string.IsNullOrEmpty(html))
            throw new ArgumentException("获取报价图片响应错误");

        // 解析 html
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, cancellationToken);

        var tradeofferItemsGroup = document.QuerySelectorAll("div.tradeoffer_items");

        // 遍历交易中自己的物品,对方的物品
        if (tradeofferItemsGroup != null && tradeofferItemsGroup.Length > 0)
        {
            var myItems = new List<string>();
            var themItems = new List<string>();

            // 获取物品
            foreach (var itemGroup in tradeofferItemsGroup)
            {
                // 通过 div 上的 class 中是否有primary/secondary判断是自己的物品还是对方的物品(没有primary/secondary说明只有自己的物品)
                bool addToThemItems = itemGroup.ClassList.Any(x => string.Equals(x, "secondary", StringComparison.Ordinal));
                if (itemGroup.HasChildNodes)
                {
                    var items = itemGroup.QuerySelectorAll("div.trade_item");
                    foreach (var item in items)
                    {
                        var img = item.QuerySelector("img")?.Attributes["src"]?.Value.ToString() ?? string.Empty;

                        if (addToThemItems)
                            themItems.Add(img);
                        else
                            myItems.Add(img);
                    }
                }
            }
            return (myItems, themItems);
        }

        return ([], []);
    }

    /// <inheritdoc/>
    public async Task<bool> SendConfirmation(string steam_id, TradeConfirmation confirmation, bool accept, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        var tag = accept ? TradeTag.ALLOW.GetDescription() : TradeTag.CANCEL.GetDescription();
        var queryString = CreateConfirmationParams(tag!, steamSession);
        queryString.Add("op", tag!);
        queryString.Add("cid", confirmation.Id);
        queryString.Add("ck", confirmation.Nonce);
        var query = string.Join("&", queryString.Keys
         .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

        var builder = new UriBuilder(SteamApiUrls.STEAM_MOBILECONF_CONFIRMATION)
        {
            Query = query,
        };

        using var sendArgs = new WebApiClientSendArgs(builder.Uri)
        {
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                return Task.CompletedTask;
            },
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage>(sendArgs, cancellationToken);

        if (response != null && response.IsSuccessStatusCode)
        {
            using var jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var root = JsonDocument.Parse(jsonStream).RootElement;
            if (root.TryGetProperty("success", out var success))
                return success.GetBoolean();
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> BatchSendConfirmation(string steam_id, Dictionary<string, string> trades, bool accept, CancellationToken cancellationToken = default)
    {
        var steamSession = await sessions.RentSession(steam_id, cancellationToken);
        steamSession = steamSession.ThrowIsNull(steam_id);

        if (!(trades.Count > 0))
            return false;

        var conf = accept ? "accept" : "reject";

        var param = CreateConfirmationParams(conf, steamSession).ToList();
        param.Add(new("op", accept ? "allow" : "cancel"));

        foreach (var trade in trades)
        {
            param.Add(new("cid[]", trade.Key));
            param.Add(new("ck[]", trade.Value));
        }

        //string str = string.Join("&", param.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_MOBILECONF_BATCH_CONFIRMATION)
        {
            Method = HttpMethod.Post,
            ContentType = MediaTypeNames.FormUrlEncoded,
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        using var response = await SendAsync<HttpResponseMessage, List<KeyValuePair<string, string>>>(sendArgs, param, cancellationToken);
        if (response != null && response.IsSuccessStatusCode)
        {
            using var jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var root = JsonDocument.Parse(jsonStream).RootElement;
            if (root.TryGetProperty("success", out var success))
                return success.GetBoolean();
        }
        return false;
    }
}

sealed class TradeOffer
{
    public bool Newversion { get; set; }

    public int Version { get; set; }

    public TradeOfferMeThem? Me { get; set; }

    public TradeOfferMeThem? Them { get; set; }
}

sealed class TradeOfferMeThem
{
    public IEnumerable<TradeAsset>? Assets { get; set; }

    public int[]? Currency { get; set; }

    public bool Ready { get; set; }
}

[JsonSerializable(typeof(TradeOffer))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    AllowTrailingCommas = true)]
sealed partial class SteamTradeServiceImpl_TradeOffer_JsonSerializerContext_ : JsonSerializerContext
{
    static SteamTradeServiceImpl_TradeOffer_JsonSerializerContext_()
    {
        // https://github.com/dotnet/runtime/issues/94135
        s_defaultOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            PropertyNamingPolicy = global::System.Text.Json.JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
        };
        Default = new SteamTradeServiceImpl_TradeOffer_JsonSerializerContext_(new JsonSerializerOptions(s_defaultOptions));
    }
}