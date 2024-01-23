#if !(IOS || ANDROID)
using static BD.SteamClient8.Services.ISteamworksLocalApiService;
using SAMAPIClient = SAM.API.Client;
using UserStatsReceivedCallback = SAM.API.Callbacks.UserStatsReceived;

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Impl;

/// <inheritdoc cref="ISteamworksLocalApiService "/> Steamworks 本地 API 服务实现
class SteamworksLocalApiServiceImpl : ISteamworksLocalApiService
{
    /// <summary>
    /// <see cref="SAMAPIClient"/> 实例
    /// </summary>
    public SAMAPIClient SteamClient { get; } = null!;

    UserStatsReceivedCallback? UserStatsReceivedCallback;

    /// <summary>
    /// 初始化 <see cref="SteamworksLocalApiServiceImpl"/> 类的新实例
    /// </summary>
    public SteamworksLocalApiServiceImpl()
    {
        if (!IsSupported)
            return;

        Steam.GetInstallPathDelegate = ISteamService.GetSteamDynamicLinkLibraryPath;
        SteamClient = new SAMAPIClient();
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl> DisposeSteamClient(CancellationToken cancellationToken = default)
    {
        if (IsSupported)
        {
            SteamClient.Dispose();
        }
        await Task.CompletedTask;
        return ApiRspHelper.Ok();
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl> Initialize(int appid, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            try
            {
                SteamClient.Initialize(appid);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "Initialize fail.");
                return ex;
            }
        }
        return ApiRspHelper.Ok();
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<long>> GetSteamId64(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var steamUser = SteamClient.SteamUser;
            if (steamUser == null)
                return 0L;
            var result = (long)steamUser.GetSteamId();
            return result;
        }
        return 0L;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> OwnsApp(uint appid, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamApps008 != null && SteamClient.SteamApps008.IsSubscribedApp(appid);
            return result;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<IEnumerable<SteamApp>>> OwnsApps(IEnumerable<SteamApp> apps, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
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
            return ApiRspHelper.Ok(ownsApps)!;
        }
        return Array.Empty<SteamApp>()!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string>> GetAppData(uint appid, string key, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamApps001.GetAppData(appid, key);
            return ApiRspHelper.Ok(result)!;
        }
        return ApiRspHelper.Ok<string>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> IsSteamChinaLauncher(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUtils.IsSteamChinaLauncher();
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> IsSteamInBigPictureMode(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUtils.IsSteamInBigPictureMode();
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<uint>> GetSecondsSinceAppActive(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUtils.GetSecondsSinceAppActive();
            return ApiRspHelper.Ok(result)!;
        }
        return 0;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<uint>> GetServerRealTime(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUtils.GetServerRealTime();
            return ApiRspHelper.Ok(result)!;
        }
        return 0;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> IsAppInstalled(uint appid, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamApps008.IsAppInstalled(appid);
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string>> GetAppInstallDir(uint appid, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamApps008.GetAppInstallDir(appid);
            return ApiRspHelper.Ok(result)!;
        }
        return ApiRspHelper.Ok<string>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string>> GetCountryOrRegionByIP(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUtils.GetIPCountry();
            return ApiRspHelper.Ok(result)!;
        }
        return ApiRspHelper.Ok<string>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string>> GetCurrentGameLanguage(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamApps008.GetCurrentGameLanguage();
            return ApiRspHelper.Ok(result)!;
        }
        return ApiRspHelper.Ok<string>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<string>> GetAvailableGameLanguages(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamApps008.GetAvailableGameLanguages();
            return ApiRspHelper.Ok(result)!;
        }
        return ApiRspHelper.Ok<string>(null)!;
    }

    #region SteamUserStats

    /// <inheritdoc/>
    public async Task<ApiRspImpl<int?>> GetStatValueInt(string name, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.GetStatValue(name, out int value);
            return ApiRspHelper.Ok(result ? value : (int?)null)!;
        }
        return ApiRspHelper.Ok<int?>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<float?>> GetStatValueFloat(string name, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.GetStatValue(name, out float value);
            return ApiRspHelper.Ok(result ? value : (float?)null)!;
        }
        return ApiRspHelper.Ok<float?>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool?>> GetAchievementState(string name, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.GetAchievementState(name, out bool isAchieved);
            return ApiRspHelper.Ok(result ? isAchieved : (bool?)null)!;
        }
        return ApiRspHelper.Ok<bool?>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<AchievementAndUnlockTimeResult?>> GetAchievementAndUnlockTime(string name, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.GetAchievementAndUnlockTime(name, out var isAchieved, out var unlockTime);
            return ApiRspHelper.Ok<AchievementAndUnlockTimeResult?>(result ? new AchievementAndUnlockTimeResult
            {
                IsAchieved = isAchieved,
                UnlockTime = unlockTime,
            } : null)!;
        }
        return ApiRspHelper.Ok<AchievementAndUnlockTimeResult?>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<float?>> GetAchievementAchievedPercent(string name, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.GetAchievementAchievedPercent(name, out var percent);
            return ApiRspHelper.Ok<float?>(result ? percent : null)!;
        }
        return ApiRspHelper.Ok<float?>(null)!;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ApiRspImpl<UserStatsReceivedResult>> AddUserStatsReceivedCallback([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (IsSupported)
        {
            var tcs = new TaskCompletionSource<UserStatsReceivedResult>();
            tcs.SetCanceled(cancellationToken);

            UserStatsReceivedCallback = SteamClient.CreateAndRegisterCallback<UserStatsReceivedCallback>();
            UserStatsReceivedCallback.OnRun += value =>
            {
                tcs.TrySetResult(new()
                {
                    GameId = value.GameId,
                    Result = value.Result,
                });
            };

            var result = await tcs.Task;

            yield return ApiRspHelper.Ok(result)!;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> RequestCurrentStats(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.RequestCurrentStats();
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> ResetAllStats(bool achievementsToo, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.ResetAllStats(achievementsToo);
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> SetAchievement(string name, bool state, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.SetAchievement(name, state);
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> SetStatInt32Value(string name, int value, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.SetStatValue(name, value);
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> SetStatSingleValue(string name, float value, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.SetStatValue(name, value);
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> StoreStats(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.StoreStats();
            return ApiRspHelper.Ok(result)!;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<nint>> RequestGlobalAchievementPercentages(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamUserStats.RequestGlobalAchievementPercentages();
            return ApiRspHelper.Ok(result);
        }
        return 0;
    }

    #endregion

    /// <inheritdoc/>
    public async Task<ApiRspImpl> RunCallbacks(bool server, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            SteamClient.RunCallbacks(server);
        }
        return ApiRspHelper.Ok();
    }

    #region SteamRemoteStorage

    /// <inheritdoc/>
    public async Task<ApiRspImpl<CloudArchiveQuotaResult?>> GetCloudArchiveQuota(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamRemoteStorage.GetQuota(out var totalBytes, out var availableBytes);
            return ApiRspHelper.Ok<CloudArchiveQuotaResult?>(result ? new CloudArchiveQuotaResult
            {
                AvailableBytes = availableBytes,
                TotalBytes = totalBytes,
            } : null)!;
        }
        return ApiRspHelper.Ok<CloudArchiveQuotaResult>(null)!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<List<SteamRemoteFile>?>> GetCloudArchiveFiles(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        List<SteamRemoteFile> files = [];

        if (IsSupported)
        {
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
        }

        return files;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<byte[]?>> FileRead(string name, long length, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            byte[] buffer = new byte[length];
            var result = SteamClient.SteamRemoteStorage.FileRead(name, buffer);
            if (result == 0)
                return ApiRspHelper.Ok((byte[]?)null);
            return buffer;
        }
        return (byte[]?)null!;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> FileWrite(string name, byte[] buffer, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamRemoteStorage.FileWrite(name, buffer);
            return ApiRspHelper.Ok(result);
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> FileForget(string name, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamRemoteStorage.FileForget(name);
            return ApiRspHelper.Ok(result);
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<ApiRspImpl<bool>> FileDelete(string name, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (IsSupported)
        {
            var result = SteamClient.SteamRemoteStorage.FileDelete(name);
            return ApiRspHelper.Ok(result);
        }
        return false;
    }

    #endregion

}
#endif