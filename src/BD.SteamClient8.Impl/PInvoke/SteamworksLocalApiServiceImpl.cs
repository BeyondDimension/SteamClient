#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using static BD.SteamClient8.Services.PInvoke.ISteamworksLocalApiService;
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
    public Task<ApiRspImpl> DisposeSteamClient(CancellationToken cancellationToken = default)
    {
        SteamClient.Dispose();

        return Task.FromResult(ApiRspHelper.Ok());
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> Initialize(CancellationToken cancellationToken = default)
    {
        try
        {
            SteamClient.Initialize(0);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "Initialize fail.");
            return Task.FromResult(ApiRspHelper.Fail());
        }
        return Task.FromResult(ApiRspHelper.Ok());
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> Initialize(int appid, CancellationToken cancellationToken = default)
    {
        try
        {
            SteamClient.Initialize(appid);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "Initialize fail.");
            return Task.FromResult(ApiRspHelper.Fail());
        }
        return Task.FromResult(ApiRspHelper.Ok());
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<long>> GetSteamId64(CancellationToken cancellationToken = default)
    {
        long result;
        if (SteamClient.SteamUser == null)
            result = 0L;
        result = (long)SteamClient.SteamUser.GetSteamId();
        return Task.FromResult(ApiRspHelper.Ok(result));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> OwnsApps(uint appid, CancellationToken cancellationToken = default)
    {
        var result = SteamClient.SteamApps008 == null ? false : SteamClient.SteamApps008.IsSubscribedApp(appid);
        return Task.FromResult(ApiRspHelper.Code(result ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<IEnumerable<SteamApp>>> OwnsApps(IEnumerable<SteamApp> apps, CancellationToken cancellationToken = default)
    {
        IEnumerable<SteamApp> ownsApps;
        if (!apps.Any_Nullable())
            ownsApps = new List<SteamApp>();
        else if (SteamClient.SteamApps008 == null || SteamClient.SteamApps001 == null)
            ownsApps = apps;
        else
            ownsApps = apps.Where(f => SteamClient.SteamApps008.IsSubscribedApp(f.AppId))
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
                s.State = IsAppInstalled(s.AppId).GetAwaiter().GetResult().IsSuccess ? 4 : s.State;
                s.InstalledDir = string.IsNullOrEmpty(s.InstalledDir) ? GetAppInstallDir(s.AppId).GetAwaiter().GetResult().Content : s.InstalledDir;
                s.Type = Enum.TryParse<SteamAppType>(GetAppData(s.AppId, "type").GetAwaiter().GetResult().Content, true, out var result) ? result : SteamAppType.Unknown;
                return s;
            });
        return Task.FromResult(ApiRspHelper.Ok(ownsApps))!;
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<string>> GetAppData(uint appid, string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Ok(SteamClient.SteamApps001.GetAppData(appid, key)))!;
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> IsSteamChinaLauncher(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUtils.IsSteamChinaLauncher() ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> IsSteamInBigPictureMode(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUtils.IsSteamInBigPictureMode() ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<uint>> GetSecondsSinceAppActive(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Ok(SteamClient.SteamUtils.GetSecondsSinceAppActive()));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<uint>> GetServerRealTime(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Ok(SteamClient.SteamUtils.GetServerRealTime()));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> IsAppInstalled(uint appid, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamApps008.IsAppInstalled(appid) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<string>> GetAppInstallDir(uint appid, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Ok(SteamClient.SteamApps008.GetAppInstallDir(appid)))!;
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<string>> GetIPCountry(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Ok(SteamClient.SteamUtils.GetIPCountry()))!;
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<string>> GetCurrentGameLanguage(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Ok(SteamClient.SteamApps008.GetCurrentGameLanguage()))!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string>> GetAvailableGameLanguages(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return ApiRspHelper.Ok(SteamClient.SteamApps008.GetAvailableGameLanguages())!;
    }

    #region SteamUserStats

    /// <inheritdoc/>
    public Task<ApiRspImpl<int>> GetStatValueInt(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code<int>(
            code: SteamClient.SteamUserStats.GetStatValue(name, out int value) ? ApiRspCode.OK : ApiRspCode.Fail,
            message: null,
            content: value
            ));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<float>> GetStatValueFloat(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code<float>(
            code: SteamClient.SteamUserStats.GetStatValue(name, out float value) ? ApiRspCode.OK : ApiRspCode.Fail,
            message: null,
            content: value
            ));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<bool>> GetAchievementState(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code<bool>(
            code: SteamClient.SteamUserStats.GetAchievementState(name, out bool isAchieved) ? ApiRspCode.OK : ApiRspCode.Fail,
            message: null,
            content: isAchieved
            ));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<(bool isAchieved, long unlockTime)>> GetAchievementAndUnlockTime(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code<(bool isAchieved, long unlockTime)>(
            code: SteamClient.SteamUserStats.GetAchievementAndUnlockTime(name, out var isAchieved, out var unlockTime) ? ApiRspCode.OK : ApiRspCode.Fail,
            message: null,
            content: (isAchieved, unlockTime)
            ));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<float>> GetAchievementAchievedPercent(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code<float>(
            code: SteamClient.SteamUserStats.GetAchievementAchievedPercent(name, out var percent) ? ApiRspCode.OK : ApiRspCode.Fail,
            message: null,
            content: percent
            ));
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl> AddUserStatsReceivedCallback(Action<IUserStatsReceived> action, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        UserStatsReceivedCallback = SteamClient.CreateAndRegisterCallback<UserStatsReceivedCallback>();
        UserStatsReceivedCallback.OnRun += value =>
        {
            var valueWrapper = new UserStatsReceivedWrapper(value);
            action(valueWrapper);
        };
        return ApiRspHelper.Ok();
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
    public Task<ApiRspImpl> RequestCurrentStats(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUserStats.RequestCurrentStats() ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> ResetAllStats(bool achievementsToo, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUserStats.ResetAllStats(achievementsToo) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> SetAchievement(string name, bool state, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUserStats.SetAchievement(name, state) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> SetStatValue(string name, int value, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUserStats.SetStatValue(name, value) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> SetStatValue(string name, float value, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUserStats.SetStatValue(name, value) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> StoreStats(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamUserStats.StoreStats() ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl> RequestGlobalAchievementPercentages(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        SteamClient.SteamUserStats.RequestGlobalAchievementPercentages();
        return ApiRspHelper.Ok();
    }
    #endregion

    /// <inheritdoc/>
    public async Task<ApiRspImpl> RunCallbacks(bool server, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        SteamClient.RunCallbacks(server);
        return ApiRspHelper.Ok();
    }

    #region SteamRemoteStorage

    /// <inheritdoc/>
    public Task<ApiRspImpl<(ulong totalBytes, ulong availableBytes)>> GetCloudArchiveQuota(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code<(ulong totalBytes, ulong availableBytes)>(
            code: SteamClient.SteamRemoteStorage.GetQuota(out var totalBytes, out var availableBytes) ? ApiRspCode.OK : ApiRspCode.Fail,
            message: null,
            content: (totalBytes, availableBytes)
            ));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl<List<SteamRemoteFile>?>> GetCloudArchiveFiles(CancellationToken cancellationToken = default)
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

        return Task.FromResult(ApiRspHelper.Ok(files));
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<int>> FileRead(string name, byte[] buffer, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return SteamClient.SteamRemoteStorage.FileRead(name, buffer);
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> FileWrite(string name, byte[] buffer, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamRemoteStorage.FileWrite(name, buffer) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> FileForget(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamRemoteStorage.FileForget(name) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    /// <inheritdoc/>
    public Task<ApiRspImpl> FileDelete(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiRspHelper.Code(SteamClient.SteamRemoteStorage.FileDelete(name) ? ApiRspCode.OK : ApiRspCode.Fail));
    }

    #endregion

#endif
}