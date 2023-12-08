namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

/// <summary>
/// <see cref="SteamGridDBWebApiServiceImpl"/> 单元测试
/// </summary>
class SteamGridDBWebApiTest
{
    IServiceProvider service;

    ISteamGridDBWebApiServiceImpl SteamGridDB => service.GetRequiredService<ISteamGridDBWebApiServiceImpl>();

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.TryAddHttpPlatformHelper();
        services.AddFusilladeHttpClientFactory();
        services.AddLogging();
        services.AddSteamGridDBWebApiService();
        service = services.BuildServiceProvider();
    }

    [TestCase(730)]
    [Test]
    public async Task TestGetSteamGridAppBySteamAppId(long appId)
    {
        var rsp = await SteamGridDB.GetSteamGridAppBySteamAppId(appId);

        Assert.IsNotNull(rsp);
        Assert.IsTrue(rsp.IsSuccess);
        Assert.IsNotEmpty(rsp.Content.Name);

        var gridItems = await SteamGridDB.GetSteamGridItemsByGameId(rsp.Content.Id);

        Assert.IsNotNull(gridItems);
        Assert.IsTrue(gridItems.IsSuccess);
        Assert.IsTrue(gridItems.Content.Count > 0);
    }
}
