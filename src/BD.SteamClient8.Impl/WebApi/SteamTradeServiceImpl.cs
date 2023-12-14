namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600 // Elements should be documented

public sealed partial class SteamTradeServiceImpl : WebApiClientFactoryService, ISteamTradeService
{
    const string TAG = "SteamTradeWebApiS";

    protected override string ClientName => TAG;

    protected override SystemTextJsonSerializerContext? JsonSerializerContext => DefaultJsonSerializerContext_.Default;

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tasks;

    private readonly ISteamSessionService _sessionService;

    public SteamTradeServiceImpl(
        IServiceProvider s,
        ILoggerFactory loggerFactory) : base(
            loggerFactory.CreateLogger(TAG),
            s)
    {
        _tasks = new ConcurrentDictionary<string, CancellationTokenSource>();
        _sessionService = s.GetRequiredService<ISteamSessionService>();
    }

    #region Public

    #region Tasks 后台任务
    public ApiRspImpl StartTradeTask(string steam_id, TimeSpan interval, TradeTaskEnum tradeTaskEnum)
    {
        try
        {
            var taskName = $"{steam_id}_{tradeTaskEnum.ToString()}";
            if (!_tasks.ContainsKey(taskName))
            {
                Action action;
                switch (tradeTaskEnum)
                {
                    case TradeTaskEnum.None:
                        return ApiRspHelper.Fail();
                    case TradeTaskEnum.AutoAcceptGitTrade:
                        action = async () => { await AcceptAllGiftTradeOfferAsync(steam_id); };
                        break;
                    default:
                        return ApiRspHelper.Fail();
                }

                var cancellationTokenSource = new CancellationTokenSource();
                _tasks.TryAdd(taskName, cancellationTokenSource);
                _ = Task.Run(() => RunTask(action, interval, cancellationTokenSource.Token));
            }
            return ApiRspHelper.Ok();
        }
        catch (Exception ex)
        {
            return ApiRspHelper.Exception(ex);
        }
    }

    public ApiRspImpl StopTask(string steam_id, TradeTaskEnum tradeTaskEnum)
    {
        try
        {
            var taskName = $"{steam_id}_{tradeTaskEnum.ToString()}";
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
    #endregion

    #region Trade 交易报价
    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    public async Task<ApiRspImpl<bool>> AcceptAllGiftTradeOfferAsync(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        if (string.IsNullOrEmpty(steamSession.APIKey))
            return false;

        var trade_summary = (await GetTradeOffersSummaryAsync(steamSession.APIKey)).Content;
        if (trade_summary != null && trade_summary.PendingReceivedCount > 0)
        {
            var trade_offers_rsp = await GetTradeOffersAsync(steamSession.APIKey);
            var trade_offers = trade_offers_rsp.IsSuccess ? ISteamTradeService.FilterNonActiveOffers(trade_offers_rsp.Content!)?.Response?.TradeOffersReceived : null; // 获取活跃状态的交易报价
            if (trade_offers != null && trade_offers.Count > 0)
            {
                var confirmations = (await GetConfirmations(steam_id)).Content;
                foreach (var trade_offer in trade_offers)
                {
                    if (trade_offer != null)
                    {
                        var give_count = trade_offer.ItemsToGive?.Count ?? -1; // 支出物品数
                        var receive_count = trade_offer.ItemsToReceive?.Count ?? -1; // 接收物品数
                        if (give_count == 0)
                        {
                            await AcceptTradeOfferAsync(steam_id, trade_offer.TradeOfferId, trade_offer, confirmations); // 接受报价
                        }
                    }
                }
            }
        }
        return false;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    public async Task<ApiRspImpl<bool>> AcceptTradeOfferAsync(string steam_id, string trade_offer_id, TradeInfo? tradeInfo = null, IEnumerable<Confirmation>? confirmations = null)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        if (tradeInfo == null)
        {
            tradeInfo = (await GetTradeOfferAsync(steamSession.APIKey, trade_offer_id)).Content;
            if (tradeInfo == null)
                return false;
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

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_ACCPET.Format(trade_offer_id))
        {
            Method = HttpMethod.Post,
            ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.Referrer = new Uri(GetTradeOfferUrl(trade_offer_id));
            }
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param);
        return response?.IsSuccessStatusCode ?? false;
    }

    public async Task<ApiRspImpl<TradeResponse?>> GetTradeOffersAsync(string api_key)
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
        var builder = new UriBuilder(SteamApiUrls.STEAM_TRADEOFFER_GET_OFFERS);
        builder.Query = query;

        using var sendArgs = new WebApiClientSendArgs(builder.Uri) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        return await SendAsync<TradeResponse>(sendArgs);
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    public async Task<ApiRspImpl<TradeInfo?>> GetTradeOfferAsync(string api_key, string trade_offer_id)
    {
        var queryString = new NameValueCollection()
        {
            { "key", api_key },
            { "tradeofferid", trade_offer_id },
            { "language", "english" },
        };
        var query = string.Join("&", queryString.AllKeys
        .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));
        var builder = new UriBuilder(SteamApiUrls.STEAM_TRADEOFFER_GET_OFFER);
        builder.Query = query;

        using var sendArgs = new WebApiClientSendArgs(builder.Uri) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        var json = await SendAsync<string>(sendArgs);

        using (var document = JsonDocument.Parse(json.ThrowIsNull()))
        {
            return document.RootElement
                .GetProperty("response")
                .GetProperty("offer")
                .Deserialize<TradeInfo>().ThrowIsNull();
        }
    }

