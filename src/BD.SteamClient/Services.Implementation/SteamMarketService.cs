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

        cookieContainer.Add(loginState.Cookies!);
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

        cookieContainer.Add(loginState.Cookies!);
        cookieContainer.Add(new Cookie()
        {
            Name = "Steam_Language",
            Value = loginState.Language,
            Domain = new Uri(STEAM_COMMUNITY_URL).Host,
            Secure = true,
            Path = "/",
        });

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

}
