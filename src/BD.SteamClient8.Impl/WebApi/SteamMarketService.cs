namespace BD.SteamClient8.Impl.WebApi;

#pragma warning disable SA1600 // Elements should be documented

public class SteamMarketService : WebApiClientFactoryService, ISteamMarketService
{
    const string TAG = "SteamMarketWebApiS";

    /// <summary>
    /// HttpClient 标识名称
    /// </summary>
    protected override string ClientName => TAG;

    protected override SystemTextJsonSerializerContext? JsonSerializerContext => DefaultJsonSerializerContext_.Default;

    public SteamMarketService(IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : base(
            loggerFactory.CreateLogger(TAG),
            serviceProvider)
    {
    }

    /// <summary>
    /// 重试间隔
    /// </summary>
    static readonly IEnumerable<TimeSpan> sleepDurations = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), };

    public async Task<ApiRspImpl<MarketItemPriceOverviewResponse>> GetMarketItemPriceOverview(string appId, string marketHashName, int currency = 1)
    {
        var url = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/market/priceoverview/?appid={appId}&currency={currency}&market_hash_name={marketHashName}";

        using var sendArgs = new WebApiClientSendArgs(url);
        try
        {
            sendArgs.SetHttpClient(CreateClient());
            return (await SendAsync<MarketItemPriceOverviewResponse>(sendArgs))!;
        }
        catch (Exception ex)
        {
            return OnErrorReApiRspBase<ApiRspImpl<MarketItemPriceOverviewResponse>>(ex, sendArgs);
        }
    }

    public async Task<ApiRspImpl<MarketItemOrdersHistogramResponse>> GetMarketItemOrdersHistogram(long marketItemNameId, string country = "CN", int currency = 23, string language = "schinese")
    {
        string url = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/market/itemordershistogram?country={country}&language={language}&currency={currency}&item_nameid={marketItemNameId}";

        using var sendArgs = new WebApiClientSendArgs(url);
        try
        {
            sendArgs.SetHttpClient(CreateClient());
            var str = await SendAsync<string>(sendArgs);
            return (await SendAsync<MarketItemOrdersHistogramResponse>(sendArgs))!;
        }
        catch (Exception ex)
        {
            return OnErrorReApiRspBase<ApiRspImpl<MarketItemOrdersHistogramResponse>>(ex, sendArgs);
        }
    }

    public async Task<ApiRspImpl<SellItemToMarketResponse>> SellItemToMarket(SteamLoginState loginState, string appId, string contextId, long assetId, int amount, int price)
    {
        string requestUrl = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/market/sellitem/";

        if (string.IsNullOrEmpty(loginState.SeesionId))
            throw new InvalidOperationException("获取会话信息失败,请稍后重试!");

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            { "sessionid", loginState.SeesionId },
            { "appid", appId },
            { "contextid", contextId },
            { "assetid", assetId.ToString() },
            { "amount", amount.ToString() },
            { "price", price.ToString() },
        };

        string referer = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/profiles/{loginState.SteamId}/inventory?modal=1&market=1";

        var cookieCollection = new CookieCollection
        {
            loginState.Cookies!,
            new Cookie()
            {
                Name = "sessionid",
                Value = loginState.SeesionId,
                Domain = new Uri(SteamApiUrls.STEAM_COMMUNITY_URL).Host,
                Secure = true,
                Path = "/"
            }
        };
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(cookieCollection);
        using var sendArgs = new WebApiClientSendArgs(requestUrl)
        {
            Method = HttpMethod.Post,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
            ConfigureRequestMessage = (req, args, token) =>
            {
                req.Headers.Add("Referer", referer);
                req.Content = new FormUrlEncodedContent(data);
            },
        };
        sendArgs.SetHttpClient(client);

        var resp = await SendAsync<SellItemToMarketResponse>(sendArgs);
        return resp!;
    }

    public async IAsyncEnumerable<MarketTradingHistoryRenderItem> GetMarketTradingHistory(SteamLoginState loginState, int start = 0, int count = 100)
    {
        string requestUrl = string.Format("{0}/market/myhistory/render/?query=&start={1}&count={2}",
           SteamApiUrls.STEAM_COMMUNITY_URL,
           start,
           count);

        var cookieCollection = new CookieCollection
        {
            loginState.Cookies!,
            new Cookie()
            {
                Name = "Steam_Language",
                Value = loginState.Language,
                Domain = new Uri(SteamApiUrls.STEAM_COMMUNITY_URL).Host,
                Secure = true,
                Path = "/",
            }
        };
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(cookieCollection);
        using var sendArgs = new WebApiClientSendArgs(requestUrl)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson
        };
        sendArgs.SetHttpClient(client);

        var result = await SendAsync<MarketTradingHistoryRenderPageResponse>(sendArgs);

        if (result == null || !result.Success)
            yield break;

        var l = container.GetCookieValue(new Uri(SteamApiUrls.STEAM_COMMUNITY_URL), "Steam_Language");

        IBrowsingContext browsingContext = BrowsingContext.New();

        IHtmlParser? htmlParser = browsingContext.GetService<IHtmlParser>();

        if (htmlParser == null)
            throw new ArgumentException(nameof(htmlParser));

        IDocument document = await htmlParser.ParseDocumentAsync(result.ResultsHtml);

        var rowsElement = document.QuerySelectorAll("div[id^='history_row_']");

        if (rowsElement == null || rowsElement.Length <= 0)
            yield break;

        foreach (var rowElement in rowsElement)
        {
            if (rowElement == null)
                continue;

            string? rowId = rowElement.Id!.Trim();

            if (string.IsNullOrEmpty(rowId))
            {
                continue;
            }

            MarketTradingHistoryRenderItem renderItem = new();

            MarketTradingHistoryRowType rowType = rowId[(rowId.LastIndexOf('_') + 1)..] switch
            {
                "1" => MarketTradingHistoryRowType.Publish,
                "2" => MarketTradingHistoryRowType.UnPublish,
                _ => MarketTradingHistoryRowType.Normal,
            };
            renderItem.HistoryRow = rowId;
            renderItem.RowType = rowType;
            renderItem.MarketItemImgUrl = rowElement.QuerySelector($"#{rowId}_image")?.GetAttribute("src") ?? string.Empty;
            renderItem.ListingPrice = rowElement.QuerySelector("span.market_listing_price")?.TextContent?.Trim();

            var tradingPartnerElement = rowElement.QuerySelector("div.market_listing_whoactedwith");
            if (tradingPartnerElement != null)
            {
                // 普通行才有交易对象,事件没有交易对象信息
                if (rowType == MarketTradingHistoryRowType.Normal)
                {
                    renderItem.TradingPartnerAvatar = tradingPartnerElement.QuerySelector("img")?.GetAttribute("src");
                    renderItem.TradingPartnerLabel = tradingPartnerElement.QuerySelector("div.market_listing_whoactedwith_name_block")?.TextContent.Trim();
                }
                else
                {
                    renderItem.TradingPartnerLabel = tradingPartnerElement.TextContent?.Trim();
                }
            }

            var datesElement = rowElement.QuerySelectorAll("div.market_listing_listed_date");
            if (datesElement != null)
            {
                if (datesElement.Length == 2)
                {
                    renderItem.TradingDate = ParseDateTimeText(datesElement[0].TextContent, l);
                    renderItem.ListingDate = ParseDateTimeText(datesElement[1].TextContent, l) ?? default;
                }
                else if (datesElement.Length == 1)
                {
                    renderItem.ListingDate = ParseDateTimeText(datesElement[0].TextContent, l) ?? default;
                }
            }

            renderItem.MarketItemName = rowElement.QuerySelector($"#{rowId}_name")?.TextContent ?? string.Empty;
            renderItem.GameName = rowElement.QuerySelector("span.market_listing_game_name")?.TextContent ?? string.Empty;

            yield return renderItem;
        }
    }

    public async Task<ApiRspImpl<MarketListings>> GetMarketListing(SteamLoginState loginState)
    {
        const string activeListingRowIdPrefix = "mylisting_";
        const string buyorderRowIdPrefix = "mybuyorder_";

        string requestUrl = $"{SteamApiUrls.STEAM_COMMUNITY_URL}/market/";

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies!);
        using var sendArgs = new WebApiClientSendArgs(requestUrl)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
        };
        sendArgs.SetHttpClient(client);

        var respStream = await SendAsync<Stream>(sendArgs);

        if (respStream == null)
            return ApiRspHelper.Fail<MarketListings>($"{requestUrl} request error")!;

        var activeListings = HtmlParseHelper.ParseSimpleTable(respStream,
         tableSelector: "#tabContentsMyActiveMarketListingsRows",
         rowSelector: $"div[id^='{activeListingRowIdPrefix}']",
         ParseActiveListingRow);

        var buyorders = HtmlParseHelper.ParseSimpleTable(respStream,
         tableSelector: "#tabContentsMyListings > div:last-child",
         rowSelector: $"div[id^='{buyorderRowIdPrefix}']",
         ParseBuyorderRow);

        return new MarketListings
        {
            ActiveListings = activeListings.ToBlockingEnumerable(),
            Buyorders = buyorders.ToBlockingEnumerable(),
        }!;

        // 解析正在出售的物品行
        ValueTask<MarketListings.ActiveListingItem> ParseActiveListingRow(IElement rowElement)
        {
            string rowId = rowElement.Id?.Trim() ?? string.Empty;

            MarketListings.ActiveListingItem item = new();

            item.Id = rowId.Replace(activeListingRowIdPrefix, string.Empty).ToString();
            item.ImgUrl = rowElement.QuerySelector($"#{rowId}_image")?.GetAttribute("src") ?? string.Empty;

            var priceElement = rowElement.QuerySelector("div.market_listing_my_price")
                ?.FirstElementChild
                ?.FirstElementChild
                ?.FirstElementChild
                ?.Children;

            if (priceElement != null)
            {
                item.Payment = priceElement
                    .First().TextContent?.Trim() ?? string.Empty;

                item.Received = priceElement
                    .LastOrDefault()?.TextContent?.Trim()?.TrimStart('(')?.TrimEnd(')') ?? string.Empty;
            }

            item.ListingDate = rowElement.QuerySelector("div.market_listing_listed_date")?.TextContent?.Trim() ?? string.Empty;

            var listingItemElement = rowElement.QuerySelector("div.market_listing_item_name_block");
            if (listingItemElement != null)
            {
                var linkElement = listingItemElement.QuerySelector($"#{rowId}_name")?.FirstElementChild;

                item.ItemName = linkElement?.TextContent?.Trim() ?? string.Empty;
                item.ItemMarketUrl = linkElement?.GetAttribute("href") ?? string.Empty;
                item.GameName = rowElement.QuerySelector("span.market_listing_game_name")?.TextContent?.Trim() ?? string.Empty;
            }

            return ValueTask.FromResult(item);
        }

        ValueTask<MarketListings.BuyorderItem> ParseBuyorderRow(IElement rowElement)
        {
            // 不知道是不是steam bug,这里的内层id前缀是 mbuyorder_而不是 mybuyorder_
            const string rowProfix = "mbuyorder_";

            string rowId = rowElement.Id?.Trim() ?? string.Empty;

            MarketListings.BuyorderItem item = new();

            item.Id = rowId.Replace(buyorderRowIdPrefix, string.Empty).ToString();
            item.ImgUrl = rowElement.QuerySelector($"#{rowId}_image")?.GetAttribute("src") ?? string.Empty;

            var priceElement = rowElement.QuerySelector("div.market_listing_my_price")
                ?.FirstElementChild
                ?.FirstElementChild
                ?.FirstElementChild;

            if (priceElement != null)
            {
                var parent = priceElement.ParentElement;
                priceElement.RemoveFromParent();

                item.Price = parent?.TextContent?.Trim() ?? string.Empty;
            }

            string amountText = rowElement
            .QuerySelector("div.market_listing_buyorder_qty > span.market_table_value > span.market_listing_price")
            ?.TextContent?.Trim() ?? string.Empty;

            if (int.TryParse(amountText, out int parsedAmount))
                item.Amount = parsedAmount;

            var listingItemElement = rowElement.QuerySelector("div.market_listing_item_name_block");
            if (listingItemElement != null)
            {
                var linkElement = listingItemElement.QuerySelector($"#{rowProfix}{item.Id}_name")?.FirstElementChild;

                item.ItemName = linkElement?.TextContent?.Trim() ?? string.Empty;
                item.ItemMarketUrl = linkElement?.GetAttribute("href") ?? string.Empty;
                item.GameName = rowElement.QuerySelector("span.market_listing_game_name")?.TextContent?.Trim() ?? string.Empty;
            }

            return ValueTask.FromResult(item);
        }
    }

    private DateTime? ParseDateTimeText(string? text, string? l)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (l == null)
            return null;

        text = text.Trim().Replace(" ", string.Empty);

        return l switch
        {
            "english" => DateTime.TryParse(text, out var parsedDate) ? parsedDate : null,
            "schinese" => DateTime.TryParse(text, CultureInfo.GetCultureInfo("zh-CN"), out var parsedDate) ? parsedDate : null,
            _ => null
        };
    }
}
