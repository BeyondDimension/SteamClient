namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
/// <summary>
/// <see cref="SteamAccountService"/> 单元测试
/// </summary>
public class SteamAccountServiceTest
{
    IServiceProvider service;

    ISteamAccountService Client => service.GetRequiredService<ISteamAccountService>();

    SteamLoginState? loginState;

    /// <summary>
    /// Setup 函数的注释
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFusilladeHttpClientFactory();
        services.AddSteamAccountService();

        service = services.BuildServiceProvider();

        if (loginState == null)
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/state.json";

            if (File.Exists(path))
            {
                using FileStream fs = new FileStream(path, FileMode.Open);

                loginState = SystemTextJsonSerializer.Deserialize<SteamLoginState>(fs);
            }
            else
            {
                var localPath = @"C:\Users\CYCY\Desktop\session.json";
                var json = JsonDocument.Parse(File.ReadAllText(localPath)).RootElement;
                loginState = new SteamLoginState()
                {
                    Username = json.GetProperty("userName").ToString(),
                    Password = json.GetProperty("passWord").ToString()
                };
                Client.DoLoginV2Async(loginState!).GetAwaiter().GetResult();
                Client.DoLoginV2Async(loginState!).GetAwaiter().GetResult();
                string x = SystemTextJsonSerializer.Serialize(loginState);
                File.WriteAllText(path, x);
            }
        }
    }

    /// <summary>
    /// 测试获取库存列表
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="appId"></param>
    /// <param name="contextId"></param>
    /// <returns></returns>
    [TestCase(76561198425787706UL, "730", "2")]
    [Test]
    public async Task TestGetInventories(ulong steamId, string appId, string contextId)
    {
        var resp = (await Client.GetInventories(steamId, appId, contextId, 1)).Content;

        Assert.That(resp.Success, Is.EqualTo(1));
    }

    [TestCase(null)]
    [TestCase(new[] { 754, 730, 570 })]
    [Test]
    public async Task TestGetAndParseInventoryTradingHistory(int[]? appFilter)
    {
        if (loginState != null)
        {
            InventoryTradeHistoryRenderPageResponse.InventoryTradeHistoryCursor? cursor = null;

            var page = (await Client.GetInventoryTradeHistory(loginState!, appFilter, cursor)).Content;

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
        if (loginState != null)
        {
            string? apiKey = (await Client.GetApiKey(loginState!)).Content;

            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = (await Client.RegisterApiKey(loginState!)).Content;
            }

            Assert.That(apiKey, Is.Not.Null);
            Assert.That(apiKey, Is.Not.EqualTo(string.Empty));
        }
    }

    [Test]
    public async Task TestGetSendGiftHistory()
    {
        if (loginState != null)
        {
            var history = await Client.GetSendGiftHistories(loginState!);

            Assert.That(history, Is.Not.Null);
        }
    }

    [Test]
    public async Task TestGetLoginHistory()
    {
        if (loginState != null)
        {
            var result = Client.GetLoginHistory(loginState!);

            await foreach (var item in result)
            {
                Console.WriteLine(item.City);
            }

            Assert.That(result, Is.Not.Null);
        }
        await Task.CompletedTask;
    }
}
