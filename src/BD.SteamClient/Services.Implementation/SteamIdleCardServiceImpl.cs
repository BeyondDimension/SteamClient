namespace BD.SteamClient.Services.Implementation;

using AngleSharp.Common;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BD.SteamClient.Models.Idle;
using Nito.Comparers.Linq;
using System.Linq;

public class SteamIdleCardServiceImpl : HttpClientUseCookiesWithDynamicProxyServiceImpl, ISteamIdleCardService
{
    private readonly ISteamSessionService _sessionService;

    public SteamIdleCardServiceImpl(
        IServiceProvider s,
        ILogger<SteamIdleCardServiceImpl> logger) : base(
            s, logger)
    {
        _sessionService = s.GetRequiredService<ISteamSessionService>();
    }

    public SteamIdleCardServiceImpl(
        IServiceProvider s,
        Func<CookieContainer, HttpMessageHandler> func) : base(func, s.GetRequiredService<ILogger<SteamIdleCardServiceImpl>>())
    {
        _sessionService = s.GetRequiredService<ISteamSessionService>();
    }

    #region Public
    public async Task<(UserIdleInfo idleInfo, IEnumerable<Badge> badges)> GetBadgesAsync(string steam_id, bool need_price = false, string currency = "CNY")
    {
        var steamSession = _sessionService.RentSession(steam_id);
        if (steamSession == null)
            throw new Exception($"Unable to find session for {steam_id}, pelese login first");

        var badges_url = STEAM_BADGES_URL.Format(steamSession.SteamId, 1);
        var pages = new List<string>() { "?p=1" };
        var parser = new HtmlParser();
        int pagesCount = 1;

        var page = await steamSession.HttpClient.GetStringAsync(badges_url);
        var document = parser.ParseDocument(page);

        var pageNodes = document.All.Where(x => x.ClassName == "pagelink");
        if (pageNodes != null)
        {
            pages.AddRange(pageNodes.Select(p => p.Attributes["href"].Value).Distinct());
            pages = pages.Distinct().ToList();
        }

        // 读取总页数
        string lastpagestr = Regex.Match(pages.Last().ToString(), @"p=\s*(\d+)").Groups[1].Value;
        if (int.TryParse(lastpagestr, out var lastpage))
        {
            pagesCount = lastpage;
        }
        else
        {
            pagesCount = pages.Count;
        }

        Func<string, Task<string>> cardpage_func = async (app_id) =>
        {
            return await steamSession.HttpClient.GetStringAsync(STEAM_GAMECARDS_URL.Format(steamSession.SteamId, app_id));
        };
        var badges = new List<Badge>();
        await FetchBadgesOnPage(document, badges, cardpage_func, need_price);

        // 获取其他页的徽章数据
        for (var i = 2; i <= pagesCount; i++)
        {

            badges_url = STEAM_BADGES_URL.Format(steamSession.SteamId, i);
            page = await steamSession.HttpClient.GetStringAsync(badges_url);

            document = parser.ParseDocument(page);

            await FetchBadgesOnPage(document, badges, cardpage_func, need_price);
        }

        if (need_price)
        {
            var avg_prices = (await GetAppCradsAvgPrice(badges.Select(s => s.AppId).ToArray(), currency)).ToDictionary(x => x.AppId, x => x);
            foreach (var badge in badges)
            {
                if (avg_prices.TryGetValue(badge.AppId, out var avg))
                {
                    badge.RegularAvgPrice = avg.Regular;
                    badge.FoilAvgPrice = avg.Foil;
                }
                if (badge.Cards != null && badge.Cards.Any())
                {
                    var card_prices = (await GetCardsMarketPrice(badge.AppId, currency)).ToDictionary(x => x.CardName, x => x.Price);
                    if (card_prices.Any())
                    {
                        foreach (var card in badge.Cards)
                        {
                            var key_cardname = card.Name.Contains("Foil") || card.Name.Contains("闪亮") ? $"{card.Name} (Foil)" : card.Name;
                            if (card_prices.TryGetValue(key_cardname, out var price))
                                card.Price = price;
                        }
                    }
                }
            }
        }

        var userIdle = new UserIdleInfo();
        try
        {
            userIdle.UserLevel = ushort.TryParse(document.QuerySelector(".friendPlayerLevelNum").TextContent, out var after_userLevel) ? after_userLevel : default;
            userIdle.CurrentExp = int.TryParse(Regex.Match(document.QuerySelector(".profile_xp_block_xp").TextContent, @"\d{1,3}(,\d{3})*").Value, NumberStyles.Number, CultureInfo.CurrentCulture, out var after_currentExp) ? after_currentExp : default;

            var matchs = Regex.Matches(document.QuerySelector(".profile_xp_block_remaining").TextContent ?? string.Empty, @"\d{1,3}(,\d{3})*");
            if (matchs.Count >= 2)
                userIdle.UpExp = int.TryParse(matchs[1].Value, out var after_upExp) ? after_upExp : default;

            userIdle.NextLevelExpPercentage = short.TryParse(Regex.Match(document.QuerySelector(".profile_xp_block_remaining_bar_progress").OuterHtml, @"width:\s*(\d+)%").Groups[1].Value, out var nextLevelExpPercentage) ? nextLevelExpPercentage : default;

        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }

        return (userIdle, badges);
    }

