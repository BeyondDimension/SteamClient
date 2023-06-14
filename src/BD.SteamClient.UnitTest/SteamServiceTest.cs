#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.IO;
using ValveKeyValue;

namespace BD.SteamClient.UnitTest;

public sealed class SteamServiceTest
{
    IServiceProvider service;

    ISteamService Client => service.GetRequiredService<ISteamService>();

    static readonly bool IsCI = Environment.UserName.Contains("runner", StringComparison.OrdinalIgnoreCase); // DOTNET_ROOT: C:\Users\runneradmin\AppData\Local\Microsoft\dotnet

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<ISteamService, TestSteamServiceImpl>();
        service = services.BuildServiceProvider();
    }

    [Test]
    public void TestVdfBenchmark()
    {
        if (IsCI && string.IsNullOrEmpty(Client.SteamDirPath)) return;
        const int numIterations = 10;
        string vdfStr = Path.Combine(Client.SteamDirPath!, "config", "config.vdf");
        var sw = Stopwatch.StartNew();
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var k = kv.Deserialize(File.OpenRead(vdfStr));
        sw.Stop();
        TestContext.WriteLine($"ValveKeyValue (VDF)       : {sw.ElapsedMilliseconds / numIterations}ms, {sw.ElapsedTicks / numIterations}ticks average");
    }

    [Test]
    public void TestVdfValueEdit()
    {
        if (IsCI && string.IsNullOrEmpty(Client.SteamDirPath)) return;
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
        if (IsCI && string.IsNullOrEmpty(Client.SteamDirPath)) return;
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
        if (IsCI) return;
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
        if (IsCI) return;
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
}
#endif