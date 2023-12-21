namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamTradeServiceImpl"/> 单元测试
/// </summary>
sealed class SteamTradeServiceTest : ServiceTestBase
{
    SteamLoginState steamLoginState = null!;
    ISteamTradeService steamTradeService = null!;
    ISteamAccountService steamAccountService = null!;
    ISteamSessionService steamSessionService = null!;
    ISteamAuthenticatorService steamAuthenticatorService = null!;
    IConfiguration configuration = null!;

    string ApiKey = string.Empty;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamAccountService();
        services.AddSteamAuthenticatorService();
        services.AddSteamTradeService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamTradeService = GetRequiredService<ISteamTradeService>();
        steamAccountService = GetRequiredService<ISteamAccountService>();
        steamAuthenticatorService = GetRequiredService<ISteamAuthenticatorService>();
        steamSessionService = GetRequiredService<ISteamSessionService>();
        configuration = GetRequiredService<IConfiguration>();

        steamLoginState = await GetSteamLoginStateAsync(configuration, steamAccountService, GetRequiredService<ISteamSessionService>());

        var steamAuthenticator = await GetSteamAuthenticatorAsync();
        var identitySecret = configuration["identitySecret"];
        long serverTimeDiff;
        // 本地令牌获取相关信息
        if (identitySecret is null)
        {
            var steamData = SystemTextJsonSerializer.Deserialize<SteamConvertSteamDataJsonStruct>(steamAuthenticator.ThrowIsNull().SteamData!);
            identitySecret = steamData!.IdentitySecret;
            serverTimeDiff = steamAuthenticator.ServerTimeDiff;
        }
        else // 用户机密获取 identitySecret 以及临时请求服务器时间
        {
            var serverTime = (await steamAuthenticatorService.TwoFAQueryTime())?.Content?.Response?.ServerTime.ThrowIsNull();
            serverTimeDiff = (long.Parse(serverTime!) * 1000L) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        var session = steamSessionService.RentSession(steamLoginState.SteamId.ToString());
        if (session is not null)
        {
            session.ServerTimeDiff = serverTimeDiff;
            session.IdentitySecret = identitySecret;
            session.APIKey = ApiKey = (await steamAccountService.GetApiKey(steamLoginState)).Content ?? (await steamAccountService.RegisterApiKey(steamLoginState)).Content.ThrowIsNull();
            steamSessionService.AddOrSetSession(session);
        }
    }

    /// <summary>
    /// 获取历史报价信息
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetTradeHistory_Test()
    {
        var tradeOffersSummary = await steamTradeService.GetTradeOffersSummaryAsync(ApiKey);

        Assert.That(tradeOffersSummary, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(tradeOffersSummary.IsSuccess, Is.True);
            Assert.That(tradeOffersSummary.Content, Is.Not.Null);
            Assert.That(tradeOffersSummary.Content?.HistoricalReceivedCount, !Is.EqualTo(0));
        });

        var tradeHistoryDetail = await steamTradeService.GetTradeHistory(ApiKey);

