namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
/// <summary>
/// <see cref="SteamServiceImpl"/> and <see cref="SteamworksLocalApiServiceImpl"/> 单元测试
/// </summary>
public class PInvokeTest
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    IServiceProvider Service;

    ISteamworksLocalApiService SteamWorksLocal;

    ISteamService Client;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<ISteamService, TestSteamServiceImpl>();
        services.AddFusilladeHttpClientFactory();
        services.TryAddSteamworksLocalApiService();

        Service = services.BuildServiceProvider();
        SteamWorksLocal = Service.GetRequiredService<ISteamworksLocalApiService>();
        Client = Service.GetRequiredService<ISteamService>();
    }

    [Test]
    public void Test_SteamworksLocal()
    {
        var init_result = SteamWorksLocal.Initialize();

        Assert.IsTrue(init_result);

        var steamId64 = SteamWorksLocal.GetSteamId64();

        Assert.That(steamId64, !Is.EqualTo(0L));

        var isOwnsApp = SteamWorksLocal.OwnsApps(730);

        Assert.IsTrue(isOwnsApp);

        var country = SteamWorksLocal.GetIPCountry();

        Assert.IsNotEmpty(country);
    }

    [Test]
    public void TestVdfBenchmark()
    {
        if (string.IsNullOrEmpty(Client.SteamDirPath)) return;
        const int numIterations = 10;
        string vdfStr = Path.Combine(Client.SteamDirPath!, "config", "config.vdf");
        var sw = Stopwatch.StartNew();
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var k = kv.Deserialize(File.OpenRead(vdfStr));
        sw.Stop();
        TestContext.WriteLine($"ValveKeyValue (VDF)       : {sw.ElapsedMilliseconds / numIterations}ms, {sw.ElapsedTicks / numIterations}ticks average");
    }

    [Ignore("unspport")]
    [Test]
    public void TestVdfValueEdit()
    {
        if (string.IsNullOrEmpty(Client.SteamDirPath)) return;
        string vdfStr = Path.Combine(Client.SteamDirPath!, "config", "config.vdf");
        var v = VdfHelper.Read(vdfStr);
        if (v != null)
        {
            var kv = v["Software"]["valve"]["Steam"]["ipv6check_http_state"] as KVObjectValue<string>;
            kv.Value = "bad1";
            TestContext.WriteLine($"{v["Software"]["valve"]["Steam"]["ipv6check_http_state"]}");
            //VdfHelper.Write(vdfStr, v);
        }
    }

    [Test]
    public void TestRemoveAuthorizedDeviceList()
    {
        if (string.IsNullOrEmpty(Client.SteamDirPath)) return;
        string vdfStr = Path.Combine(Client.SteamDirPath!, "config", "config.vdf");
        var v = VdfHelper.Read(vdfStr);
        var authorizedDevices = v["AuthorizedDevice"] as KVCollectionValue;
        if (authorizedDevices != null)
        {
            authorizedDevices.Remove("130741779");
            //v["AuthorizedDevice"] = authorizedDevices;
            foreach (var x in v["AuthorizedDevice"] as KVCollectionValue)
            {
                TestContext.WriteLine($"{x.Name}   {x["description"]}");
            }
            //VdfHelper.Write(vdfStr, v);
        }
    }

    [Test]
    public void TestGetRememberUserList()
    {
        var list = Client.GetRememberUserList();
        list.ForEach(x =>
        {
            TestContext.WriteLine($"{x.SteamId64}   {x.SteamID}");
        });
        Assert.True(list.Any_Nullable());
    }

    [Test]
    public void TestGetDownloadingAppList()
    {
        var list = Client.GetDownloadingAppList();
        list.ForEach(x =>
        {
            TestContext.WriteLine($"{x.Name}   {x.AppId}");
        });
        Assert.True(list.Any_Nullable());
    }

    sealed class TestSteamServiceImpl : SteamServiceImpl
    {
        public TestSteamServiceImpl(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public override ISteamConnectService Conn => throw new NotImplementedException();

        protected override string? StratSteamDefaultParameter => default;

        protected override bool IsRunSteamAdministrator => default;

        protected override Dictionary<uint, string?>? HideGameList => default;

        protected override string? GetString(string name) => default;
    }
#endif
}
