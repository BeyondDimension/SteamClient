using BD.SteamClient.Models.Trade;
using System;
using System.Collections.Specialized;

namespace BD.SteamClient.Services.Implementation;

public sealed partial class SteamTradeServiceImpl : HttpClientUseCookiesWithDynamicProxyServiceImpl, ISteamTradeService
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tasks;

    public SteamTradeServiceImpl(
        IServiceProvider s,
        ILogger<SteamTradeServiceImpl> logger) : base(
            s, logger)
    {
        _tasks = new ConcurrentDictionary<string, CancellationTokenSource>();
    }

    public SteamTradeServiceImpl(
    IServiceProvider s,
    Func<CookieContainer, HttpMessageHandler> func) : base(func, s.GetRequiredService<ILogger<SteamTradeServiceImpl>>())
    {
        _tasks = new ConcurrentDictionary<string, CancellationTokenSource>();
    }

    #region Tasks
    public void StartTradeTask(SteamSession steamSession, int interval, TradeTaskEnum tradeTaskEnum)
    {
        var taskName = $"{steamSession.SteamId}_{tradeTaskEnum.ToString()}";
        if (!_tasks.ContainsKey(taskName))
        {

            Action action;
            switch (tradeTaskEnum)
            {
                case TradeTaskEnum.None:
                    return;
                case TradeTaskEnum.AutoAcceptGitTrade:
                    action = async () => { await AcceptAllGiftTradeOfferAsync(steamSession); };
                    break;
                default:
                    return;
            }

            var cancellationTokenSource = new CancellationTokenSource();
            _tasks.TryAdd(taskName, cancellationTokenSource);
            Task.Run(() => RunTask(action, interval, cancellationTokenSource.Token));
        }
    }

    public void StopTask(SteamSession steamSession, TradeTaskEnum tradeTaskEnum)
    {
        var taskName = $"{steamSession.SteamId}_{tradeTaskEnum.ToString()}";
        if (_tasks.TryGetValue(taskName, out var cancellationTokenSource))
        {
            cancellationTokenSource.Cancel();
            _tasks.TryRemove(taskName, out _);
        }
    }

    private async Task RunTask(Action action, int interval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // 执行后台任务
                action();
                await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
            }
            catch (Exception ex)
            {
                // 处理异常，也可以选择记录日志等操作
                Console.WriteLine($"Exception occurred in background task: {ex.Message}");
            }
        }
    }
    #endregion

    #region Public

    public async Task<bool> AcceptAllGiftTradeOfferAsync(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.APIKey))
            return false;

        var response = await GetTradeOffersSummaryAsync(steamSession.APIKey);
        if (response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync();
            if (InvalidAPIKey(contentString))
                return false;

            var trade_summary = (await response.Content.ReadFromJsonAsync(TradeSummaryResponse_.Default.TradeSummaryResponse)).Response;
            if (trade_summary != null && trade_summary.PendingReceivedCount > 0)
            {
                response = await GetTradeOffersAsync(steamSession.APIKey);
                if (response.IsSuccessStatusCode)
                {
                    var trade_offers_rsp = await response.Content.ReadFromJsonAsync(TradeResponse_.Default.TradeResponse);
                    var trade_offers = FilterNonActiveOffers(trade_offers_rsp!).Response.TradeOffersReceived;
                    if (trade_offers.Count > 0)
                    {
                        foreach (var trade_offer in trade_offers)
                        {
                            var give_count = trade_offer?.ItemsToGive?.Count ?? 0; // 支出物品数
                            var receive_count = trade_offer?.ItemsToReceive?.Count ?? 0; // 接收物品数
                            if (give_count == 0)
                            {
                                await AcceptTradeOfferAsync(steamSession, trade_offer.TradeOfferId, trade_offer); // 接受报价
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public async Task<bool> BatchHandleTradeOfferAsync(SteamSession steamSession, Dictionary<string, string> trades, bool accept)
    {
        if (!trades.Any())
            return false;

        return await new ConfirmationExecutor(steamSession).BatchHandleTradeOffer(trades, accept);
    }

    public async Task<bool> AcceptTradeOfferAsync(SteamSession steamSession, string trade_offer_id, TradeInfo? tradeInfo)
    {
        if (tradeInfo == null)
        {
            tradeInfo = new();
        }

        if (tradeInfo.TradeOfferState != TradeOfferState.Active)
            return false;

        var partner = await FetchTradePartnerId(steamSession, trade_offer_id);
        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>()
        {
            { "sessionid", sessionid },
            { "tradeofferid", trade_offer_id },
            { "serverid", "1" },
            { "partner", partner },
            { "captcha", "" }
        };
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, STEAM_TRADEOFFER_ACCPET.Format(trade_offer_id))
        {
            Content = new FormUrlEncodedContent(param)
        };
        httpRequestMessage.Headers.Referrer = new Uri(GetTradeOfferUrl(trade_offer_id));
        httpRequestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        var response = await steamSession.HttpClient.SendAsync(httpRequestMessage);

        if (response.IsSuccessStatusCode)
        {
            var content_string = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(content_string).RootElement;
            if (root.TryGetProperty("needs_mobile_confirmation", out var _))
                return await new ConfirmationExecutor(steamSession).SendTradeAllowRequest(trade_offer_id);

            return true;
        }
        return false;
    }

    public async Task<HttpResponseMessage> GetTradeOffersAsync(string api_key)
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
        var query = string.Join("&", queryString.AllKeys
        .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));
        var builder = new UriBuilder(STEAM_TRADEOFFER_GET_OFFERS);
        builder.Query = query;

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
        return await client.SendAsync(httpRequestMessage);
    }

    public async Task<HttpResponseMessage> GetTradeOffersSummaryAsync(string api_key)
    {
        return await client.GetAsync(STEAM_TRADEOFFER_GET_SUMMARY.Format(api_key));
    }

    public async Task<bool> SendTradeOfferAsync(SteamSession steamSession, List<Asset> my_itmes, List<Asset> them_items, string target_steam_id, string message)
    {
        var offer_string = GenerateJsonTradeOffer(my_itmes, them_items);
        var sessionid = await FetchSessionId(steamSession);
        var server_id = 1;
        var param = new Dictionary<string, string>();
        param.Add("sessionid", sessionid);
        param.Add("serverid", server_id.ToString());
        param.Add("partner", target_steam_id);
        param.Add("tradeoffermessage", message);
        param.Add("json_tradeoffer", offer_string);
        param.Add("captcha", "");
        param.Add("trade_offer_create_params", "{}");

        var partner_account_id = ToSteamId32(target_steam_id);
        var tradeoffer_url = $"{STEAM_COMMUNITY_URL}/tradeoffer/new/?partner={partner_account_id}";

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, STEAM_TRADEOFFER_SEND);
        httpRequestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        httpRequestMessage.Headers.TryAddWithoutValidation("Referer", tradeoffer_url);
        httpRequestMessage.Headers.TryAddWithoutValidation("Origin", STEAM_COMMUNITY_URL);
        httpRequestMessage.Content = new FormUrlEncodedContent(param);

        var response = await client.SendAsync(httpRequestMessage);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = Encoding.UTF8.GetString(bytes);
        if (response.IsSuccessStatusCode)
        {
            var content_string = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(content_string).RootElement;
            if (root.TryGetProperty("needs_mobile_confirmation", out var _))
                return await new ConfirmationExecutor(steamSession).SendTradeAllowRequest(root.GetProperty("response").GetProperty("tradeofferid").ToString());

            return true;
        }
        return false;
    }

    public async Task<bool> SendTradeOfferWithUrlAsync(SteamSession steamSession, string trade_offer_url, List<Asset> my_itmes, List<Asset> them_items, string message)
    {
        var uri = new Uri(trade_offer_url);
        var querys = HttpUtility.ParseQueryString(uri.Query);
        var partner = querys["partner"];
        var token = querys["token"];
        if (string.IsNullOrEmpty(partner) && string.IsNullOrEmpty(token))
            return false;

        var target_steam64_id = ToSteamId64(partner!);
        var offer_string = GenerateJsonTradeOffer(my_itmes, them_items);
        var sessionid = await FetchSessionId(steamSession);
        var server_id = 1;

        var param = new Dictionary<string, string>();
        param.Add("sessionid", sessionid);
        param.Add("serverid", server_id.ToString());
        param.Add("partner", target_steam64_id.ToString());
        param.Add("tradeoffermessage", message);
        param.Add("json_tradeoffer", offer_string);
        param.Add("captcha", "");
        param.Add("trade_offer_create_params", $"{{ \"trade_offer_access_token\" : \"{token}\"}}");

        var tradeoffer_url = $"{STEAM_COMMUNITY_URL}/tradeoffer/new/{uri.Query}";

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, STEAM_TRADEOFFER_SEND);
        httpRequestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        httpRequestMessage.Headers.TryAddWithoutValidation("Referer", tradeoffer_url);
        httpRequestMessage.Headers.TryAddWithoutValidation("Origin", STEAM_COMMUNITY_URL);
        httpRequestMessage.Content = new FormUrlEncodedContent(param);

        var response = await steamSession.HttpClient.SendAsync(httpRequestMessage);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = Encoding.UTF8.GetString(bytes);
        if (response.IsSuccessStatusCode)
        {
            var content_string = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(content_string).RootElement;
            if (root.TryGetProperty("needs_mobile_confirmation", out var _))
                return await new ConfirmationExecutor(steamSession).SendTradeAllowRequest(root.GetProperty("tradeofferid").ToString());

            return true;
        }
        return false;
    }

    public async Task<bool> CancelTradeOfferAsync(SteamSession steamSession)
    {
        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>()
        {
            { "sessionid", sessionid }
        };
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, STEAM_TRADEOFFER_CANCEL);
        httpRequestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        httpRequestMessage.Content = new FormUrlEncodedContent(param);
        var response = await steamSession.HttpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            if (root.TryGetProperty("success", out var success))
                return success.GetBoolean();
        }
        return false;
    }

    public async Task<bool> DeclineTradeOfferAsync(SteamSession steamSession)
    {

        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>()
        {
            { "sessionid", sessionid }
        };
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, STEAM_TRADEOFFER_DECLINE);
        httpRequestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        httpRequestMessage.Content = new FormUrlEncodedContent(param);
        var response = await steamSession.HttpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            if (root.TryGetProperty("success", out var success))
                return success.GetBoolean();
        }
        return false;
    }
    #endregion

    #region Private
    private static bool InvalidAPIKey(string content) => content.Contains("Access is denied. Retrying will not help. Please verify your <pre>key=</pre> parameter");

    private static ulong ToSteamId64(string steam_id) => 76561197960265728ul + ulong.Parse(steam_id);

    private static int ToSteamId32(string steam_id) => Convert.ToInt32((long.Parse(steam_id) >> 0) & 0xFFFFFFFF);

    private static TradeResponse FilterNonActiveOffers(TradeResponse tradeResponse)
    {
        if (tradeResponse?.Response?.TradeOffersSent != null)
            tradeResponse.Response.TradeOffersSent = tradeResponse.Response.TradeOffersSent.Where(x => x.TradeOfferState == TradeOfferState.Active).ToList();

        if (tradeResponse?.Response?.TradeOffersReceived != null)
            tradeResponse.Response.TradeOffersReceived = tradeResponse.Response.TradeOffersReceived.Where(x => x.TradeOfferState == TradeOfferState.Active).ToList();

        return tradeResponse;
    }

    private static string GetTradeOfferUrl(string trade_offer_id) => STEAM_TRADEOFFER_URL.Format(trade_offer_id);

    private static async Task<string> FetchTradePartnerId(SteamSession steamSession, string trade_offer_id)
    {
        var url = GetTradeOfferUrl(trade_offer_id);
        var page_text = await steamSession.HttpClient.GetStringAsync(url);
        if (page_text.Contains("You have logged in from a new device. In order to protect the items"))
            return string.Empty;

        var find_text = "var g_ulTradePartnerSteamID = '";
        var start = page_text.IndexOf(find_text) + find_text.Length;
        var end = page_text.IndexOf("';", start);
        return page_text[start..end];
    }

    private static async Task<string> FetchSessionId(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.CookieContainer.GetCookieValue(new Uri(STEAM_COMMUNITY_URL), "sessionid")))
        {
            await steamSession.HttpClient.GetAsync(STEAM_LOGIN_URL);
        }
        return steamSession.CookieContainer.GetCookieValue(new Uri(STEAM_COMMUNITY_URL), "sessionid")!;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    private static string GenerateJsonTradeOffer(List<Asset> my_itmes, List<Asset> them_items)
    {
        var offer = new
        {
            Newversion = true,
            Version = 4,
            Me = new
            {
                Assets = my_itmes,
                Currency = Array.Empty<int>(),
                Ready = false
            },
            Them = new
            {
                Assets = them_items,
                Currency = Array.Empty<int>(),
                Ready = false
            }
        };
        return JsonSerializer.Serialize(offer, options: new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    #endregion

    internal class ConfirmationExecutor
    {
        private SteamSession _steamSession;

        public ConfirmationExecutor(SteamSession steamSession)
        {
            _steamSession = steamSession;
        }

        public async Task<bool> SendTradeAllowRequest(string trade_offer_id)
        {
            var confirmations = await GetComfirmations();
            var confirmation = await SelectTradeOfferConfirmation(confirmations, trade_offer_id);
            if (confirmation != null)
            {
                var response = await SendConfirmation(confirmation);
                if (response.IsSuccessStatusCode)
                {
                    var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                    if (root.TryGetProperty("success", out var success))
                        return success.GetBoolean();
                }
            }
            return false;
        }

        public async Task<bool> BatchHandleTradeOffer(Dictionary<string, string> trades, bool accept)
        {
            var conf = accept ? "accept" : "reject";
            var cids = string.Join(",", trades.Select(s => s.Key));
            var cks = string.Join(",", trades.Select(s => s.Value));

            var param = CreateConfirmationParams(conf);
            param.Add("op", accept ? "allow" : "cancel");
            param.Add("cid[]", cids);
            param.Add("ck[]", cks);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, STEAM_MOBILECONF_BATCH_CONFIRMATION);
            httpRequestMessage.Content = new FormUrlEncodedContent(param);
            var response = await _steamSession.HttpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                if (root.TryGetProperty("success", out var success))
                    return success.GetBoolean();
            }
            return false;
        }

        /// <summary>
        /// 发送交易确认
        /// </summary>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendConfirmation(Confirmation confirmation)
        {
            var tag = TradeTag.ALLOW.GetDescription()!;
            var queryString = CreateConfirmationParams(tag);
            queryString.Add("op", tag);
            queryString.Add("cid", confirmation.DataConfId);
            queryString.Add("ck", confirmation.Nonce);
            var query = string.Join("&", queryString.Keys
             .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

            using var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            var builder = new UriBuilder(STEAM_MOBILECONF_CONFIRMATION);
            builder.Query = query;
            httpRequestMessage.RequestUri = builder.Uri;
            return await _steamSession.HttpClient.SendAsync(httpRequestMessage);
        }

        #region Private

        /// <summary>
        /// 获取待确认交易信息
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<Confirmation>> GetComfirmations()
        {
            var response = await FetchConfirmationsPage();
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                var confirmations_page = JsonDocument.Parse(contentString).RootElement;
                if (contentString.Contains("Steam Guard Mobile Authenticator is providing incorrect Steam Guard codes."))
                    return Enumerable.Empty<Confirmation>();

                var confirmations = new List<Confirmation>();
                foreach (var conf in confirmations_page.GetProperty("conf").EnumerateArray())
                {
                    var confirmation = new Confirmation()
                    {
                        DataConfId = conf.GetProperty("id").ToString(),
                        Nonce = conf.GetProperty("nonce").ToString(),
                    };
                    confirmations.Add(confirmation);
                }
                return confirmations;
            }
            return Enumerable.Empty<Confirmation>();
        }

        /// <summary>
        /// 获取待确认交易信息 Confirmations 请求
        /// </summary>
        /// <returns></returns>
        private async Task<HttpResponseMessage> FetchConfirmationsPage()
        {
            var tag = TradeTag.CONF.GetDescription();
            var queryString = CreateConfirmationParams(tag!);
            var query = string.Join("&", queryString.Keys
            .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

            using var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.TryAddWithoutValidation("X-Requested-With", "'com.valvesoftware.android.steam.community'");
            var builder = new UriBuilder(STEAM_MOBILECONF_GET_CONFIRMATIONS);
            builder.Query = query;
            httpRequestMessage.RequestUri = builder.Uri;
            return await _steamSession.HttpClient.SendAsync(httpRequestMessage);
        }

        /// <summary>
        /// 获取指定 Confirmation 详细信息
        /// </summary>
        /// <returns></returns>
        private async Task<string> FetchConfirmationDetailsPage(Confirmation confirmation)
        {
            var tag = "details" + confirmation.DataConfId;
            var queryString = CreateConfirmationParams(tag);
            var query = string.Join("&", queryString.Keys
            .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

            using var httpRequestMessage = new HttpRequestMessage();
            var builder = new UriBuilder(STEAM_MOBILECONF_GET_CONFIRMATION_DETAILS.Format(confirmation.DataConfId));
            builder.Query = query;
            httpRequestMessage.RequestUri = builder.Uri;
            var response = await _steamSession.HttpClient.SendAsync(httpRequestMessage);
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("html").ToString();
        }

        /// <summary>
        /// 生成确认参数集合
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private Dictionary<string, string> CreateConfirmationParams(string tag)
        {
            var servertime = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _steamSession.ServerTimeDiff) / 1000L;
            var timehash = CteateTimeHash(_steamSession.IdentitySecret, tag, servertime);
            var android_id = GenerateDeviceId(_steamSession.SteamId);
            return new Dictionary<string, string>
            {
                { "p", android_id },
                { "a", _steamSession.SteamId },
                { "k", timehash },
                { "t", servertime.ToString() },
                { "m", "android" },
                { "tag", tag }
            };
        }

        /// <summary>
        /// 过滤指定 tradeOfferId 确认消息
        /// </summary>
        /// <param name="confirmations"></param>
        /// <param name="tradeOfferId"></param>
        /// <returns></returns>
        private async Task<Confirmation?> SelectTradeOfferConfirmation(IEnumerable<Confirmation> confirmations, string tradeOfferId)
        {
            foreach (var confirmation in confirmations)
            {
                var confirmation_details_page = await FetchConfirmationDetailsPage(confirmation);
                var confirmation_id = GetConfirmationTradeOfferId(confirmation_details_page);
                if (confirmation_id == tradeOfferId)
                    return confirmation;
            }
            return null;
        }

        /// <summary>
        /// html解析出 TradeOfferId
        /// </summary>
        /// <param name="confirmation_details_page"></param>
        /// <returns></returns>
        private static string GetConfirmationTradeOfferId(string confirmation_details_page)
        {
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var document = parser.ParseDocument(confirmation_details_page);
            var full_id = document.QuerySelectorAll(".tradeoffer").Select(s => s.Id).FirstOrDefault();
            document.Dispose();
            return full_id?.Split('_')[1] ?? string.Empty;
        }
        #endregion

        #region Guard

        private static string CteateTimeHash(string identitySecret, string tag, long timestamp)
        {
            long bigEndianTimestamp = IPAddress.HostToNetworkOrder(timestamp);
            byte[] timestampBytes = BitConverter.GetBytes(bigEndianTimestamp);
            byte[] tagBytes = Encoding.ASCII.GetBytes(tag);
            byte[] buffer = new byte[timestampBytes.Length + tagBytes.Length];
            Array.Copy(timestampBytes, buffer, timestampBytes.Length);
            Array.Copy(tagBytes, 0, buffer, timestampBytes.Length, tagBytes.Length);

            byte[] identitySecretBytes = Convert.FromBase64String(identitySecret);
            using (HMACSHA1 hmac = new HMACSHA1(identitySecretBytes))
            {
                byte[] hashedData = hmac.ComputeHash(buffer);
                return Convert.ToBase64String(hashedData);
            }
        }

        private static string GenerateDeviceId(string steamId)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] steamIdBytes = Encoding.ASCII.GetBytes(steamId);
                byte[] hashBytes = sha1.ComputeHash(steamIdBytes);
                string hexedSteamId = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return $"android:{hexedSteamId[..8]}-" +
                    $"{hexedSteamId.Substring(8, 4)}-" +
                    $"{hexedSteamId.Substring(12, 4)}-" +
                    $"{hexedSteamId.Substring(16, 4)}-" +
                    $"{hexedSteamId.Substring(20, 12)}";
            }
        }
        #endregion
    }
}
