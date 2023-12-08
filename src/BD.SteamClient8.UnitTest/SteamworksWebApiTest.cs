namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
/// <summary>
/// <see cref="SteamworksWebApiServiceImpl"/> 单元测试
/// </summary>
class SteamworksWebApiTest
{
    IServiceProvider service;

    ISteamworksWebApiService Steamworks => service.GetRequiredService<ISteamworksWebApiService>();

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddFusilladeHttpClientFactory();
        services.AddLogging();
        services.AddSteamworksWebApiService();
        service = services.BuildServiceProvider();
    }

    [Test]
    public async Task TestGetAllSteamAppsString()
    {
        var rsp = await Steamworks.GetAllSteamAppsString();

        Assert.IsNotNull(rsp);
        Assert.IsTrue(rsp.IsSuccess);
        Assert.IsNotEmpty(rsp.Content);
    }

    [Test]
    public async Task TestGetAllSteamAppList()
    {
        var rsp = await Steamworks.GetAllSteamAppList();

        Assert.IsNotNull(rsp);
        Assert.IsTrue(rsp.IsSuccess && rsp.Content.Count > 0);
    }

    [TestCase(76561198425787706L)]
    [Test]
    public async Task GetUserInfo(long steamId64)
    {
        var rsp = await Steamworks.GetUserInfo(steamId64);

        Assert.IsNotNull(rsp);
        Assert.IsTrue(rsp.IsSuccess);
        Assert.IsNotEmpty(rsp.Content.AvatarFull);
    }
}
