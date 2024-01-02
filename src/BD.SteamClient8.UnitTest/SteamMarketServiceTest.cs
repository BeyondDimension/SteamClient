namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="steamMarketService"/> 单元测试
/// </summary>
sealed class SteamMarketServiceTest : ServiceTestBase
{
    ISteamMarketService steamMarketService = null!;

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamMarketService = GetRequiredService<ISteamMarketService>();

        await GetSteamAuthenticatorAsync();
        await GetSteamLoginStateAsync();
    }

    /// <summary>
    /// 测试获取市场订单柱状图数据
    /// </summary>
    /// <param name="marketItemNameId"></param>
    /// <returns></returns>
    [TestCase(2384364)]
    [Test]
    public async Task GetMarketItemOrdersHistogram(long marketItemNameId)
    {
        var histogram = await steamMarketService
            .GetMarketItemOrdersHistogram(marketItemNameId);

        Assert.Multiple(() =>
        {
            Assert.That(histogram.IsSuccess && histogram.Content is not null);
            Assert.That(histogram.Content?.Success, Is.EqualTo(1));
        });

        TestContext.WriteLine(Serializable.SJSON(histogram, writeIndented: true));
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
    public async Task GetMarketItemPriceOverview(string appId, string marketHashName, int currency)
    {
        var overview = await steamMarketService
            .GetMarketItemPriceOverview(appId, marketHashName, currency);

        Assert.Multiple(() =>
        {
            Assert.That(overview.IsSuccess && overview.Content is not null);
            Assert.That(overview.Content?.Success, Is.True);
        });

        TestContext.WriteLine(Serializable.SJSON(overview, writeIndented: true));
    }

    /// <summary>
    /// 测试获取市场出售信息
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetMyListings()
    {
        if (SteamLoginState == null)
        {
            Assert.Pass("SteamLoginState is null.");
            return;
        }
        var rsp = await steamMarketService.GetMarketListing(SteamLoginState);

        Assert.That(rsp.IsSuccess && rsp.Content is not null);

        var listings = rsp.Content;
        listings.ThrowIsNull();
        var activeListings = listings.ActiveListings.ToList();
        var buyorders = listings.Buyorders.ToList();

        Assert.That(listings.ActiveListings, Is.Not.Null);

        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
    }
}
