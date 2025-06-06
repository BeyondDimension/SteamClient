#if !(IOS || ANDROID || MACCATALYST)
using BD.Common8.Helpers;
using BD.Common8.Models;
using BD.SteamClient8.Enums.WebApi.SteamApps;
using BD.SteamClient8.Helpers;
using BD.SteamClient8.Models.PInvoke;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;
using BD.SteamClient8.Services.Abstractions.PInvoke;
using System.Extensions;
using System.Runtime.CompilerServices;
using static BD.SteamClient8.Services.Abstractions.PInvoke.ISteamworksLocalApiService;
using SAMAPIClient = SAM.API.Client;
using UserStatsReceivedCallback = SAM.API.Callbacks.UserStatsReceived;

namespace BD.SteamClient8.Services.PInvoke;

/// <inheritdoc cref="ISteamworksLocalApiService "/> Steamworks 本地 API 服务实现
sealed partial class SteamworksLocalApiServiceImpl : LazySAMAPIClient, ISteamworksLocalApiService
{
    UserStatsReceivedCallback? UserStatsReceivedCallback;

    /// <summary>
    /// 初始化 <see cref="SteamworksLocalApiServiceImpl"/> 类的新实例
    /// </summary>
    public SteamworksLocalApiServiceImpl()
    {
    }

    /// <inheritdoc/>
    public Exception? Initialize(int appid, bool processPathIsReadOnly)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            try
            {
                stmClient.Initialize(appid, processPathIsReadOnly);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "Initialize fail, appid: {appid}", appid);
                return ex;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public ulong GetSteamId64()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var steamUser = stmClient.SteamUser;
            if (steamUser == null)
                return default;
            var result = steamUser.GetSteamId();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool OwnsApp(uint appid)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamApps008 != null && stmClient.SteamApps008.IsSubscribedApp(appid);
            return result;
        }
        return false;
    }

    /// <inheritdoc/>
    public List<SteamApp> OwnsApps(List<SteamApp> apps)
    {
        var stmClient = SteamClient;
        List<SteamApp> ownsApps = new();
        if (stmClient != null)
        {
            if (!apps.Any_Nullable())
            {
            }
            else if (stmClient.SteamApps008 == null || stmClient.SteamApps001 == null)
            {
                ownsApps = apps;
            }
            else
            {
                ownsApps = [.. apps.Where(f => stmClient.SteamApps008.IsSubscribedApp(f.AppId))
                .OrderBy(s => s.Name).Select((s) =>
                {
                    ////Index = i + 1,
                    //AppId = s.AppId,
                    //Name = s.Name,
                    //Type = Enum.TryParse<SteamAppType>(GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppType.Unknown,
                    ////Icon = GetAppData(s.AppId, "icon"),
                    ////Logo = GetAppData(s.AppId, "logo"),
                    //InstalledDir = GetAppInstallDir(s.AppId),
                    //IsInstalled = IsAppInstalled(s.AppId)
                    //s.IsInstalled = IsAppInstalled(s.AppId);
                    //s.InstalledDir = GetAppInstallDir(s.AppId);
                    s.State = stmClient.SteamApps008.IsAppInstalled(s.AppId) ? 4 : s.State;
                    s.InstalledDir = string.IsNullOrEmpty(s.InstalledDir) ? GetAppInstallDir(s.AppId) : s.InstalledDir;
                    s.Type = Enum.TryParse<SteamAppType>(GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppType.Unknown;
                    return s;
                })];
            }
        }
        return ownsApps;
    }

    /// <inheritdoc/>
    public string? GetAppData(uint appid, string key)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamApps001.GetAppData(appid, key);
            return result;
        }
        return null;
    }

    /// <inheritdoc/>
    public bool IsSteamChinaLauncher()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUtils.IsSteamChinaLauncher();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool IsSteamInBigPictureMode()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUtils.IsSteamInBigPictureMode();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public uint GetSecondsSinceAppActive()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUtils.GetSecondsSinceAppActive();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public uint GetServerRealTime()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUtils.GetServerRealTime();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool IsAppInstalled(uint appid)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamApps008.IsAppInstalled(appid);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public string? GetAppInstallDir(uint appid)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamApps008.GetAppInstallDir(appid);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public string? GetCountryOrRegionByIP()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUtils.GetIPCountry();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public string? GetCurrentGameLanguage()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamApps008.GetCurrentGameLanguage();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public string? GetAvailableGameLanguages()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamApps008.GetAvailableGameLanguages();
            return result;
        }
        return default;
    }

    #region SteamUserStats

    /// <inheritdoc/>
    public int? GetStatValueInt(string name)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.GetStatValue(name, out int value);
            return result ? value : null;
        }
        return default;
    }

    /// <inheritdoc/>
    public float? GetStatValueFloat(string name)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.GetStatValue(name, out float value);
            return result ? value : null;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool? GetAchievementState(string name)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.GetAchievementState(name, out bool isAchieved);
            return result ? isAchieved : null;
        }
        return default;
    }

    /// <inheritdoc/>
    public AchievementAndUnlockTimeResult? GetAchievementAndUnlockTime(string name)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.GetAchievementAndUnlockTime(name, out var isAchieved, out var unlockTime);
            return result ? new AchievementAndUnlockTimeResult
            {
                IsAchieved = isAchieved,
                UnlockTime = unlockTime,
            } : null;
        }
        return null;
    }

    /// <inheritdoc/>
    public float? GetAchievementAchievedPercent(string name)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.GetAchievementAchievedPercent(name, out var percent);
            return result ? percent : null;
        }
        return null;
    }

    public event Action<UserStatsReceivedResult>? OnUserStatsReceived;

    void UserStatsReceivedCallbackOnRun(global::SAM.API.Types.UserStatsReceived r)
    {
        var d = OnUserStatsReceived;
        if (d != null)
        {
            UserStatsReceivedResult r2 = new()
            {
                GameId = r.GameId,
                Result = r.Result,
            };
            d.Invoke(r2);
        }
    }

    /// <inheritdoc/>
    public void RegisterUserStatsReceivedCallback()
    {
        if (UserStatsReceivedCallback == null)
        {
            var stmClient = SteamClient;
            if (stmClient != null)
            {
                UserStatsReceivedCallback = stmClient.CreateAndRegisterCallback<UserStatsReceivedCallback>();
                UserStatsReceivedCallback.OnRun += UserStatsReceivedCallbackOnRun;
            }
        }
    }

    /// <inheritdoc/>
    public bool RequestCurrentStats()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.RequestCurrentStats();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool ResetAllStats(bool achievementsToo)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.ResetAllStats(achievementsToo);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool SetAchievement(string name, bool state)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.SetAchievement(name, state);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool SetStatInt32Value(string name, int value)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.SetStatValue(name, value);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool SetStatSingleValue(string name, float value)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.SetStatValue(name, value);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool StoreStats()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.StoreStats();
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public nint RequestGlobalAchievementPercentages()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamUserStats.RequestGlobalAchievementPercentages();
            return result;
        }
        return default;
    }

    #endregion

    /// <inheritdoc/>
    public void RunCallbacks(bool server)
    {
        var stmClient = SteamClient;
        stmClient?.RunCallbacks(server);
    }

    #region SteamRemoteStorage

    /// <inheritdoc/>
    public CloudArchiveQuotaResult? GetCloudArchiveQuota()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamRemoteStorage.GetQuota(out var totalBytes, out var availableBytes);
            return result ? new CloudArchiveQuotaResult
            {
                AvailableBytes = availableBytes,
                TotalBytes = totalBytes,
            } : default;
        }
        return default;
    }

    /// <inheritdoc/>
    public SteamRemoteFile[]? GetCloudArchiveFiles()
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var fileCount = stmClient.SteamRemoteStorage.GetFileCount();
            var files = new SteamRemoteFile[fileCount];
            for (var i = 0; i < fileCount; ++i)
            {
                var name = stmClient.SteamRemoteStorage.GetFileNameAndSize(i, out var length) ?? "";
                files[i] = new SteamRemoteFile(name, length, stmClient.SteamRemoteStorage.FileExists(name),
                    stmClient.SteamRemoteStorage.FilePersisted(name), stmClient.SteamRemoteStorage.GetFileTimestamp(name))
                {
                    SyncPlatforms = unchecked((SteamKit2.ERemoteStoragePlatform)stmClient.SteamRemoteStorage.GetSyncPlatforms(name)),
                };
            }
            return files;
        }
        return default;
    }

    /// <inheritdoc/>
    public int FileRead(string name, byte[] buffer)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamRemoteStorage.FileRead(name, buffer);
            return result;
        }
        return -1;
    }

    /// <inheritdoc/>
    public byte[]? FileRead(string name, long length)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            byte[] buffer = new byte[length];
            var result = stmClient.SteamRemoteStorage.FileRead(name, buffer);
            if (result != 0)
            {
                return buffer;
            }
        }
        return default;
    }

    /// <inheritdoc/>
    public bool FileWrite(string name, byte[] buffer)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamRemoteStorage.FileWrite(name, buffer);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool FileForget(string name)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamRemoteStorage.FileForget(name);
            return result;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool FileDelete(string name)
    {
        var stmClient = SteamClient;
        if (stmClient != null)
        {
            var result = stmClient.SteamRemoteStorage.FileDelete(name);
            return result;
        }
        return default;
    }

    #endregion

}

