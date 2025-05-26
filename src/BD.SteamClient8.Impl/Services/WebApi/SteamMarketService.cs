using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BD.Common8.Crawler.Helpers;
using BD.Common8.Helpers;
using BD.Common8.Http.ClientFactory.Models;
using BD.Common8.Http.ClientFactory.Services;
using BD.Common8.Models;
using BD.SteamClient8.Constants;
using BD.SteamClient8.Enums.WebApi.Markets;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.WebApi.Logins;
using BD.SteamClient8.Models.WebApi.Markets;
using BD.SteamClient8.Services.Abstractions.WebApi;
using Microsoft.Extensions.Logging;
using System.Extensions;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BD.SteamClient8.Services.WebApi;

/// <summary>
/// <see cref="ISteamMarketService"/> Steam 市场交易相关服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamMarketService"/> 类的新实例
/// </remarks>
/// <param name="serviceProvider"></param>
/// <param name="loggerFactory"></param>
public sealed class SteamMarketService(IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : WebApiClientFactoryService(
        loggerFactory.CreateLogger(TAG),
        serviceProvider), ISteamMarketService
{
    const string TAG = "SteamMarketWebApiS";

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

    ///// <summary>
    ///// 重试间隔
    ///// </summary>
    //static readonly IEnumerable<TimeSpan> sleepDurations = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), };

    /// <inheritdoc/>
    public async Task<ApiRspImpl<MarketItemPriceOverviewResponse>> GetMarketItemPriceOverview(string appId, string marketHashName, int currency = 1, CancellationToken cancellationToken = default)
    {
        var url = SteamApiUrls.STEAM_MARKET_ITEMPRICEOVERVIEW_GET.Format(appId, currency, marketHashName);

        using var sendArgs = new WebApiClientSendArgs(url);
        try
        {
            sendArgs.SetHttpClient(CreateClient());
            return (await SendAsync<MarketItemPriceOverviewResponse>(sendArgs, cancellationToken))!;
        }
        catch (Exception ex)
        {
            return OnErrorReApiRspBase<ApiRspImpl<MarketItemPriceOverviewResponse>>(ex, sendArgs);
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<MarketItemOrdersHistogramResponse>> GetMarketItemOrdersHistogram(long marketItemNameId, string country = "CN", int currency = 23, string language = "schinese", CancellationToken cancellationToken = default)
    {
        string url = SteamApiUrls.STEAM_MARKET_ITEMORDERHISTOGRAM_GET.Format(country, language, currency, marketItemNameId);

        using var sendArgs = new WebApiClientSendArgs(url);
        try
        {
            sendArgs.SetHttpClient(CreateClient());
            var str = await SendAsync<string>(sendArgs, cancellationToken);
            return (await SendAsync<MarketItemOrdersHistogramResponse>(sendArgs, cancellationToken))!;
        }
        catch (Exception ex)
        {
            return OnErrorReApiRspBase<ApiRspImpl<MarketItemOrdersHistogramResponse>>(ex, sendArgs);
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<SellItemToMarketResponse>> SellItemToMarket(SteamLoginState loginState, string appId, string contextId, long assetId, int amount, int price, CancellationToken cancellationToken = default)
    {
        string requestUrl = SteamApiUrls.STEAM_MARKET_SELLITEM;

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
                Path = "/",
            },
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

                return Task.CompletedTask;
            },
        };
        sendArgs.SetHttpClient(client);

        var resp = await SendAsync<SellItemToMarketResponse>(sendArgs, cancellationToken);
        return resp!;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<MarketTradingHistoryRenderItem> GetMarketTradingHistory(SteamLoginState loginState, int start = 0, int count = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string requestUrl = SteamApiUrls.STEAM_MARKET_TRADING_HISTORY_GET.Format(start, count);

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
            },
        };
        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(cookieCollection);
        using var sendArgs = new WebApiClientSendArgs(requestUrl)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
        };
        sendArgs.SetHttpClient(client);

        var result = await SendAsync<MarketTradingHistoryRenderPageResponse>(sendArgs, cancellationToken);

        if (result == null || !result.Success)
            yield break;

        var l = container.GetCookieValue(new Uri(SteamApiUrls.STEAM_COMMUNITY_URL), "Steam_Language");

        IBrowsingContext browsingContext = BrowsingContext.New();

        IHtmlParser? htmlParser = browsingContext.GetService<IHtmlParser>();

        if (htmlParser == null)
            throw new ArgumentException(nameof(htmlParser));

        IDocument document = await htmlParser.ParseDocumentAsync(result.ResultsHtml, cancellationToken);

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
                    renderItem.TradingDate = SteamMarketService.ParseDateTimeText(datesElement[0].TextContent, l);
                    renderItem.ListingDate = SteamMarketService.ParseDateTimeText(datesElement[1].TextContent, l) ?? default;
                }
                else if (datesElement.Length == 1)
                {
                    renderItem.ListingDate = SteamMarketService.ParseDateTimeText(datesElement[0].TextContent, l) ?? default;
                }
            }

            renderItem.MarketItemName = rowElement.QuerySelector($"#{rowId}_name")?.TextContent ?? string.Empty;
            renderItem.GameName = rowElement.QuerySelector("span.market_listing_game_name")?.TextContent ?? string.Empty;

            yield return renderItem;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<MarketListings>> GetMarketListing(SteamLoginState loginState, CancellationToken cancellationToken = default)
    {
        const string activeListingRowIdPrefix = "mylisting_";
        const string buyorderRowIdPrefix = "mybuyorder_";

        string requestUrl = SteamApiUrls.STEAM_MARKET;

        var client = CreateClient(loginState.Username.ThrowIsNull());
        var container = GetCookieContainer(loginState.Username);
        container.Add(loginState.Cookies!);
        using var sendArgs = new WebApiClientSendArgs(requestUrl)
        {
            Method = HttpMethod.Get,
            JsonImplType = Serializable.JsonImplType.SystemTextJson,
        };
        sendArgs.SetHttpClient(client);

        using var respStream = await SendAsync<Stream>(sendArgs, cancellationToken);

        if (respStream == null)
            return ApiRspHelper.Fail<MarketListings>($"{requestUrl} request error")!;

        using IBrowsingContext browsingContext = BrowsingContext.New();

        var htmlParser = browsingContext.GetService<IHtmlParser>();
        ArgumentNullException.ThrowIfNull(htmlParser);

        using IDocument document = await htmlParser.ParseDocumentAsync(respStream);

        var activeListings = HtmlParseHelper.ParseSimpleTable(document,
         tableSelector: "#tabContentsMyActiveMarketListingsRows",
         rowSelector: $"div[id^='{activeListingRowIdPrefix}']",
         ParseActiveListingRow);

        var buyorders = HtmlParseHelper.ParseSimpleTable(document,
         tableSelector: "#tabContentsMyListings > div:last-child",
         rowSelector: $"div[id^='{buyorderRowIdPrefix}']",
         ParseBuyorderRow);

        return new MarketListings
        {
            ActiveListings = activeListings.ToBlockingEnumerable(cancellationToken: cancellationToken),
            Buyorders = buyorders.ToBlockingEnumerable(cancellationToken: cancellationToken),
        }!;

        // 解析正在出售的物品行
        ValueTask<MarketListings.ActiveListingItem> ParseActiveListingRow(IElement rowElement)
        {
            string rowId = rowElement.Id?.Trim() ?? string.Empty;

            MarketListings.ActiveListingItem item = new()
            {
                Id = rowId.Replace(activeListingRowIdPrefix, string.Empty).ToString(),
                ImgUrl = rowElement.QuerySelector($"#{rowId}_image")?.GetAttribute("src") ?? string.Empty,
            };

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

    static DateTime? ParseDateTimeText(string? text, string? l)
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
            _ => null,
        };
    }
}
