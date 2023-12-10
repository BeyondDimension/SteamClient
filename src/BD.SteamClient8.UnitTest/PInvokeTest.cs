#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamServiceImpl"/> and <see cref="SteamworksLocalApiServiceImpl"/> 单元测试
/// </summary>
sealed class PInvokeTest : ServiceTestBase
{
    ISteamService steamService = null!;
    ISteamworksLocalApiService steamworksLocalApiService = null!;

    /// <inheritdoc/>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<ISteamService, TestSteamServiceImpl>();
        services.TryAddSteamworksLocalApiService();
    }

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
    public void Test_SteamworksLocal()
    {
        var init_result = steamworksLocalApiService.Initialize();
        Assert.That(init_result);

        var steamId64 = steamworksLocalApiService.GetSteamId64();
        Assert.That(steamId64, !Is.EqualTo(0L));

        var isOwnsApp = steamworksLocalApiService.OwnsApps(730);
        Assert.That(isOwnsApp);

        var country = steamworksLocalApiService.GetIPCountry();
        Assert.That(country, Is.Not.Empty);
    }

    /// <summary>
    /// Vdf 基准测试
    /// </summary>
    [Test]
    public void TestVdfBenchmark()
    {
        var steamDirPath = steamService.SteamDirPath;
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
    public void TestVdfValueEdit()
    {
        var steamDirPath = steamService.SteamDirPath;
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
    public void TestRemoveAuthorizedDeviceList()
    {
        var steamDirPath = steamService.SteamDirPath;
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
    }

    /// <summary>
    /// 测试获取记住的用户列表
    /// </summary>
    [Test]
    public void TestGetRememberUserList()
    {
        var list = steamService.GetRememberUserList();
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
    public void TestGetDownloadingAppList()
    {
        var list = steamService.GetDownloadingAppList();
        list.ForEach(x =>
        {
            TestContext.WriteLine($"{x.Name}   {x.AppId}");
        });
        Assert.That(list, Is.Not.Empty);
    }

    sealed class TestSteamServiceImpl(ILoggerFactory loggerFactory) : SteamServiceImpl(loggerFactory)
    {
        public override ISteamConnectService Conn => throw new NotImplementedException();

        protected override string? StratSteamDefaultParameter => default;

        protected override bool IsRunSteamAdministrator => default;

        protected override Dictionary<uint, string?>? HideGameList => default;

        protected override string? GetString(string name) => default;
    }
}
#endif