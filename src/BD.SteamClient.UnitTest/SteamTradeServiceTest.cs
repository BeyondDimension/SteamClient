using BD.SteamClient.Models.Trade;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.SteamClient.UnitTest;

public sealed class SteamTradeServiceTest
{
    private string _steamId = "76561199079758512";

    private string _identitySecret = "dF7d1PLbB3p49Y303v0ecrpEDHI=";

    IServiceProvider service;

    ISteamTradeService Client => service.GetRequiredService<ISteamTradeService>();

    ISteamAccountService Account => service.GetRequiredService<ISteamAccountService>();

    ISteamIdleCardService Idle => service.GetRequiredService<ISteamIdleCardService>();

    static bool IsCI => ProjectUtils.IsCI();

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));
        services.AddSteamAccountService(c => new SocketsHttpHandler() { CookieContainer = c });
        services.AddSteamTradeService(c => new SocketsHttpHandler() { CookieContainer = c });
        services.AddSingleton<ISteamIdleCardService, SteamIdleCardServiceImpl>();
        service = services.BuildServiceProvider();
    }

    [Test]
    public async Task Run()
    {
    }
}
