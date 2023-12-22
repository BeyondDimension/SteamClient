namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamAccountService"/> 单元测试
/// </summary>
sealed class SteamAccountServiceTest : ServiceTestBase
{
    SteamLoginState? steamLoginState;
    ISteamAccountService steamAccountService = null!;
    IConfiguration configuration = null!;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamAccountService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamAccountService = GetRequiredService<ISteamAccountService>();
        configuration = GetRequiredService<IConfiguration>();

        steamLoginState = await GetSteamLoginStateAsync(configuration, steamAccountService, GetRequiredService<ISteamSessionService>());
    }

    /// <summary>
    /// 测试获取库存列表
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="appId"></param>
    /// <param name="contextId"></param>
    /// <returns></returns>
    [TestCase(76561199494800019UL, "730", "2")]
    [Test]
    public async Task TestGetInventories(ulong steamId, string appId, string contextId)
    {
        var rsp = await steamAccountService.GetInventories(steamId, appId, contextId, 1);

        Assert.That(rsp.Content.ThrowIsNull().Success, Is.EqualTo(1));
    }

    /// <summary>
    /// 测试获取库存交易历史
    /// </summary>
    /// <param name="appFilter"></param>
    /// <returns></returns>
    [TestCase(null)]
#pragma warning disable CA1861 // 不要将常量数组作为参数
    [TestCase(new int[] { 754, 730, 570 })]
#pragma warning restore CA1861 // 不要将常量数组作为参数
    [Test]
    public async Task TestGetAndParseInventoryTradingHistory(int[]? appFilter)
    {
        if (steamLoginState != null)
        {
            InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null;

            var rsp = await steamAccountService.GetInventoryTradeHistory(steamLoginState!, appFilter, cursor);

            Assert.Multiple(() =>
            {
                Assert.That(rsp?.Content, Is.Not.Null);
                Assert.That(rsp!.Content!.Success, Is.True);
            });

            var parsedRows = steamAccountService.ParseInventoryTradeHistory(rsp.Content.Html)
                .ToBlockingEnumerable()
                .ToArray();

            Assert.That(parsedRows, Is.Not.Null);
        }
    }

    /// <summary>
    /// 测试获取开发 ApiKey
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestGetApiKey()
    {
        if (steamLoginState != null)
        {
            string? apiKey = (await steamAccountService.GetApiKey(steamLoginState!)).Content;

            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = (await steamAccountService.RegisterApiKey(steamLoginState!)).Content;
            }

            Assert.That(apiKey, Is.Not.Null);
            Assert.That(apiKey, Is.Not.EqualTo(string.Empty));
        }
    }

    /// <summary>
    /// 测试获取发送礼物记录
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestGetSendGiftHistory()
    {
        if (steamLoginState != null)
        {
            var history = await steamAccountService.GetSendGiftHistories(steamLoginState!);

            Assert.That(history, Is.Not.Null);
        }
    }

    /// <summary>
    /// 测试获取登录历史记录
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestGetLoginHistory()
    {
        if (steamLoginState != null)
        {
            var result = steamAccountService.GetLoginHistory(steamLoginState);

            Assert.That(result, Is.Not.Null);

            await foreach (var item in result)
            {
                Console.WriteLine(item.City);
            }
        }
    }
}
