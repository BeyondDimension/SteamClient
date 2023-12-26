namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamAccountService"/> 单元测试
/// </summary>
sealed class SteamAccountServiceTest : ServiceTestBase
{
    ISteamAccountService steamAccountService = null!;

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamAccountService = GetRequiredService<ISteamAccountService>();

        SteamLoginState.ThrowIsNull();
    }

    /// <summary>
    /// 测试获取库存列表
    /// </summary>
    /// <param name="steam_id"></param>
    /// <param name="appId"></param>
    /// <param name="contextId"></param>
    /// <returns></returns>
    [TestCase(76561199495399375UL, "570", "2")]
    [Test]
    public async Task TestGetInventories(ulong steam_id, string appId, string contextId)
    {
        await GetInventories(steamAccountService, steam_id, appId, contextId);
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
        if (SteamLoginState != null)
        {
            InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null;

            var rsp = await steamAccountService.GetInventoryTradeHistory(SteamLoginState!, appFilter, cursor);

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
        if (SteamLoginState != null)
        {
            string? apiKey = (await steamAccountService.GetApiKey(SteamLoginState!)).Content;

            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = (await steamAccountService.RegisterApiKey(SteamLoginState!)).Content;
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
        if (SteamLoginState != null)
        {
            var history = await steamAccountService.GetSendGiftHistories(SteamLoginState!);

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
        if (SteamLoginState != null)
        {
            var result = steamAccountService.GetLoginHistory(SteamLoginState);

            Assert.That(result, Is.Not.Null);

            await foreach (var item in result)
            {
                Console.WriteLine(item.City);
            }
        }
    }
}
