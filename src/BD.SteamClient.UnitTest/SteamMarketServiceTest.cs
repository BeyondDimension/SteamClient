using static System.Net.Http.HttpClientUseCookiesWithProxyServiceImpl;

namespace BD.SteamClient.UnitTest;

public class SteamMarketServiceTest
{
    IServiceProvider service;

    SteamLoginState? globalState;

    ISteamAccountService AccountService => service.GetRequiredService<ISteamAccountService>();

    ISteamMarketService SteamMarketService => service.GetRequiredService<ISteamMarketService>();

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
        }
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
}
