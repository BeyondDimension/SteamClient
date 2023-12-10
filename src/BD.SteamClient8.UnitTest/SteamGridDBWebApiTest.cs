namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamGridDBWebApiServiceImpl"/> 单元测试
/// </summary>
sealed class SteamGridDBWebApiTest : ServiceTestBase
{
    ISteamGridDBWebApiServiceImpl steamGridDBWebApiService = null!;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamGridDBWebApiService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamGridDBWebApiService = GetRequiredService<ISteamGridDBWebApiServiceImpl>();
    }

    /// <summary>
    /// 测试通过 AppId 获取 SteamGridApp 信息
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    [TestCase(730)]
    [Test]
    public async Task TestGetSteamGridAppBySteamAppId(long appId)
    {
        var rsp = await steamGridDBWebApiService.GetSteamGridAppBySteamAppId(appId);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content?.Name, Is.Not.Empty);
        });

        var gridItems = await steamGridDBWebApiService.GetSteamGridItemsByGameId(rsp.Content.Id);

        Assert.That(gridItems, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(gridItems.IsSuccess);
            Assert.That(gridItems.Content, Is.Not.Empty);
        });
    }
}