        Assert.That(tradeHistoryDetail, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(tradeHistoryDetail.IsSuccess, Is.True);
            Assert.That(tradeHistoryDetail.Content, Is.Not.Null);
            Assert.That(tradeHistoryDetail.Content?.Trades, !Is.EqualTo(0));
        });
    }

    /// <summary>
    /// 接受交易报价
    /// </summary>
    /// <returns></returns>
    public async Task AcceptTradeOffer_Test()
    {
        // ------------接受所有礼物形式的交易报价
        var accept_result = await steamTradeService.AcceptAllGiftTradeOfferAsync(steamLoginState.SteamId.ToString());
        Assert.That(accept_result, Is.Not.Null);
        Assert.That(accept_result.Content, Is.True);

        // ------------开启后台任务 定时接收礼物报价
        var startTask = steamTradeService.StartTradeTask(
            steam_id: steamLoginState.SteamId.ToString(),
            interval: TimeSpan.FromMinutes(3),
            tradeTaskEnum: TradeTaskEnum.AutoAcceptGitTrade);

        Assert.That(startTask.IsSuccess, Is.True);

        // ------------指定报价接收
        var tradeOffers = await steamTradeService.GetTradeOffersAsync(ApiKey);

        Assert.Multiple(() =>
        {
            Assert.That(tradeOffers, Is.Not.Null);
            Assert.That(tradeOffers.IsSuccess, Is.True);
            Assert.That(tradeOffers.Content, Is.Not.Null);
        });
        // 过滤出活跃状态的报价进行处理
        var actives = ISteamTradeService.FilterNonActiveOffers(tradeOffers.Content)?.Response?.TradeOffersReceived;
        if (actives != null && actives.Count > 0)
        {
            // 获取当前 steam 用户的所有令牌确认消息
            var confirmations = await steamTradeService.GetConfirmations(steamLoginState.SteamId.ToString());
            Assert.Multiple(() =>
            {
                Assert.That(confirmations, Is.Not.Null);
                Assert.That(confirmations.Content, Is.Not.Null);
                Assert.That(confirmations.Content!.Count(), !Is.EqualTo(0));
            });

            // 找一条礼物报价
            var giveCountZeroTradeInfo = actives.Where(x => x.ItemsToGive != null && x.ItemsToGive.Count == 0).FirstOrDefault();
            if (giveCountZeroTradeInfo != null)
            {
                // 接受报价
                var result = await steamTradeService.AcceptTradeOfferAsync(
                    steam_id: steamLoginState.SteamId.ToString(),
                    trade_offer_id: giveCountZeroTradeInfo.TradeOfferId,
                    tradeInfo: giveCountZeroTradeInfo,
                    confirmations.Content);

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result.IsSuccess && result.Content, Is.True);
                });
            }
        }

        // ------------停止后台任务
        var stopTask = steamTradeService.StopTask(steamLoginState.SteamId.ToString(), TradeTaskEnum.AutoAcceptGitTrade);
        Assert.That(stopTask, Is.Not.Null);
        Assert.That(stopTask.IsSuccess, Is.True);
    }

    /// <summary>
    /// 发送交易报价【交易链接】
    /// </summary>
    /// <param name="trade_url"></param>
    /// <param name="app_id"></param>
    /// <param name="context_id"></param>
    /// <returns></returns>
    [Test]
    [TestCase("https://steamcommunity.com/tradeoffer/new/?partner=1551675232&token=j7yIRcwK", "730", "2")]
    public async Task SendTradeOffer_TradeUrl_Test(string trade_url, string app_id, string context_id)
    {
        var queryParams = HttpUtility.ParseQueryString(new Uri(trade_url).Query);
        var target_steam_id = new SteamIdConvert(queryParams["partner"].ThrowIsNull());

        var them_inventories = await steamAccountService.GetInventories(ulong.Parse(target_steam_id.Id64), app_id, context_id);

        Assert.Multiple(() =>
        {
            Assert.That(them_inventories, Is.Not.Null);
            Assert.That(them_inventories.IsSuccess, Is.True);
            Assert.That(them_inventories.Content != null && them_inventories.Content.Assets.ThrowIsNull().Length > 0, Is.True);
        });

        var my_times = new List<Asset>();
        // 索取的物品
        var them_items = them_inventories.Content!.Assets!
                .Select(s => new Asset
                {
                    AppId = s.AppId,
                    Amount = 1,
                    AssetId = s.AssetId,
                    ContextId = s.ContextId
                })
                .Take(1)
                .ToList();

        var send_result = await steamTradeService.SendTradeOfferWithUrlAsync(
            steam_id: steamLoginState.SteamId.ToString(),
            trade_offer_url: trade_url,
            my_items: my_times,
            them_items: them_items,
            message: "this item is so lowed, why not give it to me");

        Assert.Multiple(() =>
        {
            Assert.That(send_result, Is.Not.Null);
            Assert.That(send_result.IsSuccess && send_result.Content, Is.True);
        });
    }

    /// <summary>
    /// 发送交易报价 【好友】
    /// </summary>
    /// <param name="friend_steam_id"></param>
    /// <param name="app_id"></param>
    /// <param name="context_id"></param>
    /// <returns></returns>
    [Test]
    [TestCase("76561198425787706", "730", "2")]
    public async Task SendTradOffer_Friend_Test(string friend_steam_id, string app_id, string context_id)
    {
        var them_inventories = await steamAccountService.GetInventories(ulong.Parse(friend_steam_id), app_id, context_id);

        Assert.Multiple(() =>
        {
            Assert.That(them_inventories, Is.Not.Null);
            Assert.That(them_inventories.IsSuccess, Is.True);
            Assert.That(them_inventories.Content != null && them_inventories.Content.Assets.ThrowIsNull().Length > 0, Is.True);
        });

        var my_times = new List<Asset>();
        // 索取的物品
        var them_items = them_inventories.Content!.Assets!
                .Select(s => new Asset
                {
                    AppId = s.AppId,
                    Amount = 1,
                    AssetId = s.AssetId,
                    ContextId = s.ContextId
                })
                .Take(1)
                .ToList();

        var send_result = await steamTradeService.SendTradeOfferAsync(
            steam_id: steamLoginState.SteamId.ToString(),
            my_items: my_times,
            them_items: them_items,
            target_steam_id: friend_steam_id,
            message: "this item is so lowed, why not give it to me");

        Assert.Multiple(() =>
        {
            Assert.That(send_result, Is.Not.Null);
            Assert.That(send_result.IsSuccess && send_result.Content, Is.True);
        });
    }

    /// <summary>
    /// 拒绝接收到的报价 和 取消已发送报价
    /// </summary>
    /// <returns></returns>
    public async Task Decline_Cancel_TradeOffer_Test()
    {
        var trade_summary = await steamTradeService.GetTradeOffersSummaryAsync(ApiKey);
        Assert.Multiple(() =>
        {
            Assert.That(trade_summary, Is.Not.Null);
            Assert.That(trade_summary.IsSuccess, Is.True);
            Assert.That(trade_summary.Content, Is.Not.Null);
        });

        if (trade_summary.Content.PendingReceivedCount > 0 || trade_summary.Content.PendingSentCount > 0)
        {
            var tradeOffers = await steamTradeService.GetTradeOffersAsync(ApiKey);

            Assert.Multiple(() =>
            {
                Assert.That(tradeOffers, Is.Not.Null);
                Assert.That(tradeOffers.IsSuccess, Is.True);
                Assert.That(tradeOffers.Content, Is.Not.Null);
            });
            var fetched = ISteamTradeService.FilterNonActiveOffers(tradeOffers.Content);

            var decline_trade_offer = fetched!.Response!.TradeOffersReceived.ThrowIsNull().FirstOrDefault();
            if (decline_trade_offer != null)
            {
                var decline_result = await steamTradeService.DeclineTradeOfferAsync(steamLoginState.SteamId.ToString(), decline_trade_offer.TradeOfferId);
                Assert.That(decline_result, Is.Not.Null);
                Assert.That(decline_result.IsSuccess && decline_result.Content, Is.True);
            }

            var cancel_trade_offer = fetched!.Response!.TradeOffersSent.ThrowIsNull().FirstOrDefault();
            if (cancel_trade_offer != null)
            {
                var cancel_result = await steamTradeService.CancelTradeOfferAsync(steamLoginState.SteamId.ToString(), cancel_trade_offer.TradeOfferId);
                Assert.That(cancel_result, Is.Not.Null);
                Assert.That(cancel_result.IsSuccess && cancel_result.Content, Is.True);
            }
        }
    }
}
