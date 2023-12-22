namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamIdleCardServiceImpl"/> 单元测试
/// </summary>
sealed class SteamIdleServiceTest : ServiceTestBase
{
    SteamLoginState steamLoginState = null!; // ?未引用？
    ISteamIdleCardService steamIdleCardService = null!;
    ISteamAccountService steamAccountService = null!;
    ISteamSessionService steamSessionService = null!;
    IConfiguration configuration = null!;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamAccountService();
        services.AddSteamIdleCardService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamIdleCardService = GetRequiredService<ISteamIdleCardService>();
        steamAccountService = GetRequiredService<ISteamAccountService>();
        steamSessionService = GetRequiredService<ISteamSessionService>();
        configuration = GetRequiredService<IConfiguration>();

        steamLoginState = await GetSteamLoginStateAsync(configuration, steamAccountService, steamSessionService);
    }

    /// <summary>
    /// 测试获取用户徽章和卡片数据
    /// </summary>
    /// <param name="steam_id"></param>
    /// <returns></returns>
    [TestCase("76561199494800019")]
    [Test]
    public async Task TestsGetBadgesAsync(string steam_id)
    {
        var rsp = await steamIdleCardService.GetBadgesAsync(steam_id);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content.idleInfo, Is.Not.Null);
            Assert.That(rsp.Content.badges, Is.Not.Null);
        });
    }

    /// <summary>
    /// 测试获取游戏卡组卡片平均价格
    /// </summary>
    /// <param name="appIds"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    [TestCase(new uint[] { 730, 580 }, "CNY")]
    [Test]
    public async Task TestGetAppCardsAvgPrice(uint[] appIds, string currency)
    {
        var rsp = await steamIdleCardService.GetAppCardsAvgPrice(appIds, currency);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content, Is.Not.Null);
            Assert.That(rsp.Content, Is.Not.Empty);
        });
    }

    /// <summary>
    /// 测试获取游戏卡片价格
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    [TestCase(730U, "CNY")]
    [Test]
    public async Task TestGetAppCardsAvgPrice(uint appId, string currency)
    {
        var rsp = await steamIdleCardService.GetCardsMarketPrice(appId, currency);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content, Is.Not.Null);
            Assert.That(rsp.Content, Is.Not.Empty);
        });
    }
}
