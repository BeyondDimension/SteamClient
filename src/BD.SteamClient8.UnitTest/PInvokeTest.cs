#if !(IOS || ANDROID)
namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamServiceImpl"/> and <see cref="SteamworksLocalApiServiceImpl"/> 单元测试
/// </summary>
sealed class PInvokeTest : ServiceTestBase
{
    ISteamService steamService = null!;
    ISteamworksLocalApiService steamworksLocalApiService = null!;

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        steamworksLocalApiService = GetRequiredService<ISteamworksLocalApiService>();
        steamService = GetRequiredService<ISteamService>();
    }

    /// <summary>
    /// 测试本机库初始化
    /// </summary>
    [Test]
    public async Task SteamworksLocal()
    {
        var init_result = await steamworksLocalApiService.Initialize();
        Assert.That(init_result.IsSuccess);

        var steamId64 = await steamworksLocalApiService.GetSteamId64();
        Assert.Multiple(() =>
        {
            Assert.That(steamId64.IsSuccess);
            Assert.That(steamId64.Content, !Is.EqualTo(0L));
        });

        await steamworksLocalApiService.OwnsApps(730);

        var countryOrRegion = await steamworksLocalApiService.GetCountryOrRegionByIP();
        Assert.Multiple(() =>
        {
            Assert.That(countryOrRegion.IsSuccess);
            Assert.That(countryOrRegion.Content, Is.Not.Empty);
        });

        TestContext.WriteLine(Serializable.SJSON(countryOrRegion, writeIndented: true));
    }

    /// <summary>
    /// Vdf 基准测试
    /// </summary>
    [Test]
    public void VdfBenchmark()
    {
        var steamDirPath = ISteamService.SteamDirPath;
        if (string.IsNullOrEmpty(steamDirPath))
            return;

        const int numIterations = 10;
        string vdfStr = Path.Combine(steamDirPath, "config", "config.vdf");
        var sw = Stopwatch.StartNew();
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var k = kv.Deserialize(File.OpenRead(vdfStr));
        sw.Stop();
        TestContext.WriteLine(
            $"ValveKeyValue (VDF)       : {sw.ElapsedMilliseconds / numIterations}ms, {sw.ElapsedTicks / numIterations}ticks average");
    }

    /// <summary>
    /// Vdf 值编辑测试
    /// </summary>
    [Ignore("unspport")]
    [Test]
    public void VdfValueEdit()
    {
        var steamDirPath = ISteamService.SteamDirPath;
        if (string.IsNullOrEmpty(steamDirPath))
            return;

        string vdfStr = Path.Combine(steamDirPath, "config", "config.vdf");
        var v = VdfHelper.Read(vdfStr);
        if (v != null)
        {
            var kv = v["Software"]["valve"]["Steam"]["ipv6check_http_state"] as KVObjectValue<string>;
            kv.ThrowIsNull().Value = "bad1";
            TestContext.WriteLine($"{v["Software"]["valve"]["Steam"]["ipv6check_http_state"]}");
            //VdfHelper.Write(vdfStr, v);
        }
    }

    /// <summary>
    /// 测试移除授权设备列表
    /// </summary>
    [Test]
    public void RemoveAuthorizedDeviceList()
    {
        var steamDirPath = ISteamService.SteamDirPath;
        if (string.IsNullOrEmpty(steamDirPath))
            return;

        string vdfStr = Path.Combine(steamDirPath, "config", "config.vdf");
        var v = VdfHelper.Read(vdfStr);
        if (v["AuthorizedDevice"] is KVCollectionValue authorizedDevices)
        {
            authorizedDevices.Remove("130741779");
            //v["AuthorizedDevice"] = authorizedDevices;
            foreach (var x in (KVCollectionValue)v["AuthorizedDevice"])
            {
                TestContext.WriteLine($"{x.Name}   {x["description"]}");
            }
            //VdfHelper.Write(vdfStr, v);
        }

        TestContext.WriteLine("OK");
    }

    /// <summary>
    /// 测试获取记住的用户列表
    /// </summary>
    [Test]
    public async Task GetRememberUserList()
    {
        var list_rsp = await steamService.GetRememberUserList();
        Assert.Multiple(() =>
        {
            Assert.That(list_rsp.IsSuccess);
            Assert.That(list_rsp.Content, Is.Not.Null);
        });

        var list = list_rsp.Content;
        list.ForEach(x =>
        {
            TestContext.WriteLine($"{x.SteamId64}   {x.SteamID}");
        });
        Assert.That(list, Is.Not.Empty);
    }

    /// <summary>
    /// 测试获取下载的游戏列表
    /// </summary>
    [Test]
    public async Task GetDownloadingAppList()
    {
        var list_rsp = await steamService.GetDownloadingAppList();

        Assert.Multiple(() =>
        {
            Assert.That(list_rsp.IsSuccess);
            Assert.That(list_rsp.Content, Is.Not.Null);
        });

        var list = list_rsp.Content;
        list.ForEach(x =>
        {
            TestContext.WriteLine($"{x.Name}   {x.AppId}");
        });
        Assert.That(list, Is.Not.Empty);
    }
}
#endif