#if !(IOS || ANDROID)
using BD.Common8.Extensions;
using BD.SteamClient8.Helpers;
using BD.SteamClient8.Services.Abstractions.PInvoke;
using BD.SteamClient8.Services.PInvoke;
using System.Diagnostics;
using System.Extensions;
using System.Runtime.InteropServices;
using ValveKeyValue;

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
    public void SteamworksLocal()
    {
        TestContext.Out.WriteLine($"UserName: {Environment.UserName}");
        TestContext.Out.WriteLine($"UserInteractive: {Environment.UserInteractive}");
        TestContext.Out.WriteLine($"UserDomainName: {Environment.UserDomainName}");
        TestContext.Out.WriteLine($"ProcessPath: {Environment.ProcessPath}");
        TestContext.Out.WriteLine($"CurrentDirectory: {Environment.CurrentDirectory}");
        TestContext.Out.WriteLine($"OSArchitecture: {RuntimeInformation.OSArchitecture}");
        TestContext.Out.WriteLine($"ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}");
        TestContext.Out.WriteLine($"RuntimeIdentifier: {RuntimeInformation.RuntimeIdentifier}");
        TestContext.Out.WriteLine($"FrameworkDescription: {RuntimeInformation.FrameworkDescription}");
        TestContext.Out.WriteLine($"ISteamworksLocalApiService.IsSupported: {ISteamworksLocalApiService.IsSupported}");

        var init_exception = steamworksLocalApiService.Initialize(0, false);
        if (IsCI() && init_exception != null)
        {
            TestContext.Out.WriteLine(init_exception);
            return; // CI 中运行出现异常忽略
        }

        Assert.That(init_exception, Is.EqualTo(null));

        var steamId64 = steamworksLocalApiService.GetSteamId64();
        Assert.Multiple(() =>
        {
            Assert.That(steamId64, !Is.EqualTo(0L));
        });

        steamworksLocalApiService.OwnsApp(730);

        var countryOrRegion = steamworksLocalApiService.GetCountryOrRegionByIP();
        Assert.Multiple(() =>
        {
            Assert.That(countryOrRegion, Is.Not.Empty, countryOrRegion);
        });

        TestContext.Out.WriteLine(Serializable.SJSON(countryOrRegion, writeIndented: true));
    }

    /// <summary>
    /// Vdf 基准测试
    /// </summary>
    [Test]
    public void VdfBenchmark()
    {
        var steamDirPath = SteamPathHelper.GetSteamDirPath();
        if (string.IsNullOrEmpty(steamDirPath))
            return;

        const int numIterations = 10;
        string vdfStr = Path.Combine(steamDirPath, "config", "config.vdf");
        var sw = Stopwatch.StartNew();
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var k = kv.Deserialize(File.OpenRead(vdfStr));
        _ = k.GetHashCode();
        sw.Stop();
        TestContext.Out.WriteLine(
            $"ValveKeyValue (VDF)       : {sw.ElapsedMilliseconds / numIterations}ms, {sw.ElapsedTicks / numIterations}ticks average");
    }

    /// <summary>
    /// Vdf 值编辑测试
    /// </summary>
    [Ignore("unspport")]
    [Test]
    public void VdfValueEdit()
    {
        var steamDirPath = SteamPathHelper.GetSteamDirPath();
        if (string.IsNullOrEmpty(steamDirPath))
            return;

        string vdfStr = Path.Combine(steamDirPath, "config", "config.vdf");
        var v = VdfHelper.Read(vdfStr);
        if (v != null)
        {
            var kv = v["Software"]["valve"]["Steam"]["ipv6check_http_state"] as KVObjectValue<string>;
            kv.ThrowIsNull().Value = "bad1";
            TestContext.Out.WriteLine($"{v["Software"]["valve"]["Steam"]["ipv6check_http_state"]}");
            //VdfHelper.Write(vdfStr, v);
        }
    }

    /// <summary>
    /// 测试移除授权设备列表
    /// </summary>
    [Test]
    public void RemoveAuthorizedDeviceList()
    {
        var steamDirPath = SteamPathHelper.GetSteamDirPath();
        if (string.IsNullOrEmpty(steamDirPath))
            return;

        string vdfStr = Path.Combine(steamDirPath, "config", "config.vdf");
        var v = VdfHelper.Read(vdfStr);
        if (v?["AuthorizedDevice"] is KVCollectionValue authorizedDevices)
        {
            authorizedDevices.Remove("130741779");
            //v["AuthorizedDevice"] = authorizedDevices;
            foreach (var x in (KVCollectionValue)v["AuthorizedDevice"])
            {
                TestContext.Out.WriteLine($"{x.Name}   {x["description"]}");
            }
            //VdfHelper.Write(vdfStr, v);
        }

        TestContext.Out.WriteLine("OK");
    }

    /// <summary>
    /// 测试获取记住的用户列表
    /// </summary>
    [Test]
    public void GetRememberUserList()
    {
        var list_rsp = steamService.GetRememberUserList();
        Assert.Multiple(() =>
        {
            Assert.That(list_rsp, Is.Not.Null);
        });

        var list = list_rsp;
        list.ForEach(x =>
        {
            TestContext.Out.WriteLine($"{x.SteamId64}   {x.SteamID}");
        });
        Assert.That(list, Is.Not.Empty);
    }

    /// <summary>
    /// 测试获取下载的游戏列表
    /// </summary>
    [Test]
    public void GetDownloadingAppList()
    {
        var list_rsp = steamService.GetDownloadingAppList();

        Assert.Multiple(() =>
        {
            Assert.That(list_rsp, Is.Not.Null);
        });

        var list = list_rsp;
        list.ForEach(x =>
        {
            TestContext.Out.WriteLine($"{x.Name}   {x.AppId}");
        });
    }
}
#endif