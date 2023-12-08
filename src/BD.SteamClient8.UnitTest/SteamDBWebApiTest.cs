namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

/// <summary>
/// <see cref="SteamDbWebApiServiceImpl"/> 单元测试
/// </summary>
public class SteamDBWebApiTest
{
    IServiceProvider service;

    ISteamDbWebApiService SteamDB => service.GetRequiredService<ISteamDbWebApiService>();

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.TryAddHttpPlatformHelper();
        services.AddFusilladeHttpClientFactory();
        services.AddLogging();
        services.AddSteamDbWebApiService();
        service = services.BuildServiceProvider();
    }

    [TestCase(76561198425787706)]
    [Test]
    public async Task TestGetUserInfo(long steamId)
    {
        var rsp = await SteamDB.GetUserInfo(steamId);

        Assert.IsNotNull(rsp);
        Assert.IsTrue(rsp.IsSuccess);
        Assert.IsNotEmpty(rsp.Content.ProfileUrl);
    }

    [TestCase(new long[] { 76561198425787706, 76561198409610315 })]
    [Test]
    public async Task TestGetUserInfos(long[] steamIds)
    {
        var rsp = await SteamDB.GetUserInfo(steamIds);

        Assert.IsNotNull(rsp);
        Assert.IsTrue(rsp.IsSuccess);
        Assert.IsTrue(rsp.Content.Count > 0);
    }

    [TestCase(730)]
    [Test]
    public async Task GetAppInfo(int appId)
    {
        var rsp = await SteamDB.GetAppInfo(appId);

        Assert.IsNotNull(rsp);
        Assert.IsTrue(rsp.IsSuccess);
        Assert.IsNotEmpty(rsp.Content.Name);
    }
}
