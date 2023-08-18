using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace BD.SteamClient;

public class SteamMarketService : HttpClientUseCookiesWithDynamicProxyServiceImpl, ISteamMarketService
{
    public SteamMarketService(IServiceProvider? s, ILogger<SteamMarketService> logger) : base(s, logger)
    {
    }

    public async Task<MarketItemPriceOverviewResponse> GetMarketItemPriceOverview(string appId, string marketHashName, int currency = 1)
    {
        var url = $"{STEAM_COMMUNITY_URL}/market/priceoverview/?appid={appId}&currency={currency}&market_hash_name={marketHashName}";

        var resp = await client.GetAsync(url);

        return await resp.Content.ReadFromJsonAsync<MarketItemPriceOverviewResponse>();
    }

    public async Task<MarketItemOrdersHistogramResponse> GetMarketItemOrdersHistogram(long marketItemNameId, string country = "CN", int currency = 23, string language = "schinese")
    {
        string url = $"{STEAM_COMMUNITY_URL}/market/itemordershistogram?country={country}&language={language}&currency={currency}&item_nameid={marketItemNameId}";

        var resp = await client.GetAsync(url);

        return await resp.Content.ReadFromJsonAsync<MarketItemOrdersHistogramResponse>();
    }

    public async Task<SellItemToMarketResponse> SellItemToMarket(SteamLoginState loginState, string appId, string contextId, long assetId, int amount, int price)
    {
        string requestUrl = $"{STEAM_COMMUNITY_URL}/market/sellitem/";

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

        string referer = $"{STEAM_COMMUNITY_URL}/profiles/{loginState.SteamId}/inventory?modal=1&market=1";

        HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        requestMsg.Headers.Add("Referer", referer);

        requestMsg.Content = new FormUrlEncodedContent(data);

        SetDefaultCookie(loginState);
        //cookieContainer.Add(loginState.Cookies!);
        cookieContainer.Add(new Cookie()
        {
            Name = "sessionid",
            Value = loginState.SeesionId,
            Domain = new Uri(STEAM_COMMUNITY_URL).Host,
            Secure = true,
            Path = "/"
        });

        var resp = await client.SendAsync(requestMsg);

        return await resp.Content.ReadFromJsonAsync<SellItemToMarketResponse>();
    }

    public async IAsyncEnumerable<MarketTradingHisotryRenderItem> GetMarketTradingHistory(SteamLoginState loginState, int start = 0, int count = 100)
    {
        string requestUrl = string.Format("{0}/market/myhistory/render/?query=&start={1}&count={2}",
           STEAM_COMMUNITY_URL,
           start,
           count);

        SetDefaultCookie(loginState);

        // cookieContainer.Add(loginState.Cookies!);
        // cookieContainer.Add(new Cookie()
        // {
        //     Name = "Steam_Language",
        //     Value = loginState.Language,
        //     Domain = new Uri(STEAM_COMMUNITY_URL).Host,
        //     Secure = true,
        //     Path = "/",
        // });

        var resp = await client.GetAsync(requestUrl);

        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<MarketTradingHisotryRenderPageResponse>();

        if (result == null || !result.Success)
            yield break;

        IBrowsingContext browsingContext = BrowsingContext.New();

        IHtmlParser? htmlParser = browsingContext.GetService<IHtmlParser>();

        if (htmlParser == null)
            throw new ArgumentException(nameof(htmlParser));

        IDocument document = await htmlParser.ParseDocumentAsync(result.ResultsHtml);

        var rowsElement = document.QuerySelectorAll("div[id^='history_row_']");

        if (rowsElement == null || !rowsElement.Any())
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

            MarketTradingHisotryRenderItem renderItem = default;

            MarketTradingHisotryRowType rowType = rowId[(rowId.LastIndexOf('_') + 1)..] switch
            {
                "1" => MarketTradingHisotryRowType.Publish,
                "2" => MarketTradingHisotryRowType.UnPublish,
                _ => MarketTradingHisotryRowType.Normal,
            };
            renderItem.HistoryRow = rowId;
            renderItem.RowType = rowType;
            renderItem.MarketItemImgUrl = rowElement.QuerySelector($"#{rowId}_image")?.GetAttribute("src") ?? string.Empty;
            renderItem.ListingPrice = rowElement.QuerySelector("span.market_listing_price")?.TextContent?.Trim();

            var tradingPartnerElement = rowElement.QuerySelector("div.market_listing_whoactedwith");
            if (tradingPartnerElement != null)
            {
                // 普通行才有交易对象,事件没有交易对象信息
                if (rowType == MarketTradingHisotryRowType.Normal)
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
                    renderItem.TradingDate = ParseDateTimeText(datesElement[0].TextContent);
                    renderItem.ListingDate = ParseDateTimeText(datesElement[1].TextContent) ?? default;
                }
                else if (datesElement.Length == 1)
                {
                    renderItem.ListingDate = ParseDateTimeText(datesElement[0].TextContent) ?? default;
                }
            }

            renderItem.MarketItemName = rowElement.QuerySelector($"#{rowId}_name")?.TextContent ?? string.Empty;
            renderItem.GameName = rowElement.QuerySelector("span.market_listing_game_name")?.TextContent ?? string.Empty;

            yield return renderItem;
        }
    }

    public async Task<MarketListings> GetMarketListing(SteamLoginState loginState)
    {
        const string activeListingRowIdPrefix = "mylisting_";
        const string buyorderRowIdPrefix = "mybuyorder_";

        string requestUrl = $"{STEAM_COMMUNITY_URL}/market/";

        SetDefaultCookie(loginState);

        var resp = await client.GetAsync(requestUrl);

        resp.EnsureSuccessStatusCode();

        var html = await resp.Content.ReadAsStringAsync();

        var activeListings = HtmlParseHelper.ParseSimpleTable(html,
         tableSelector: "#tabContentsMyActiveMarketListingsRows",
         rowSelector: $"div[id^='{activeListingRowIdPrefix}']",
         ParseActiveListingRow);

        var buyorders = HtmlParseHelper.ParseSimpleTable(html,
         tableSelector: "#tabContentsMyListings > div:last-child",
         rowSelector: $"div[id^='{buyorderRowIdPrefix}']",
         ParseBuyorderRow);

        return new MarketListings
        {
            ActiveListings = activeListings.ToBlockingEnumerable(),
            Buyorders = buyorders.ToBlockingEnumerable(),
        };

        // 解析正在出售的物品行
        ValueTask<MarketListings.ActiveListingItem> ParseActiveListingRow(IElement rowElement)
        {
            string rowId = rowElement.Id?.Trim() ?? string.Empty;

            MarketListings.ActiveListingItem item = default;

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

            MarketListings.BuyorderItem item = default;

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

    private DateTime? ParseDateTimeText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        string? l = cookieContainer.GetCookieValue(new Uri(STEAM_COMMUNITY_URL), "Steam_Language");

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

    private void SetDefaultCookie(SteamLoginState? loginState = null)
    {
        if (loginState != null && loginState.Cookies != null)
        {
            cookieContainer.Add(loginState.Cookies);
        }

        Uri uri = new(STEAM_COMMUNITY_URL);

        cookieContainer.Add(new Cookie()
        {
            Name = "Steam_Language",
            Value = "schinese",
            Domain = uri.Host,
            Secure = true,
            Path = "/",
        });

        cookieContainer.Add(new Cookie()
        {
            Name = "timezoneOffset",
            Value = Uri.EscapeDataString($"{TimeSpan.FromHours(8).TotalSeconds},0"),
            Domain = uri.Host
        });
    }
}
