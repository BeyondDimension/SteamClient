namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamworksWebApiServiceImpl"/> 单元测试
/// </summary>
sealed class SteamworksWebApiTest : ServiceTestBase
{
    ISteamworksWebApiService steamworksWebApiService = null!;

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamworksWebApiService = GetRequiredService<ISteamworksWebApiService>();
    }

    /// <summary>
    /// 测试获取所有游戏 JSON 字符串
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetAllSteamAppsString()
    {
        var rsp = await steamworksWebApiService.GetAllSteamAppsString();

        Assert.That(rsp, Is.Not.EqualTo(null));
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content, Is.Not.Empty);
        });

        TestContext.WriteLine(rsp.Content.Length);
    }

    /// <summary>
    /// 测试获取所有 Steam 游戏列表
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetAllSteamAppList()
    {
        var rsp = await steamworksWebApiService.GetAllSteamAppList();

        Assert.That(rsp, Is.Not.EqualTo(null));
        Assert.That(rsp.IsSuccess && rsp.Content != null && rsp.Content.Count > 0);

        TestContext.WriteLine(Serializable.SJSON(rsp.Content!.Take(3), writeIndented: true));
    }

    /// <summary>
    /// 测试获取 Steam 个人资料
    /// </summary>
    /// <param name="steamId64"></param>
    /// <returns></returns>
    [TestCase(76561199494800019L)]
    [Test]
    public async Task GetUserInfo(long steamId64)
    {
        var rsp = await steamworksWebApiService.GetUserInfo(steamId64);
        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));

        Assert.That(rsp, Is.Not.EqualTo(null));
        Assert.Multiple(() =>
        {
            Assert.That(rsp.IsSuccess);
            Assert.That(rsp.Content?.AvatarFull, Is.Not.Empty);
        });

        TestContext.WriteLine(Serializable.SJSON(rsp, writeIndented: true));
    }
}
