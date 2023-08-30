using static System.Net.Http.HttpClientUseCookiesWithProxyServiceImpl;

namespace BD.SteamClient.UnitTest;

public sealed class SteamAccountServiceTest
{
    SteamLoginState? globalState;

    IServiceProvider service;

    ISteamAccountService Client => service.GetRequiredService<ISteamAccountService>();

    internal class TestRandomGetUserAgentService : IRandomGetUserAgentService
    {
        public string GetUserAgent()
        {
            return IRandomGetUserAgentService.Steam;
        }
    }

    internal class TestAppSettings : IAppSettings, IOptionsMonitor<IAppSettings>
    {
        public string? WebProxyAddress { get; set; }

        public int? WebProxyPort { get; set; }

        public string? WebProxyUserName { get; set; }

        public string? WebProxyPassword { get; set; }

        public IAppSettings CurrentValue => this;

        public IAppSettings Get(string? name)
        {
            return this;
        }

        public IDisposable? OnChange(Action<IAppSettings, string?> listener)
        {
            return null;
        }
    }

    [SetUp]
    public void Setup()
    {
        if (ProjectUtils.IsCI())
            return;

        var services = new ServiceCollection();
        services.AddTransient<ISteamAccountService, SteamAccountService>();
        services.AddTransient<ISteamSessionService, SteamSessionServiceImpl>();
        services.AddTransient<ISteamMarketService, SteamMarketService>();
        services.AddTransient<IRandomGetUserAgentService, TestRandomGetUserAgentService>();
        services.AddTransient<IOptionsMonitor<IAppSettings>, TestAppSettings>();

        services.AddLogging();

        service = services.BuildServiceProvider();

        service.GetRequiredService<IRandomGetUserAgentService>();
        service.GetRequiredService<IOptionsMonitor<IAppSettings>>();
        service.GetRequiredService<ISteamAccountService>();
        service.GetRequiredService<ISteamSessionService>();

        if (globalState == null)
        {

            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/state.json";

            if (File.Exists(path))
            {
                using FileStream fs = new FileStream(path, FileMode.Open);

                globalState = JsonSerializer.Deserialize<SteamLoginState>(fs);
            }

            // if (true)
            // {
            //     globalState = new SteamLoginState()
            //     {
            //         Username = globalState!.Username,
            //         Password = globalState!.Password,
            //     };
            //     Client.DoLoginV2Async(globalState!).GetAwaiter().GetResult();
            //     Client.DoLoginV2Async(globalState!).GetAwaiter().GetResult();
            //     string x = JsonSerializer.Serialize(globalState);
            // }
        }
    }

    /// <summary>
    /// 测试获取库存列表
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="appId"></param>
    /// <param name="contextId"></param>
    /// <returns></returns>
    [TestCase(76561199473732040UL, "730", "2")]
    [Test]
    public async Task TestGetInventories(ulong steamId, string appId, string contextId)
    {
        if (ProjectUtils.IsCI())
            return;

        var resp = await Client.GetInventories(steamId, appId, contextId, 1);

        Assert.That(resp.Success, Is.EqualTo(1));
    }

    [TestCase(null)]
    [TestCase(new[] { 754, 730, 570 })]
    [Test]
    public async Task TestGetAndParseInventoryTradingHistory(int[]? appFilter)
    {
        if (globalState != null)
        {
            InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null;

            var page = await Client.GetInventoryTradeHistory(globalState!, appFilter, cursor);

            Assert.That(page, Is.Not.Null);
            Assert.That(page.Success, Is.True);

            var parsedRows = Client.ParseInventoryTradeHistory(page.Html)
                .ToBlockingEnumerable()
                .ToList();

            Assert.That(parsedRows, Is.Not.Null);
        }
    }

    /// <summary>
    /// 测试获取开发api key
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestGetApiKey()
    {
        if (globalState != null)
        {
            string? apiKey = await Client.GetApiKey(globalState!);

            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = await Client.RegisterApiKey(globalState!);
            }

            Assert.That(apiKey, Is.Not.Null);
            Assert.That(apiKey, Is.Not.EqualTo(string.Empty));
        }
    }

    [Test]
    public async Task TestGetSendGiftHistory()
    {
        if (globalState != null)
        {
            var history = await Client.GetSendGiftHisotries(globalState!);

            Assert.That(history, Is.Not.Null);
        }
    }

    [Test]
    public Task TestGetLoginHistory()
    {
        if (globalState != null)
        {
            var result = Client.GetLoginHistory(globalState!);

            Assert.That(result, Is.Not.Null);
        }

        return Task.CompletedTask;
    }

    //[Test]
    //public async Task TestDoLoginV2Async()
    //{
    //    //var loginState = new SteamLoginResponse
    //    //{
    //    //    Username = "hhhhhh",
    //    //    Password = "hhhhhh",
    //    //};
    //    //await Client.DoLoginV2Async(loginState);
    //    //TestContext.WriteLine(loginState);
    //    //Assert.True(loginState.Success);
    //}

    //[Test]
    //public async Task TestGetRSAkeyV2Async()
    //{
    //    //var data = await Client.GetRSAkeyV2Async("hhhhhh", "hhhhhh");
    //    //TestContext.WriteLine(data);
    //    //Assert.True(data.encryptedPassword64 != null);
    //}
}