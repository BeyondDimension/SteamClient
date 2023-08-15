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
        SteamSession steamSession = new SteamSession();
        steamSession.SteamId = _steamId;
        var cookieContainer = new CookieContainer();
        var uri = new Uri("https://steamcommunity.com/");
        cookieContainer.Add(uri, new Cookie("sessionid", "bb58a1bd59b38d31b002c1f5"));
        cookieContainer.Add(uri, new Cookie("steamLoginSecure", "76561199079758512||eyAidHlwIjogIkpXVCIsICJhbGciOiAiRWREU0EiIH0.eyAiaXNzIjogInI6MTQyQV8yMkZEQjZDN183OUFBOSIsICJzdWIiOiAiNzY1NjExOTkwNzk3NTg1MTIiLCAiYXVkIjogWyAid2ViIiBdLCAiZXhwIjogMTY5MTgyMzA3MCwgIm5iZiI6IDE2ODMwOTUwNTQsICJpYXQiOiAxNjkxNzM1MDU0LCAianRpIjogIjE0MkFfMjJGREI2QzdfODU3ODEiLCAib2F0IjogMTY5MTczNTA1MywgInJ0X2V4cCI6IDE2OTQzMjEyNDYsICJwZXIiOiAwLCAiaXBfc3ViamVjdCI6ICIxMTAuNTIuMjE3LjEwOSIsICJpcF9jb25maXJtZXIiOiAiMTEwLjUyLjIxNy4xMDkiIH0.0wbZIYUMOD2As9Igs5ycaZQt5yxrxr4BI_8TkR2AhC6ZwqBnf-VbFBDJs6bSHobgZAUI9xd5UzBql3RgRI2pCw"));
        steamSession.CookieContainer = cookieContainer;
        steamSession.HttpClient = new HttpClient(new HttpClientHandler() { UseCookies = true, CookieContainer = cookieContainer });

        var trade_rsp = await Client.GetTradeOfferAsync("1AA37B9ED9A6A46995ABC581E92A8978", "6275665902");

        if (trade_rsp.IsSuccessStatusCode)
        {
            var tradeInfo = JsonDocument.Parse(await trade_rsp.Content.ReadAsStringAsync()).RootElement.GetProperty("response").GetProperty("offer").Deserialize<TradeInfo>() ?? null;

        }
    }
}