partial class SteamworksLocalApiServiceImpl : IDisposable
{
    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                if (UserStatsReceivedCallback != null)
                {
                    UserStatsReceivedCallback.OnRun -= UserStatsReceivedCallbackOnRun;
                }
                DisposeSteamClient();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            UserStatsReceivedCallback = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

abstract class LazySAMAPIClient
{
    protected bool disposedValue;

    SAMAPIClient? steamClient;

    readonly Lock lockGet = new();

    /// <summary>
    /// <see cref="SAMAPIClient"/> 实例
    /// </summary>
    internal SAMAPIClient? SteamClient
    {
        get
        {
            if (!IsSupported)
            {
                return null;
            }
            if (disposedValue)
            {
                return null;
            }
            lock (lockGet)
            {
                if (steamClient == null)
                {
                    if (!global::SAM.API.Steam.IsSetCustomPathDelegate)
                    {
                        global::SAM.API.Steam.GetInstallPathDelegate = SteamPathHelper.GetSteamDirPath;
                        global::SAM.API.Steam.GetSteamClientNativeLibraryPathDelegate = SteamPathHelper.GetSteamClientNativeLibraryPath;
                    }
                    steamClient = new SAMAPIClient();
                }
                return steamClient;
            }
        }
    }

    public void DisposeSteamClient()
    {
        if (steamClient != null)
        {
            steamClient.Dispose();
            steamClient = null;
        }
    }
}

public static class SteamworksLocalApiServiceImplExtensions
{
    public static SAMAPIClient? GetSteamClient(this ISteamworksLocalApiService service)
    {
        if (service is LazySAMAPIClient i)
        {
            return i.SteamClient;
        }
        return null;
    }
}
#endif