    public async Task<ApiRspImpl<TradeSummary?>> GetTradeOffersSummaryAsync(string api_key)
    {
        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_GET_SUMMARY.Format(api_key)) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        try
        {
            var response = await SendAsync<HttpResponseMessage>(sendArgs);
            if (response.ThrowIsNull().IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                if (InvalidAPIKey(contentString))
                    throw new Exception("the steam api_key is invalid!");

                return (await ReadFromSJsonAsync<TradeSummaryResponse>(response.Content, null))?.Response;
            }
            return "GetTradeOffersSummaryAsync Response IsSuccessStatusCode false";
        }
        catch (Exception ex)
        {
            return OnErrorReApiRspBase<ApiRspImpl<TradeSummary?>>(ex, sendArgs);
        }
    }

    public async Task<ApiRspImpl<TradeHistory.TradeHistoryResponseDetail?>> GetTradeHistory(string api_key, int maxTrades = 500, string? startTradeId = null, bool getDescriptions = false)
    {
        const string Language = "schinese";

        const int MaxTrades = 500;
        const int MaxTradesWithDescriptions = 100;

        // 当获取详情时 Steam Api 最多获取100条信息 否则会无返回,普通情况500条
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
        var query = string.Join("&", queryString.AllKeys
        .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));
        var builder = new UriBuilder(SteamApiUrls.STEAM_TRADEOFFER_GET_HISTORY);
        builder.Query = query;

        using var sendArgs = new WebApiClientSendArgs(builder.Uri) { Method = HttpMethod.Get };
        sendArgs.SetHttpClient(CreateClient());
        return (await SendAsync<TradeHistory>(sendArgs))?.Response;
    }

    public async Task<ApiRspImpl<bool>> SendTradeOfferAsync(string steam_id, List<Asset> my_items, List<Asset> them_items, string target_steam_id, string message)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        var offer_string = GenerateJsonTradeOffer(my_items, them_items);
        var sessionid = await FetchSessionId(steamSession);
        var server_id = 1;
        var param = new Dictionary<string, string>
        {
            { "sessionid", sessionid },
            { "serverid", server_id.ToString() },
            { "partner", target_steam_id },
            { "tradeoffermessage", message },
            { "json_tradeoffer", offer_string },
            { "captcha", "" },
            { "trade_offer_create_params", "{}" }
        };

        var partner_account_id = ToSteamId32(target_steam_id);
        var tradeoffer_url = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/tradeoffer/new/?partner={partner_account_id}";

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_SEND)
        {
            Method = HttpMethod.Post,
            ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("Referer", tradeoffer_url);
                req.Headers.TryAddWithoutValidation("Origin", SteamApiUrls.STEAM_COMMUNITY_URL);
            }
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param);
        return response?.IsSuccessStatusCode ?? false;
    }

    public async Task<ApiRspImpl<bool>> SendTradeOfferWithUrlAsync(string steam_id, string trade_offer_url, List<Asset> my_items, List<Asset> them_items, string message)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        var uri = new Uri(trade_offer_url);
        var querys = HttpUtility.ParseQueryString(uri.Query);
        var partner = querys["partner"];
        var token = querys["token"];
        if (string.IsNullOrEmpty(partner) && string.IsNullOrEmpty(token))
            return false;

        var target_steam64_id = ToSteamId64(partner!);
        var offer_string = GenerateJsonTradeOffer(my_items, them_items);
        var sessionid = await FetchSessionId(steamSession);
        var server_id = 1;

        var param = new Dictionary<string, string>
        {
            { "sessionid", sessionid },
            { "serverid", server_id.ToString() },
            { "partner", target_steam64_id.ToString() },
            { "tradeoffermessage", message },
            { "json_tradeoffer", offer_string },
            { "captcha", "" },
            { "trade_offer_create_params", $"{{ \"trade_offer_access_token\" : \"{token}\"}}" }
        };

        var tradeoffer_url = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/tradeoffer/new/{uri.Query}";

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_SEND)
        {
            Method = HttpMethod.Post,
            ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("Referer", tradeoffer_url);
                req.Headers.TryAddWithoutValidation("Origin", SteamApiUrls.STEAM_COMMUNITY_URL);
            }
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param);
        return response?.IsSuccessStatusCode ?? false;
    }

    public async Task<ApiRspImpl<bool>> CancelTradeOfferAsync(string steam_id, string trade_offer_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>() { { "sessionid", sessionid } };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_CANCEL.Format(trade_offer_id))
        {
            Method = HttpMethod.Post,
            ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param);
        return response?.IsSuccessStatusCode ?? false;
    }

    public async Task<ApiRspImpl<bool>> DeclineTradeOfferAsync(string steam_id, string trade_offer_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        var sessionid = await FetchSessionId(steamSession);
        var param = new Dictionary<string, string>() { { "sessionid", sessionid } };

        using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_TRADEOFFER_DECLINE.Format(trade_offer_id))
        {
            Method = HttpMethod.Post,
            ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<HttpResponseMessage, Dictionary<string, string>>(sendArgs, param);
        return response?.IsSuccessStatusCode ?? false;
    }
    #endregion

    #region Confirmation 交易确认
    public async Task<ApiRspImpl<IEnumerable<Confirmation>>> GetConfirmations(string steam_id)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        var tag = TradeTag.CONF.GetDescription();
        var queryString = CreateConfirmationParams(tag!, steamSession);
        var query = string.Join("&", queryString.Keys
        .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

        var builder = new UriBuilder(SteamApiUrls.STEAM_MOBILECONF_GET_CONFIRMATIONS);
        builder.Query = query;

        using var sendArgs = new WebApiClientSendArgs(builder.Uri)
        {
            Method = HttpMethod.Get,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("X-Requested-With", "'com.valvesoftware.android.steam.community'");
            }
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var response = await SendAsync<HttpResponseMessage>(sendArgs);

        List<Confirmation> failed = [];
        if (response != null && response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync();
            using var confirmations_page = JsonDocument.Parse(contentString);
            if (contentString.Contains("Steam Guard Mobile Authenticator is providing incorrect Steam Guard codes."))
                return failed!;

            var confirmations = confirmations_page.RootElement.GetProperty("conf").Deserialize<IEnumerable<Confirmation>>();
            confirmations ??= failed;
            return ApiRspHelper.Ok(confirmations)!;
        }
        return failed!;
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
    public async Task<ApiRspImpl<(string[] my_items, string[] them_items)>> GetConfirmationImages(string steam_id, Confirmation confirmation)
    {
        // 获取登陆状态
        var steamSession = _sessionService.RentSession(steam_id) ?? throw new Exception($"Unable to find session for {steam_id}, please login first");

        // 构建请求参数
        var tag = $"details{confirmation.Id}";
        var queryParams = CreateConfirmationParams(tag, steamSession);
        var query = string.Join("&",
            queryParams.Keys
                .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryParams[key]!)}"));

        var builder = new UriBuilder(SteamApiUrls.STEAM_MOBILECONF_GET_CONFIRMATION_DETAILS.Format(confirmation.Id));
        builder.Query = query;

        using var sendArgs = new WebApiClientSendArgs(builder.Uri);
        sendArgs.SetHttpClient(steamSession.HttpClient!);
        var resp = await SendAsync<HttpResponseMessage>(sendArgs);

        resp.ThrowIsNull().EnsureSuccessStatusCode();

        var html = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement.GetProperty("html").GetString();

        if (string.IsNullOrEmpty(html))
            throw new ArgumentException("获取报价图片响应错误");

        // 解析html
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html);

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
            return (myItems.ToArray(), themItems.ToArray());
        }

        return (Array.Empty<string>(), Array.Empty<string>());
    }

    public async Task<ApiRspImpl<bool>> SendConfirmation(string steam_id, Confirmation confirmation, bool accept)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

        var tag = accept ? TradeTag.ALLOW.GetDescription() : TradeTag.CANCEL.GetDescription();
        var queryString = CreateConfirmationParams(tag!, steamSession);
        queryString.Add("op", tag!);
        queryString.Add("cid", confirmation.Id);
        queryString.Add("ck", confirmation.Nonce);
        var query = string.Join("&", queryString.Keys
         .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(queryString[key]!)}"));

        var builder = new UriBuilder(SteamApiUrls.STEAM_MOBILECONF_CONFIRMATION);
        builder.Query = query;

        using var sendArgs = new WebApiClientSendArgs(builder.Uri)
        {
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            }
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage>(sendArgs);

        if (response != null && response.IsSuccessStatusCode)
        {
            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            if (root.TryGetProperty("success", out var success))
                return success.GetBoolean();
        }
        return false;
    }

    public async Task<ApiRspImpl<bool>> BatchSendConfirmation(string steam_id, Dictionary<string, string> trades, bool accept)
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, please login first");

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
            ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
        };
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var response = await SendAsync<HttpResponseMessage, List<KeyValuePair<string, string>>>(sendArgs, param);
        if (response != null && response.IsSuccessStatusCode)
        {
            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            if (root.TryGetProperty("success", out var success))
                return success.GetBoolean();
        }
        return false;
    }

    #endregion

    #endregion

    #region Private

    #region Tasks 后台任务
    private async Task RunTask(Action action, TimeSpan interval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // 执行后台任务
                action();
                await Task.Delay(interval, cancellationToken);
            }
            catch (Exception ex)
            {
                // 处理异常，也可以选择记录日志等操作
                Console.WriteLine($"Exception occurred in background task: {ex.Message}");
            }
        }
    }
    #endregion

    #region Trade 交易报价

    /// <summary>
    /// 检验 APIKey 是否有效
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private static bool InvalidAPIKey(string content) => content.Contains("Access is denied. Retrying will not help. Please verify your <pre>key=</pre> parameter");

    private static ulong ToSteamId64(string steam_id) => 76561197960265728ul + ulong.Parse(steam_id);

    private static int ToSteamId32(string steam_id) => Convert.ToInt32((long.Parse(steam_id) >> 0) & 0xFFFFFFFF);

    private static string GetTradeOfferUrl(string trade_offer_id) => SteamApiUrls.STEAM_TRADEOFFER_URL.Format(trade_offer_id);

    /// <summary>
    /// 获取交易报价 PartnerId
    /// </summary>
    /// <param name="steamSession"></param>
    /// <param name="trade_offer_id"></param>
    /// <returns></returns>
    private async Task<string> FetchTradePartnerId(SteamSession steamSession, string trade_offer_id)
    {
        var url = GetTradeOfferUrl(trade_offer_id);
        using var sendArgs = new WebApiClientSendArgs(url);
        sendArgs.SetHttpClient(steamSession.HttpClient!);

        var page_text = await SendAsync<string>(sendArgs) ?? string.Empty;
        if (page_text.Contains("You have logged in from a new device. In order to protect the items"))
            return string.Empty;

        var find_text = "var g_ulTradePartnerSteamID = '";
        var start = page_text.IndexOf(find_text) + find_text.Length;
        var end = page_text.IndexOf("';", start);
        return page_text[start..end];
    }

    /// <summary>
    /// 获取 SessionId
    /// </summary>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    private async Task<string> FetchSessionId(SteamSession steamSession)
    {
        if (string.IsNullOrEmpty(steamSession.Cookies?["sessionid"].Value))
        {
            using var sendArgs = new WebApiClientSendArgs(SteamApiUrls.STEAM_LOGIN_URL);
            sendArgs.SetHttpClient(steamSession.HttpClient!);
            await SendAsync<string>(sendArgs);
        }
        return steamSession.Cookies?["sessionid"].Value!;
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
        return SystemTextJsonSerializer.Serialize(offer, options: new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    #endregion

    #region Confirmation 交易确认

    /// <summary>
    /// 生成确认参数集合
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="steamSession"></param>
    /// <returns></returns>
    private Dictionary<string, string> CreateConfirmationParams(string tag, SteamSession steamSession)
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
                { "tag", tag }
            };
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

    private static string CreateTimeHash(string identitySecret, string tag, long timestamp)
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

    #endregion
}
