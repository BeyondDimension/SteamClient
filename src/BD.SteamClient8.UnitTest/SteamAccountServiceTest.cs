using BD.SteamClient8.Models.WebApi.Profiles;
using BD.SteamClient8.Services.Abstractions.WebApi;
using BD.SteamClient8.Services.WebApi;

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

        await GetSteamAuthenticatorAsync();
        await GetSteamLoginStateAsync();
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
    public async Task GetInventories(ulong steam_id, string appId, string contextId)
    {
        await GetInventories(steamAccountService, steam_id, appId, contextId);

        TestContext.WriteLine(Serializable.SJSON(InventoryPageResponse, writeIndented: true));
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
    public async Task GetAndParseInventoryTradingHistory(int[]? appFilter)
    {
        if (SteamLoginState == null)
        {
            Assert.Pass("SteamLoginState is null.");
            return;
        }

        InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null;

        var rsp = await steamAccountService.GetInventoryTradeHistory(SteamLoginState!, appFilter, cursor);

        Assert.Multiple(() =>
        {
            Assert.That(rsp, Is.Not.Null);
        });

        var parsedRows = steamAccountService.ParseInventoryTradeHistory(rsp.Html)
            .ToBlockingEnumerable()
            .ToArray();

        Assert.That(parsedRows, Is.Not.Null);

        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
        TestContext.WriteLine(Serializable.SJSON(parsedRows, writeIndented: true));
    }

    /// <summary>
    /// 测试获取开发 ApiKey
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetApiKey()
    {
        if (SteamLoginState == null)
        {
            Assert.Pass("SteamLoginState is null.");
            return;
        }

        string? apiKey = await steamAccountService.GetApiKey(SteamLoginState);

        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = await steamAccountService.RegisterApiKey(SteamLoginState);
        }

        Assert.That(apiKey, Is.Not.Null);
        Assert.That(apiKey, Is.Not.EqualTo(string.Empty));

        TestContext.WriteLine("OK");
    }

    /// <summary>
    /// 测试获取发送礼物记录
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetSendGiftHistory()
    {
        if (SteamLoginState == null)
        {
            Assert.Pass("SteamLoginState is null.");
            return;
        }

        var history = await steamAccountService.GetSendGiftHistories(SteamLoginState);
        Assert.That(history, Is.Not.Null);

        TestContext.WriteLine(Serializable.SJSON(history, writeIndented: true));
    }

    /// <summary>
    /// 测试获取登录历史记录
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetLoginHistory()
    {
        if (SteamLoginState == null)
        {
            Assert.Pass("SteamLoginState is null.");
            return;
        }

        var result = steamAccountService.GetLoginHistory(SteamLoginState);

        Assert.That(result, Is.Not.Null);

        List<LoginHistoryItem> loginHistories = new();
        await foreach (var item in result)
        {
            Console.WriteLine(item.City);
            loginHistories.Add(item);
        }

        TestContext.WriteLine(Serializable.SJSON(loginHistories, writeIndented: true));
    }
}
