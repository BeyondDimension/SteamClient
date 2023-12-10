namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamDbWebApiServiceImpl"/> 单元测试
/// </summary>
sealed class SteamDBWebApiTest : ServiceTestBase
{
    ISteamDbWebApiService steamDbWebApiService = null!;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSteamDbWebApiService();
    }

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamDbWebApiService = GetRequiredService<ISteamDbWebApiService>();
    }

    /// <summary>
    /// 测试获取用户详情
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    [TestCase(76561198425787706)]
    [Test]
    public async Task TestGetUserInfo(long steamId)
    {
        var rsp = await steamDbWebApiService.GetUserInfo(steamId);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content?.ProfileUrl, Is.Not.Empty);
        });
    }

    /// <summary>
    /// 测试批量获取用户详情
    /// </summary>
    /// <param name="steamIds"></param>
    /// <returns></returns>
#pragma warning disable CA1861 // 不要将常量数组作为参数
    [TestCase(new long[] { 76561198425787706, 76561198409610315 })]
#pragma warning restore CA1861 // 不要将常量数组作为参数
    [Test]
    public async Task TestGetUserInfos(long[] steamIds)
    {
        var rsp = await steamDbWebApiService.GetUserInfo(steamIds);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content?.Count, Is.Not.Empty);
        });
    }

    /// <summary>
    /// 测试通过 AppId 获取游戏详情
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    [TestCase(730)]
    [Test]
    public async Task GetAppInfo(int appId)
    {
        var rsp = await steamDbWebApiService.GetAppInfo(appId);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content?.Name, Is.Not.Empty);
        });
    }
}
