namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamIdleCardServiceImpl"/> 单元测试
/// </summary>
sealed class SteamIdleServiceTest : ServiceTestBase
{
    ISteamIdleCardService steamIdleCardService = null!;

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamIdleCardService = GetRequiredService<ISteamIdleCardService>();

        await GetSteamAuthenticatorAsync();
        await GetSteamLoginStateAsync();
    }

    /// <summary>
    /// 测试获取用户徽章和卡片数据
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    [Test]
    public async Task GetBadgesAsync()
    {
        Assert.That(SteamLoginState?.SteamId, Is.Not.Null);
        var rsp = await steamIdleCardService.GetBadgesAsync(SteamLoginState.SteamId.ToString());

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content.UserIdleInfo, Is.Not.Null);
            Assert.That(rsp.Content.Badges, Is.Not.Null);
        });

        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
    }

    /// <summary>
    /// 测试获取游戏卡组卡片平均价格
    /// </summary>
    /// <param name="appIds"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    [TestCase(new uint[] { 730, 580 }, "CNY")]
    [Test]
    public async Task GetAppCardsAvgPrice(uint[] appIds, string currency)
    {
        var rsp = await steamIdleCardService.GetAppCardsAvgPrice(appIds, currency);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content, Is.Not.Null);
        });

        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
    }

    /// <summary>
    /// 测试获取游戏卡片价格
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    [TestCase(730U, "CNY")]
    [Test]
    public async Task GetAppCardsMarketPrice(uint appId, string currency)
    {
        var rsp = await steamIdleCardService.GetCardsMarketPrice(appId, currency);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content, Is.Not.Null);
        });

        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
    }
}