    public async Task<IEnumerable<AppCardsAvgPrice>> GetAppCradsAvgPrice(uint[] appIds, string currency)
    {
        var url = STEAM_IDLE_APPCARDS_AVG.Format(string.Join(",", appIds), currency);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request);
        var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        if (document.RootElement.TryGetProperty("result", out var result)
            && result.ToString() == "success"
            && document.RootElement.GetProperty("data").ValueKind == JsonValueKind.Object)
        {
            var avgs = new List<AppCardsAvgPrice>();
            foreach (var item in document.RootElement.GetProperty("data").EnumerateObject())
            {
                var avg = new AppCardsAvgPrice
                {
                    AppId = uint.Parse(item.Name),
                    Regular = item.Value.GetProperty("regular").GetDecimal(),
                    Foil = item.Value.GetProperty("foil").GetDecimal()
                };
                avgs.Add(avg);
            }
            return avgs;
        }
        return Enumerable.Empty<AppCardsAvgPrice>();
    }

    public async Task<IEnumerable<CardsMarketPrice>> GetCardsMarketPrice(uint appId, string currency)
    {
        var url = STEAM_IDLE_APPCARDS_MARKETPRICE.Format(appId, currency);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request);
        var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        if (document.RootElement.TryGetProperty("result", out var result)
            && result.ToString() == "success"
            && document.RootElement.GetProperty("data").ValueKind == JsonValueKind.Object)
        {
            var cardPrices = new List<CardsMarketPrice>();
            foreach (var item in document.RootElement.GetProperty("data").EnumerateObject())
            {
                var cardPrice = new CardsMarketPrice();
                cardPrice.CardName = item.Name;
                cardPrice.IsFoil = cardPrice.CardName.Contains("(Foil)");
                cardPrice.MarketUrl = item.Value.GetProperty("url").ToString();
                cardPrice.Price = item.Value.GetProperty("price").GetDecimal();
                cardPrices.Add(cardPrice);
            }
            return cardPrices;
        }
        return Enumerable.Empty<CardsMarketPrice>();
    }
    #endregion

    #region Private
    private async Task FetchBadgesOnPage(IHtmlDocument document, List<Badge> badges, Func<string, Task<string>> func, bool need_price)
    {
        var parser = new HtmlParser();
        var badges_rows = document.QuerySelectorAll("div.badge_row.is_link");
        foreach (var badge in badges_rows)
        {
            var appIdNode = badge.QuerySelector("a.badge_row_overlay")?.Attributes["href"]?.Value ?? "";
            var appid = Regex.Match(appIdNode, @"gamecards/(\d+)/").Groups[1].Value;

            if (string.IsNullOrWhiteSpace(appid) || appid == "368020" || appid == "335590" || appIdNode.Contains("border=1"))
            {
                continue;
            }

            var hoursNode = badge.QuerySelector("div.badge_title_stats_playtime");
            var hours = hoursNode == null ? string.Empty : Regex.Match(hoursNode.TextContent, @"[0-9\.,]+").Value;

            var nameNode = badge.QuerySelector("div.badge_title");
            var name = WebUtility.HtmlDecode(nameNode?.FirstChild?.TextContent)?.Trim() ?? "";

            var badgeNameNode = badge.QuerySelector("div.badge_info_title") ?? badge.QuerySelector("div.badge_empty_name");
            var badgeName = WebUtility.HtmlDecode(badgeNameNode?.FirstChild?.TextContent)?.Trim() ?? "";

            var badgeInfo = badge.QuerySelector("div.badge_info");
            byte level = 0; short exp = 0;
            var badgeImageUrl = string.Empty;
            if (badgeInfo != null)
            {
                badgeImageUrl = badgeInfo.QuerySelector("img.badge_icon")?.Attributes["src"]?.Value ?? string.Empty;
                var level_expCards = badgeInfo.QuerySelector("div.badge_info_description")?.Children[1];
                var matchs = Regex.Matches(level_expCards?.TextContent ?? string.Empty, @"[0-9]+");
                if (matchs.Count == 2)
                {
                    level = byte.Parse(matchs[0].Value);
                    exp = short.Parse(matchs[1].Value);
                }
            }

            var cardRemainingNode = badge.QuerySelector("span.progress_info_bold");
            var remaining_cards = cardRemainingNode == null ? string.Empty : Regex.Match(cardRemainingNode.TextContent, @"[0-9]+").Value;

            var cardGathering = badge.QuerySelector("div.badge_progress_info");
            int collected = 0, gathering = 0;
            if (cardGathering != null)
            {
                var matchs = Regex.Matches(cardGathering.TextContent, @"[0-9]+");

                if (matchs.Count == 2)
                {
                    collected = int.TryParse(matchs[0].Value, out var after_collected) ? after_collected : default;
                    gathering = int.TryParse(matchs[1].Value, out var after_gathering) ? after_gathering : default;
                }
                else if (matchs.Count == 3)
                {
                    collected = int.TryParse(matchs[2].Value, out var after_collected) ? after_collected : default;
                    gathering = int.TryParse(matchs[1].Value, out var after_gathering) ? after_gathering : default;
                }
            }

            var badge_item = new Badge
            {
                AppId = uint.TryParse(appid, out var after_appid) ? after_appid : 0,
                AppName = name,
                BadgeName = badgeName,
                BadgeImageUrl = badgeImageUrl,
                BadgeLevel = level,
                BadgeCurrentExp = exp,
                MinutesPlayed = double.TryParse(hours, out var after_hours) ? Math.Round(after_hours * 60D) : 0D,
                CardsRemaining = int.TryParse(remaining_cards, out var after_remaining_cards) ? after_remaining_cards : 0,
                CardsCollected = collected,
                CardsGathering = gathering
            };

            if (need_price)
            {
                var card_page = await func(appid);
                var card_document = parser.ParseDocument(card_page);
                var cards = FetchCardsOnPage(card_document);
                badge_item.Cards = cards.ToList();
            }

            badges.Add(badge_item);
        }
    }

    private IEnumerable<SteamCard> FetchCardsOnPage(IHtmlDocument document)
    {
        var cards = new List<SteamCard>();
        foreach (var owned in document.QuerySelectorAll("div.badge_card_set_card.owned"))
        {
            var card = GetCard(owned);
            card.IsCollected = true;
            cards.Add(card);
        }

        foreach (var unowned in document.QuerySelectorAll("div.badge_card_set_card.unowned"))
        {
            var card = GetCard(unowned);
            card.IsCollected = false;
            cards.Add(card);
        }
        return cards;

        SteamCard GetCard(IElement element)
        {
            var card = new SteamCard();
            card.ImageUrl = element.QuerySelector("img.gamecard")?.Attributes["src"]?.Value ?? "";
            var name = element.QuerySelector("div.badge_card_set_text.badge_card_set_text")?.ChildNodes.Where(x => x.NodeType == NodeType.Text).Select(s => s.TextContent);
            card.Name = name != null ? WebUtility.HtmlDecode(string.Join("", name)).Trim() : "";
            return card;
        }
    }
    #endregion
}
