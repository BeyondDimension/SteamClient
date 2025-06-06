using BD.SteamClient8.Services.Abstractions.WebApi;
using BD.SteamClient8.Services.WebApi;

namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamGridDBWebApiServiceImpl"/> 单元测试
/// </summary>
sealed class SteamGridDBWebApiTest : ServiceTestBase
{
    ISteamGridDBWebApiServiceImpl steamGridDBWebApiService = null!;

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
    public async Task GetSteamGridAppBySteamAppId(long appId)
    {
        var rsp = await steamGridDBWebApiService.GetSteamGridAppBySteamAppId(appId);

        Assert.That(rsp, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rsp?.Name, Is.Not.Empty);
        });

        var gridItems = await steamGridDBWebApiService.GetSteamGridItemsByGameId(rsp.Id);

        Assert.That(gridItems, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(gridItems, Is.Not.Empty);
        });

        TestContext.Out.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
        TestContext.Out.WriteLine(Serializable.SJSON(gridItems, writeIndented: true));
    }
}
