namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
/// <summary>
/// <see cref="SteamMarketService"/> 单元测试
/// </summary>
public class SteamMarketServiceTest
{
    IServiceProvider service;

    SteamLoginState? loginState;

    ISteamAccountService AccountService => service.GetRequiredService<ISteamAccountService>();

    ISteamMarketService SteamMarketService => service.GetRequiredService<ISteamMarketService>();

    ISteamTradeService TradeService => service.GetRequiredService<ISteamTradeService>();

    public SteamMarketServiceTest()
    {
        var services = new ServiceCollection();
        services.TryAddHttpPlatformHelper();
        services.AddLogging();
        services.AddSteamAccountService();
        services.AddSteamTradeService();
        services.AddSteamMarketService();
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
                AccountService.DoLoginV2Async(loginState!).GetAwaiter().GetResult();
                AccountService.DoLoginV2Async(loginState!).GetAwaiter().GetResult();
                string x = SystemTextJsonSerializer.Serialize(loginState);
                File.WriteAllText(path, x);
            }
        }
    }

    [TestCase(2384364)]
    [Test]
    public async Task TestGetMarketItemOrdersHistogram(long marketItemNameId)
    {
        var histogram = await SteamMarketService
        .GetMarketItemOrdersHistogram(marketItemNameId);

        Assert.IsTrue(histogram.IsSuccess && histogram.Content is not null);
        Assert.That(histogram.Content.Success, Is.EqualTo(1));
    }

    [TestCase("730", "AK-47%20%7C%20Safari%20Mesh%20%28Field-Tested%29", 23)]
    [Test]
    public async Task TestGetMarketItemPriceOverview(string appId, string marketHashName, int currency)
    {
        var overview = await SteamMarketService
            .GetMarketItemPriceOverview(appId, marketHashName, currency);

        Assert.IsTrue(overview.IsSuccess && overview.Content is not null);
        Assert.That(overview.Content.Success, Is.True);
    }

    [Test]
    public async Task TestGetMyListings()
    {
        if (loginState != null)
        {
            var rsp = await SteamMarketService.GetMarketListing(loginState!);

            Assert.IsTrue(rsp.IsSuccess && rsp.Content is not null);

            var listings = rsp.Content;
            var activeListings = listings.ActiveListings.ToList();
            var buyorders = listings.Buyorders.ToList();

            Assert.That(listings.ActiveListings, Is.Not.Null);
        }
    }

    [Test]
    public async Task TestGetConfirmations()
    {
        if (loginState != null)
        {
            // 需要  IdentitySecret !!!!!
            var rsp = await TradeService.GetConfirmations(loginState.SteamId.ToString()!);

            Assert.IsTrue(rsp.IsSuccess && rsp.Content is not null);
            var confirmations = rsp.Content!;
            Dictionary<string, string> param = new Dictionary<string, string>(confirmations.Select(x => new KeyValuePair<string, string>(x.Id, x.Nonce)));

            if (param.Count > 0)
            {
                //var res = await TradeService.SendConfirmation(loginState.SteamId.ToString()!, confirmations.First(), true);
                var sendResult = await TradeService.BatchSendConfirmation(loginState.SteamId.ToString()!, param, true);
            }
        }
    }
}
