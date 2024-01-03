namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamTradeServiceImpl"/> 单元测试
/// </summary>
sealed class SteamTradeServiceTest : ServiceTestBase
{
    protected ISteamTradeService steamTradeService = null!;
    protected ISteamAccountService steamAccountService = null!;
    protected ISteamSessionService steamSessionService = null!;
    protected ISteamAuthenticatorService steamAuthenticatorService = null!;
    protected IConfiguration configuration = null!;

    static string ApiKey => SteamLoginStateHelper.ApiKey;

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

        await GetSteamAuthenticatorAsync();
        await GetSteamLoginStateAsync();
    }

    /// <summary>
    /// 获取历史报价信息
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetTradeHistory()
    {
        var tradeOffersSummary = await steamTradeService.GetTradeOffersSummaryAsync(ApiKey);

        Assert.That(tradeOffersSummary, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(tradeOffersSummary.IsSuccess, Is.True);
            Assert.That(tradeOffersSummary.Content, Is.Not.Null);
        });

        var tradeHistoryDetail = await steamTradeService.GetTradeHistory(ApiKey);

        Assert.That(tradeHistoryDetail, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(tradeHistoryDetail.IsSuccess, Is.True);
            Assert.That(tradeHistoryDetail.Content, Is.Not.Null);
        });

        TestContext.WriteLine(Serializable.SJSON(tradeOffersSummary, writeIndented: true));
        TestContext.WriteLine(Serializable.SJSON(tradeHistoryDetail, writeIndented: true));
    }

    /// <summary>
    /// 接受交易报价
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task AcceptTradeOffer()
    {
        // ------------接受所有礼物形式的交易报价
        var accept_result = await steamTradeService.AcceptAllGiftTradeOfferAsync(SteamLoginState!.SteamId.ToString());
        Assert.That(accept_result, Is.Not.Null);
        Assert.That(accept_result.IsSuccess, Is.True);

        // ------------开启后台任务 定时接收礼物报价
        var startTask = await steamTradeService.StartTradeTask(
            steam_id: SteamLoginState.SteamId.ToString(),
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
            var confirmations = await steamTradeService.GetConfirmations(SteamLoginState.SteamId.ToString());
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
                    steam_id: SteamLoginState.SteamId.ToString(),
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
        var stopTask = await steamTradeService.StopTask(SteamLoginState.SteamId.ToString(), TradeTaskEnum.AutoAcceptGitTrade);
        Assert.That(stopTask, Is.Not.Null);
        Assert.That(stopTask.IsSuccess, Is.True);

        TestContext.WriteLine("OK");
    }

    /// <summary>
    /// 发送交易报价【交易链接】
    /// </summary>
    /// <param name="trade_url"></param>
    /// <param name="app_id"></param>
    /// <param name="context_id"></param>
    /// <returns></returns>
    [Test, Order(2)]
    [TestCase("https://steamcommunity.com/tradeoffer/new/?partner=1535133647&token=HxGEz2TY", "570", "2")]
    public async Task SendTradeOffer_TradeUrl(string trade_url, string app_id, string context_id)
    {
        var queryParams = HttpUtility.ParseQueryString(new Uri(trade_url).Query);
        var target_steam_id = new SteamIdConvert(queryParams["partner"].ThrowIsNull());

        await GetInventories(steamAccountService, ulong.Parse(target_steam_id.Id64), app_id, context_id);

        var them_inventories = InventoryPageResponse;

        if (them_inventories is null || them_inventories.Assets is null || them_inventories.Assets.Length <= 0)
            return;

        var my_times = new List<TradeAsset>();
        // 索取的物品
        var them_items = them_inventories.Assets!
                .Select(s => new TradeAsset
                {
                    AppId = s.AppId,
                    Amount = 1,
                    AssetId = s.AssetId,
                    ContextId = s.ContextId,
                })
                .Take(1)
                .ToList();

        var send_result = await steamTradeService.SendTradeOfferWithUrlAsync(
            steam_id: SteamLoginState!.SteamId.ToString(),
            trade_offer_url: trade_url,
            my_items: my_times,
            them_items: them_items,
            message: "this item is so lowed, why not give it to me");

        Assert.Multiple(() =>
        {
            Assert.That(send_result, Is.Not.Null);
            Assert.That(send_result.IsSuccess && send_result.Content, Is.True);
        });

        TestContext.WriteLine(Serializable.SJSON(send_result, writeIndented: true));
    }

    /// <summary>
    /// 发送交易报价 【好友】
    /// </summary>
    /// <param name="friend_steam_id"></param>
    /// <param name="app_id"></param>
    /// <param name="context_id"></param>
    /// <returns></returns>
    [Test, Order(1)]
    [TestCase("76561199495399375", "570", "2")]
    public async Task SendTradOffer_Friend(string friend_steam_id, string app_id, string context_id)
    {
        await GetInventories(steamAccountService, ulong.Parse(friend_steam_id), app_id, context_id);

        var them_inventories = InventoryPageResponse;

        if (them_inventories is null || them_inventories.Assets is null || them_inventories.Assets.Length <= 0)
            return;

        var my_times = new List<TradeAsset>();
        // 索取的物品
        var them_items = them_inventories.Assets!
                .Select(s => new TradeAsset
                {
                    AppId = s.AppId,
                    Amount = 1,
                    AssetId = s.AssetId,
                    ContextId = s.ContextId,
                })
                .Take(1)
                .ToList();

        var send_result = await steamTradeService.SendTradeOfferAsync(
            steam_id: SteamLoginState!.SteamId.ToString(),
            my_items: my_times,
            them_items: them_items,
            target_steam_id: friend_steam_id,
            message: "this item is so lowed, why not give it to me");

        Assert.Multiple(() =>
        {
            Assert.That(send_result, Is.Not.Null);
            Assert.That(send_result.IsSuccess && send_result.Content, Is.True);
        });

        TestContext.WriteLine(Serializable.SJSON(send_result, writeIndented: true));
    }

    /// <summary>
    /// 拒绝接收到的报价 和 取消已发送报价
    /// </summary>
    /// <returns></returns>
    [Test, Order(3)]
    [TestCase(76561199495399375)]
    public async Task Decline_Cancel_TradeOffer(long target_steam_id)
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

            var target_steam_id32 = long.Parse(new SteamIdConvert(target_steam_id.ToString()).Id32);
            var decline_trade_offers = fetched!.Response!.TradeOffersReceived?.Where(x => x.AccountIdOther == target_steam_id32).ToList();
            if (decline_trade_offers != null && decline_trade_offers.Count > 0)
            {
                foreach (var decline_trade_offer in decline_trade_offers)
                {
                    var decline_result = await steamTradeService.DeclineTradeOfferAsync(SteamLoginState!.SteamId.ToString(), decline_trade_offer.TradeOfferId);
                    Assert.That(decline_result, Is.Not.Null);
                    Assert.That(decline_result.IsSuccess && decline_result.Content, Is.True);
                }
            }

            var cancel_trade_offers = fetched!.Response!.TradeOffersSent?.Where(x => x.AccountIdOther == target_steam_id32).ToList();
            if (cancel_trade_offers != null && cancel_trade_offers.Count > 0)
            {
                foreach (var cancel_trade_offer in cancel_trade_offers)
                {
                    var cancel_result = await steamTradeService.CancelTradeOfferAsync(SteamLoginState.SteamId.ToString(), cancel_trade_offer.TradeOfferId);
                    Assert.That(cancel_result, Is.Not.Null);
                    Assert.That(cancel_result.IsSuccess && cancel_result.Content, Is.True);
                }
            }
        }

        TestContext.WriteLine("OK");
    }

    /// <summary>
    /// 测试获取交易确认列表
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetConfirmations()
    {
        if (SteamLoginState == null)
        {
            Assert.Pass("SteamLoginState is null.");
            return;
        }

        // 需要 IdentitySecret !!!!!
        var rsp = await steamTradeService.GetConfirmations(SteamLoginState.SteamId.ToString()!);

        Assert.That(rsp.IsSuccess && rsp.Content is not null);
        var confirmations = rsp.Content!;
        TradeConfirmation? confirmation = null;
        if (confirmations.Any())
        {
            foreach (var item in confirmations)
            {
                var assets = await steamTradeService.GetConfirmationImages(SteamLoginState.SteamId.ToString(), item);
                if (assets.Content.my_items.Length == 0)
                {
                    confirmation = item;
                    break;
                }
            }
        }
        if (confirmation is not null)
        {
            var param = new Dictionary<string, string>() { { confirmation.Id, confirmation.Nonce } };
            var sendResult = await steamTradeService.BatchSendConfirmation(SteamLoginState.SteamId.ToString()!, param, true);

            Assert.That(sendResult, Is.Not.Null);
            Assert.That(sendResult.Content);
        }

        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
    }
}
