using System.Net;
using static System.Net.Http.HttpClientUseCookiesWithProxyServiceImpl;

namespace BD.SteamClient.UnitTest;

public class SteamMarketServiceTest
{
    IServiceProvider service;

    SteamLoginState? globalState;

    ISteamAccountService AccountService => service.GetRequiredService<ISteamAccountService>();

    ISteamMarketService SteamMarketService => service.GetRequiredService<ISteamMarketService>();

    ISteamTradeService TradeService => service.GetRequiredService<ISteamTradeService>();

    ISteamSessionService Session => service.GetRequiredService<ISteamSessionService>();

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
        var services = new ServiceCollection();
        services.AddTransient<ISteamAccountService, SteamAccountService>();
        services.AddTransient<ISteamSessionService, SteamSessionServiceImpl>();
        services.AddTransient<ISteamMarketService, SteamMarketService>();
        services.AddTransient<IRandomGetUserAgentService, TestRandomGetUserAgentService>();
        services.AddTransient<IOptionsMonitor<IAppSettings>, TestAppSettings>();
        services.AddSingleton<ISteamSessionService>(new SteamSessionServiceImpl());
        services.AddSingleton<ISteamIdleCardService, SteamIdleCardServiceImpl>();
        services.AddTransient<ISteamTradeService, SteamTradeServiceImpl>();
        services.AddSteamAuthenticatorService();
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
            //     AccountService.DoLoginV2Async(globalState!).GetAwaiter().GetResult();
            //     AccountService.DoLoginV2Async(globalState!).GetAwaiter().GetResult();
            //     string bbbs = JsonSerializer.Serialize(globalState);
            // }
        }

        CookieContainer c = new CookieContainer();
        c.Add(globalState.Cookies!);

        var s = service.GetRequiredService<ISteamSessionService>();
        var serverTimeStr = service.GetRequiredService<ISteamAuthenticatorService>().TwoFAQueryTime().GetAwaiter().GetResult();
        var serverTime = JsonDocument.Parse(serverTimeStr).RootElement.GetProperty("response").GetProperty("server_time").GetString();
        var diff = (long.Parse(serverTime!) * 1000L) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        bool x = s.AddOrSetSeesion(new SteamSession()
        {
            CookieContainer = c,
            SteamId = globalState.SteamId.ToString(),
            ServerTimeDiff = diff,
            //IdentitySecret = "XXXXXXXXXXXX"
        });

    }

    [TestCase(2384364)]
    [Test]
    public async Task TestGetMarketItemOrdersHistogram(long marketItemNameId)
    {
        var histogram = await SteamMarketService
        .GetMarketItemOrdersHistogram(2384364);

        Assert.That(histogram.Success, Is.EqualTo(1));
    }

    [TestCase("730", "AK-47%20%7C%20Safari%20Mesh%20%28Field-Tested%29", 23)]
    [Test]
    public async Task TestGetMarketItemPriceOverview(string appId, string marketHashName, int currency)
    {
        var overview = await SteamMarketService
        .GetMarketItemPriceOverview(appId, marketHashName, currency);

        Assert.That(overview.Success, Is.True);
    }

    [Test]
    public async Task TestGetMyListings()
    {
        if (globalState != null)
        {
            var listings = await SteamMarketService.GetMarketListing(globalState!);

            var activeListings = listings.ActiveListings.ToList();
            var buyorders = listings.Buyorders.ToList();

            Assert.That(listings.ActiveListings, Is.Not.Null);
        }
    }

    [Test]
    public async Task TestGetConfirmations()
    {
        if (globalState != null)
        {

            // 需要  IdentitySecret !!!!!
            var confirmations = await TradeService.GetConfirmations(globalState.SteamId.ToString()!);

            Dictionary<string, string> param = new Dictionary<string, string>(confirmations.Select(x => new KeyValuePair<string, string>(x.Id, x.Nonce)));

            if (param.Any())
            {
                //var res = await TradeService.SendConfirmation(globalState.SteamId.ToString()!, confirmations.First(), true);
                var sendResult = await TradeService.BatchSendConfirmation(globalState.SteamId.ToString()!, param, true);
            }
        }
    }
}
