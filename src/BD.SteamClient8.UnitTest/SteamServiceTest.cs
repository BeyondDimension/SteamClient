using BD.SteamClient8.Enums.WebApi.SteamApps;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;
using BD.SteamClient8.Services.Abstractions.PInvoke;
using BD.SteamClient8.Services.PInvoke;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BD.SteamClient8.UnitTest;

/// <summary>
/// <see cref="SteamServiceImpl"/> 单元测试
/// </summary>
sealed class SteamServiceTest : ServiceTestBase
{
    SteamServiceImpl s = null!;
    JsonSerializerOptions opt = null!;

    /// <inheritdoc/>
    [SetUp]
    public override async ValueTask Setup()
    {
        await base.Setup();

        s = (SteamServiceImpl)GetRequiredService<ISteamService>();
        opt = new(DefaultJsonSerializerContext_.Default.Options)
        {
            WriteIndented = true,
        };
    }

    void AppendLine(StringBuilder b, string key, object? obj)
    {
        b.Append(key);
        b.AppendLine(": ");
        b.AppendLine(JsonSerializer.Serialize(obj, opt));
    }

    [Test]
    public async Task GetInfo()
    {
        var isOK = true;
        StringBuilder b = new();

        try
        {
            b.AppendLine($"SteamDirPath: {s.SteamDirPath}");
            b.AppendLine($"SteamProgramPath: {s.SteamProgramPath}");
            b.AppendLine($"IsRunningSteamProcess: {s.IsRunningSteamProcess}");
            b.AppendLine($"SteamProcessPid: {s.GetSteamProcessPid()}");
            b.AppendLine($"IsSteamChinaLauncher: {await s.IsSteamChinaLauncherAsync()}");
            b.AppendLine($"LastLoginUserName: {s.GetLastLoginUserName()}");
            AppendLine(b, "RememberUserList", s.GetRememberUserList()?.Take(3).ToArray());
            AppendLine(b, "AuthorizedDeviceList", s.GetAuthorizedDeviceList());
            IEnumerable<SteamApp>? appinfos;
            AppendLine(b, "AppInfos", appinfos = (await s.GetAppInfos())?.Take(3)?.ToArray());
            AppendLine(b, "ModifiedApps", s.GetModifiedApps());
            var libCacheTypes = Enum.GetValues<LibCacheType>();
            var testAppId = appinfos?.FirstOrDefault()?.AppId ?? 730;
            foreach (var libCacheType in libCacheTypes)
            {
                var filePath = s.GetAppImageFilePath(testAppId, libCacheType, false);
                b.AppendLine($"AppImageFilePath_{libCacheType}: {filePath}");
            }
            AppendLine(b, "DownloadingAppList", s.GetDownloadingAppList());
        }
        catch (Exception ex)
        {
            b.AppendLine($"Exception: {ex}");
            isOK = false;
        }

        TestContext.Out.WriteLine(b);
        Assert.That(isOK, Is.True, "获取 Steam 相关信息失败");
    }

    //[Test]
    //public void TODO()
    //{
    //    // TryKillSteamProcessAsync
    //    // StartSteam
    //    // StartSteamWithDefaultParameter
    //    // ShutdownSteamAsync
    //    // UpdateAuthorizedDeviceListAsync
    //    // RemoveAuthorizedDeviceListAsync
    //    // SetSteamCurrentUserAsync
    //    // SetPersonaStateAsync
    //    // DeleteLocalUserDataAsync
    //    // UpdateLocalUserDataAsync
    //    // StartWatchLocalUserDataChange
    //    // SaveAppInfosToSteam
    //    // GetAppImageFilePath
    //    // SaveAppImageToSteamFile
    //    // StartWatchSteamDownloading
    //    // StopWatchSteamDownloading
    //}
}
