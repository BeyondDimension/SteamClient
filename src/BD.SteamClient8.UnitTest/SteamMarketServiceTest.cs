namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="steamMarketService"/> 单元测试
/// </summary>
sealed class SteamMarketServiceTest : ServiceTestBase
{
    SteamLoginState steamLoginState = null!;
    ISteamAccountService steamAccountService = null!;
    IConfiguration configuration = null!;
    ISteamMarketService steamMarketService = null!;
    ISteamTradeService steamTradeService = null!;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamAccountService();
        services.AddSteamTradeService();
        services.AddSteamMarketService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamTradeService = GetRequiredService<ISteamTradeService>();
        steamMarketService = GetRequiredService<ISteamMarketService>();
        steamAccountService = GetRequiredService<ISteamAccountService>();
        configuration = GetRequiredService<IConfiguration>();

        steamLoginState = await GetSteamLoginStateAsync(configuration, steamAccountService);
    }

    /// <summary>
    /// 测试获取市场订单柱状图数据
    /// </summary>
    /// <param name="marketItemNameId"></param>
    /// <returns></returns>
    [TestCase(2384364)]
    [Test]
    public async Task TestGetMarketItemOrdersHistogram(long marketItemNameId)
    {
        var histogram = await steamMarketService
            .GetMarketItemOrdersHistogram(marketItemNameId);

        Assert.That(histogram.IsSuccess && histogram.Content is not null);
        Assert.That(histogram.Content?.Success, Is.EqualTo(1));
    }

    /// <summary>
    /// 测试获取市场物品价格概述
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="marketHashName"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    [TestCase("730", "AK-47%20%7C%20Safari%20Mesh%20%28Field-Tested%29", 23)]
    [Test]
    public async Task TestGetMarketItemPriceOverview(string appId, string marketHashName, int currency)
    {
        var overview = await steamMarketService
            .GetMarketItemPriceOverview(appId, marketHashName, currency);

        Assert.That(overview.IsSuccess && overview.Content is not null);
        Assert.That(overview.Content?.Success, Is.True);
    }

    /// <summary>
    /// 测试获取市场出售信息
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestGetMyListings()
    {
        if (steamLoginState != null)
        {
            var rsp = await steamMarketService.GetMarketListing(steamLoginState!);

            Assert.That(rsp.IsSuccess && rsp.Content is not null);

            var listings = rsp.Content;
            listings.ThrowIsNull();
            var activeListings = listings.ActiveListings.ToList();
            var buyorders = listings.Buyorders.ToList();

            Assert.That(listings.ActiveListings, Is.Not.Null);
        }
    }

    /// <summary>
    /// 测试获取交易确认列表
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestGetConfirmations()
    {
        if (steamLoginState != null)
        {
            // 需要 IdentitySecret !!!!!
            var rsp = await steamTradeService.GetConfirmations(steamLoginState.SteamId.ToString()!);

            Assert.That(rsp.IsSuccess && rsp.Content is not null);
            var confirmations = rsp.Content!;
            Dictionary<string, string> param = new Dictionary<string, string>(confirmations.Select(x => new KeyValuePair<string, string>(x.Id, x.Nonce)));

            if (param.Count > 0)
            {
                //var res = await TradeService.SendConfirmation(loginState.SteamId.ToString()!, confirmations.First(), true);
                var sendResult = await steamTradeService.BatchSendConfirmation(steamLoginState.SteamId.ToString()!, param, true);
            }
        }
    }
}
