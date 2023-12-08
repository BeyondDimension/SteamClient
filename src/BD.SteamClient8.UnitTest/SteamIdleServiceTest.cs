namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

/// <summary>
/// <see cref="SteamIdleCardServiceImpl"/> 单元测试
/// </summary>
class SteamIdleServiceTest
{
    IServiceProvider service;

    SteamLoginState loginState = null;

    ISteamIdleCardService Idle => service.GetRequiredService<ISteamIdleCardService>();

    ISteamAccountService Client => service.GetRequiredService<ISteamAccountService>();

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.TryAddHttpPlatformHelper();
        services.AddLogging();
        services.AddSteamAccountService();
        services.AddSteamIdleCardService();
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

    [TestCase("76561198425787706")]
    [Test]
    public async Task TestsGetBadgesAsync(string steam_id)
    {
        var badges = await Idle.GetBadgesAsync(steam_id);

        Assert.IsTrue(badges.IsSuccess);
        Assert.IsNotNull(badges.Content.idleInfo);
        Assert.IsNotNull(badges.Content.badges);
    }

    [TestCase(new uint[] { 730, 580 }, "CNY")]
    [Test]
    public async Task TestGetAppCardsAvgPrice(uint[] appIds, string currency)
    {
        var avgPrices = await Idle.GetAppCardsAvgPrice(appIds, currency);

        Assert.IsTrue(avgPrices.IsSuccess);
        Assert.IsNotNull(avgPrices.Content);
        Assert.IsTrue(avgPrices.Content.Count() > 0);
    }

    [TestCase(730U, "CNY")]
    [Test]
    public async Task TestGetAppCardsAvgPrice(uint appId, string currency)
    {
        var avgPrices = await Idle.GetCardsMarketPrice(appId, currency);

        Assert.IsTrue(avgPrices.IsSuccess);
        Assert.IsNotNull(avgPrices.Content);
        Assert.IsTrue(avgPrices.Content.Count() > 0);
    }
}
