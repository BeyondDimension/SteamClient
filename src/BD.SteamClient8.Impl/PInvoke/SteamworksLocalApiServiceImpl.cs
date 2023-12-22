#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using static BD.SteamClient8.Services.WebApi.ISteamworksLocalApiService;
using SAMAPIClient = SAM.API.Client;
using UserStatsReceived = SAM.API.Types.UserStatsReceived;
using UserStatsReceivedCallback = SAM.API.Callbacks.UserStatsReceived;
#endif

namespace BD.SteamClient8.Impl.PInvoke;

/// <inheritdoc cref="ISteamworksLocalApiService "/> Steamworks 本地 API 服务实现
public sealed class SteamworksLocalApiServiceImpl : ISteamworksLocalApiService
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// <see cref="SAMAPIClient"/> 实例
    /// </summary>
    public SAMAPIClient SteamClient { get; }

    UserStatsReceivedCallback? UserStatsReceivedCallback;

    /// <summary>
    /// 初始化 <see cref="SteamworksLocalApiServiceImpl"/> 类的新实例
    /// </summary>
    public SteamworksLocalApiServiceImpl()
    {
        Steam.GetInstallPathDelegate = SteamServiceImpl.GetSteamDynamicLinkLibraryPath;
        SteamClient = new SAMAPIClient();
    }

    /// <inheritdoc/>
    public bool IsSupported => RuntimeInformation.ProcessArchitecture.IsX86OrX64();

    /// <inheritdoc/>
    public void DisposeSteamClient()
    {
        SteamClient.Dispose();
    }

    /// <inheritdoc/>
    public bool Initialize()
    {
        try
        {
            SteamClient.Initialize(0);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "Initialize fail.");
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(int appid)
    {
        try
        {
            SteamClient.Initialize(appid);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "Initialize fail.");
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public long GetSteamId64()
    {
        if (SteamClient.SteamUser == null)
            return 0L;
        return (long)SteamClient.SteamUser.GetSteamId();
    }

    /// <inheritdoc/>
    public bool OwnsApps(uint appid)
    {
        if (SteamClient.SteamApps008 == null)
            return false;
        return SteamClient.SteamApps008.IsSubscribedApp(appid);
    }

    /// <inheritdoc/>
    public IEnumerable<SteamApp> OwnsApps(IEnumerable<SteamApp> apps)
    {
        if (!apps.Any_Nullable())
            return new List<SteamApp>();
        if (SteamClient.SteamApps008 == null || SteamClient.SteamApps001 == null)
            return apps;
        return apps.Where(f => SteamClient.SteamApps008.IsSubscribedApp(f.AppId))
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
                s.State = IsAppInstalled(s.AppId) ? 4 : s.State;
                s.InstalledDir = string.IsNullOrEmpty(s.InstalledDir) ? GetAppInstallDir(s.AppId) : s.InstalledDir;
                s.Type = Enum.TryParse<SteamAppType>(GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppType.Unknown;
                return s;
            });
    }

    /// <inheritdoc/>
    public string GetAppData(uint appid, string key)
    {
        return SteamClient.SteamApps001.GetAppData(appid, key);
    }

    /// <inheritdoc/>
    public bool IsSteamChinaLauncher()
    {
        return SteamClient.SteamUtils.IsSteamChinaLauncher();
    }

    /// <inheritdoc/>
    public bool IsSteamInBigPictureMode()
    {
        return SteamClient.SteamUtils.IsSteamInBigPictureMode();
    }

    /// <inheritdoc/>
    public uint GetSecondsSinceAppActive()
    {
        return SteamClient.SteamUtils.GetSecondsSinceAppActive();
    }

    /// <inheritdoc/>
    public uint GetServerRealTime()
    {
        return SteamClient.SteamUtils.GetServerRealTime();
    }

    /// <inheritdoc/>
    public bool IsAppInstalled(uint appid)
    {
        return SteamClient.SteamApps008.IsAppInstalled(appid);
    }

    /// <inheritdoc/>
    public string GetAppInstallDir(uint appid)
    {
        return SteamClient.SteamApps008.GetAppInstallDir(appid);
    }

    /// <inheritdoc/>
    public string GetIPCountry()
    {
        return SteamClient.SteamUtils.GetIPCountry();
    }

    /// <inheritdoc/>
    public string GetCurrentGameLanguage()
    {
        return SteamClient.SteamApps008.GetCurrentGameLanguage();
    }

    /// <inheritdoc/>
    public string GetAvailableGameLanguages()
    {
        return SteamClient.SteamApps008.GetAvailableGameLanguages();
    }

    #region SteamUserStats

    /// <inheritdoc/>
    public bool GetStatValue(string name, out int value)
    {
        return SteamClient.SteamUserStats.GetStatValue(name, out value);
    }

    /// <inheritdoc/>
    public bool GetStatValue(string name, out float value)
    {
        return SteamClient.SteamUserStats.GetStatValue(name, out value);
    }

    /// <inheritdoc/>
    public bool GetAchievementState(string name, out bool isAchieved)
    {
        return SteamClient.SteamUserStats.GetAchievementState(name, out isAchieved);
    }

    /// <inheritdoc/>
    public bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime)
    {
        return SteamClient.SteamUserStats.GetAchievementAndUnlockTime(name, out isAchieved, out unlockTime);
    }

    /// <inheritdoc/>
    public bool GetAchievementAchievedPercent(string name, out float percent)
    {
        return SteamClient.SteamUserStats.GetAchievementAchievedPercent(name, out percent);
    }

    /// <inheritdoc/>
    public void AddUserStatsReceivedCallback(Action<IUserStatsReceived> action)
    {
        UserStatsReceivedCallback = SteamClient.CreateAndRegisterCallback<UserStatsReceivedCallback>();
        UserStatsReceivedCallback.OnRun += value =>
        {
            var valueWrapper = new UserStatsReceivedWrapper(value);
            action(valueWrapper);
        };
    }

    sealed class UserStatsReceivedWrapper : IUserStatsReceived
    {
        readonly SAM.API.Types.UserStatsReceived userStatsReceived;

        public UserStatsReceivedWrapper(UserStatsReceived userStatsReceived)
        {
            this.userStatsReceived = userStatsReceived;
        }

        public ulong GameId => userStatsReceived.GameId;

        public int Result => userStatsReceived.Result;
    }

    /// <inheritdoc/>
    public bool RequestCurrentStats()
    {
        return SteamClient.SteamUserStats.RequestCurrentStats();
    }

    /// <inheritdoc/>
    public bool ResetAllStats(bool achievementsToo)
    {
        return SteamClient.SteamUserStats.ResetAllStats(achievementsToo);
    }

    /// <inheritdoc/>
    public bool SetAchievement(string name, bool state)
    {
        return SteamClient.SteamUserStats.SetAchievement(name, state);
    }

    /// <inheritdoc/>
    public bool SetStatValue(string name, int value)
    {
        return SteamClient.SteamUserStats.SetStatValue(name, value);
    }

    /// <inheritdoc/>
    public bool SetStatValue(string name, float value)
    {
        return SteamClient.SteamUserStats.SetStatValue(name, value);
    }

    /// <inheritdoc/>
    public bool StoreStats()
    {
        return SteamClient.SteamUserStats.StoreStats();
    }

    /// <inheritdoc/>
    public void RequestGlobalAchievementPercentages()
    {
        SteamClient.SteamUserStats.RequestGlobalAchievementPercentages();
    }
    #endregion

    /// <inheritdoc/>
    public void RunCallbacks(bool server)
    {
        SteamClient.RunCallbacks(server);
    }

    #region SteamRemoteStorage

    /// <inheritdoc/>
    public bool GetCloudArchiveQuota(out ulong totalBytes, out ulong availableBytes)
    {
        return SteamClient.SteamRemoteStorage.GetQuota(out totalBytes, out availableBytes);
    }

    /// <inheritdoc/>
    public List<SteamRemoteFile>? GetCloudArchiveFiles()
    {
        List<SteamRemoteFile> files = [];

        var fileCount = SteamClient.SteamRemoteStorage.GetFileCount();
        for (var i = 0; i < fileCount; ++i)
        {
            var name = SteamClient.SteamRemoteStorage.GetFileNameAndSize(i, out var length);
            var file = new SteamRemoteFile(name, length, SteamClient.SteamRemoteStorage.FileExists(name),
                SteamClient.SteamRemoteStorage.FilePersisted(name), SteamClient.SteamRemoteStorage.GetFileTimestamp(name))
            {
                SyncPlatforms = (SteamKit2ERemoteStoragePlatform)SteamClient.SteamRemoteStorage.GetSyncPlatforms(name),
            };
            files.Add(file);
        }

        return files;
    }

    /// <inheritdoc/>
    public int FileRead(string name, byte[] buffer)
    {
        return SteamClient.SteamRemoteStorage.FileRead(name, buffer);
    }

    /// <inheritdoc/>
    public bool FileWrite(string name, byte[] buffer)
    {
        return SteamClient.SteamRemoteStorage.FileWrite(name, buffer);
    }

    /// <inheritdoc/>
    public bool FileForget(string name)
    {
        return SteamClient.SteamRemoteStorage.FileForget(name);
    }

    /// <inheritdoc/>
    public bool FileDelete(string name)
    {
        return SteamClient.SteamRemoteStorage.FileDelete(name);
    }

    #endregion

#endif
}