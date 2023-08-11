namespace BD.SteamClient.Services.Implementation;

using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

public class SteamIdleCardServiceImpl : ISteamIdleCardService
{
    public SteamIdleCardServiceImpl()
    {
    }

    public async Task<IEnumerable<Badge>> GetBadgesAsync(SteamSession steamSession)
    {
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
        string lastpage = pages.Last().ToString().Replace("?p=", "");
        pagesCount = Convert.ToInt32(lastpage);

        Func<string, Task<string>> cardpage_func = async (app_id) =>
        {
            return await steamSession.HttpClient.GetStringAsync(STEAM_GAMECARDS_URL.Format(steamSession.SteamId, app_id));
        };
        var badges = new List<Badge>();
        await FetchBadgesOnPage(document, badges, cardpage_func);

        // 获取其他页的徽章数据
        for (var i = 2; i <= pagesCount; i++)
        {

            badges_url = STEAM_BADGES_URL.Format(steamSession.SteamId, i);
            page = await steamSession.HttpClient.GetStringAsync(badges_url);

            document = parser.ParseDocument(page);

            await FetchBadgesOnPage(document, badges, cardpage_func);
        }

        return badges;
    }

    #region Private
    private async Task FetchBadgesOnPage(IHtmlDocument document, List<Badge> badges, Func<string, Task<string>> func)
    {
        var parser = new HtmlParser();
        IHtmlDocument? card_document = null;
        var card_page = string.Empty;
        foreach (var badge in document.QuerySelectorAll("div.badge_row.is_link"))
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

            var cardNode = badge.QuerySelector("span.progress_info_bold");
            var remaining_cards = cardNode == null ? string.Empty : Regex.Match(cardNode.TextContent, @"[0-9]+").Value;

            var badge_item = new Badge();
            badge_item.AppId = int.TryParse(appid, out var after_appid) ? after_appid : 0;
            badge_item.AppName = name;
            badge_item.BadgeName = badgeName;
            badge_item.MinutesPlayed = double.TryParse(hours, out var after_hours) ? Math.Round(after_hours * 60D) : 0D;
            badge_item.CardsRemaining = int.TryParse(remaining_cards, out var after_remaining_cards) ? after_remaining_cards : 0;

            card_page = await func(appid);
            card_document = parser.ParseDocument(card_page);
            var cards = FetchCardsOnPage(card_document);
            badge_item.Cards = cards.ToList();

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
