#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
#if WINDOWS
using System.Management;
#endif
using BD.Common8.Enums;
using BD.Common8.Helpers;
using BD.Common8.Models;
using BD.SteamClient8.Enums.WebApi;
using BD.SteamClient8.Enums.WebApi.SteamApps;
using BD.SteamClient8.Enums.WebApi.SteamGridDB;
using BD.SteamClient8.Helpers;
using BD.SteamClient8.Models;
using BD.SteamClient8.Models.Extensions;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;
using BD.SteamClient8.Services.Abstractions.PInvoke;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Extensions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using ValveKeyValue;
using static BD.SteamClient8.Services.Abstractions.PInvoke.ISteamService;
using BD.SteamClient8.Services.Abstractions.Mvvm;

namespace BD.SteamClient8.Services.PInvoke;

/// <summary>
/// <see cref="ISteamService"/> Steam 相关助手、工具类服务实现
/// </summary>
/// <remarks>
/// 初始化 <see cref="SteamServiceImpl"/> 类的新实例
/// </remarks>
/// <param name="loggerFactory"></param>
public abstract partial class SteamServiceImpl(ILoggerFactory loggerFactory)
{
    /// <summary>
    /// 用于标识和记录日志信息
    /// </summary>
    protected const string TAG = "SteamS";

    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger logger = loggerFactory.CreateLogger(TAG);

    /// <inheritdoc/>
    public string? SteamDirPath => SteamPathHelper.GetSteamDirPath();

    /// <inheritdoc/>
    public string? SteamProgramPath => SteamPathHelper.GetSteamProgramPath();
}

partial class SteamServiceImpl : ISteamService
{
    /// <inheritdoc/>
    public virtual bool IsRunningSteamProcess => GetSteamProcesses().Length != 0;

    /// <inheritdoc/>
    public async Task<bool> TryKillSteamProcessAsync()
    {
        try
        {
            var isOK = await TryKillSteamProcessCoreAsync();
            if (isOK)
            {
                Conn.IsConnectToSteam = false;
                return true;
            }
        }
        catch
        {
        }
        if (!IsRunningSteamProcess)
        {
            Conn.IsConnectToSteam = false;
        }
        return false;
    }

    /// <inheritdoc/>
    public int GetSteamProcessPid()
    {
        var steamProces = GetSteamProcess();
        return steamProces != null ? steamProces.Id : default;
    }

    /// <inheritdoc/>
    public Task<bool> IsSteamChinaLauncherAsync()
    {
        var result = IsSteamChinaLauncherCoreAsync();
        return result;
    }

    /// <inheritdoc/>
    public void StartSteam(string? arguments = null)
    {
        var steamProgramPath = SteamPathHelper.GetSteamProgramPath();
        if (!string.IsNullOrWhiteSpace(steamProgramPath) && File.Exists(steamProgramPath))
        {
            StartProcess(steamProgramPath, arguments, IsRunSteamAdministrator);
        }
    }

    /// <inheritdoc/>
    public void StartSteamWithDefaultParameter() => StartSteam(StratSteamDefaultParameter);

    /// <inheritdoc/>
    public async Task<bool> ShutdownSteamAsync()
    {
        if (IsRunningSteamProcess)
        {
            try
            {
                var isOK = await ShutdownSteamCoreAsync();
                if (isOK)
                {
                    Conn.IsConnectToSteam = false;
                    return true;
                }
            }
            catch
            {
            }
            if (!IsRunningSteamProcess)
            {
                Conn.IsConnectToSteam = false;
            }
            return false;
        }
        else
        {
            Conn.IsConnectToSteam = false;
            return true;
        }
    }

    /// <inheritdoc/>
    public string? GetLastLoginUserName()
    {
        var autoLoginUser = SteamPathHelper.GetAutoLoginUser();
        return autoLoginUser;
    }

    /// <inheritdoc/>
    public List<SteamUser> GetRememberUserList()
    {
        var users = new List<SteamUser>();
        try
        {
            var userVdfPath = SteamPathHelper.GetUserVdfPath();
            if (!string.IsNullOrWhiteSpace(userVdfPath) && File.Exists(userVdfPath))
            {
                var v = VdfHelper.Read(userVdfPath);
                if (v != null)
                {
                    foreach (KVObject it in v)
                    {
                        try
                        {
                            if (long.TryParse(it.Name, out var steamId64) &&
                                it.Value is KVCollectionValue collection)
                            {
                                long timestamp;
                                DateTime lastLoginTime;
                                if (collection.TryParse<long>("timestamp", out var timestamp1, StringComparison.OrdinalIgnoreCase))
                                {
                                    timestamp = timestamp1;
                                    lastLoginTime = timestamp.ToDateTimeS();
                                }
                                else
                                {
                                    timestamp = default;
                                    lastLoginTime = default;
                                }
                                var user = new SteamUser
                                {
                                    SteamId64 = steamId64,
                                    AccountName = collection.GetString("AccountName"),
                                    //SteamID = collection.GetString("PersonaName"),
                                    PersonaName = collection.GetString("PersonaName"),
                                    RememberPassword = collection.TryParse<bool>("RememberPassword", out var rememberPassword) && rememberPassword,
                                    AllowAutoLogin = collection.TryParse<bool>("AllowAutoLogin", out var allowAutoLogin) && allowAutoLogin,

                                    // 老版本 Steam 数据 小写 mostrecent 支持
                                    MostRecent = collection.TryParse<bool>("MostRecent", out var mostrecent, StringComparison.OrdinalIgnoreCase) && mostrecent,

                                    Timestamp = timestamp,
                                    LastLoginTime = lastLoginTime,

                                    WantsOfflineMode = collection.TryParse<bool>("WantsOfflineMode", out var wantsOfflineMode) && wantsOfflineMode,

                                    // 因为警告这个东西应该都不需要所以直接默认跳过好了
                                    // SkipOfflineModeWarning = i.SkipOfflineModeWarning != null ?
                                    //    Convert.ToBoolean(Convert.ToByte(i.SkipOfflineModeWarning.ToString())) : false,
                                    SkipOfflineModeWarning = true,
                                };
                                user.SteamID = user.PersonaName;
                                users.Add(user);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "GetRememberUserList it fail");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetRememberUserList foreach fail");
        }
        return users;
    }

    /// <inheritdoc/>
    public string? GetRememberUserListString(bool writeIndented)
    {
        var list = GetRememberUserList();

#pragma warning disable CA1869 // 缓存并重用“JsonSerializerOptions”实例
        var opt = writeIndented ? new JsonSerializerOptions(DefaultJsonSerializerContext_.Default.Options)
        {
            WriteIndented = true,
        } : DefaultJsonSerializerContext_.Default.Options;
#pragma warning restore CA1869 // 缓存并重用“JsonSerializerOptions”实例

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        var jsonString = JsonSerializer.Serialize(list, opt);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        return jsonString;
    }

    /// <inheritdoc/>
    public virtual Task<bool> UpdateAuthorizedDeviceListAsync(IEnumerable<AuthorizedDevice> items)
    {
        var result = VdfHelper.UpdateAuthorizedDeviceList(items);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public virtual Task<bool> RemoveAuthorizedDeviceListAsync(AuthorizedDevice item)
    {
        var result = VdfHelper.RemoveAuthorizedDeviceList(item);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public List<AuthorizedDevice> GetAuthorizedDeviceList()
    {
        string? configVdfPath = null;
        var authorizeds = new List<AuthorizedDevice>();
        try
        {
            configVdfPath = SteamPathHelper.GetConfigVdfPath();
            if (!string.IsNullOrWhiteSpace(configVdfPath) && File.Exists(configVdfPath))
            {
                var v = VdfHelper.Read(configVdfPath);
                if (v != null)
                {
                    var authorizedDevice = v["AuthorizedDevice"];
                    if (authorizedDevice is IEnumerable<KVObject> authorizedDevice2)
                    {
                        var index = 0;
                        foreach (var it in authorizedDevice2)
                        {
                            try
                            {
                                if (long.TryParse(it.Name, out var steamId3) && it.Value is KVCollectionValue c)
                                {
                                    authorizeds.Add(new AuthorizedDevice()
                                    {
                                        Index = index,
                                        SteamId3_Int = steamId3,
                                        Timeused = c.TryParse<long>("timeused", out var timeused) ? timeused : default,
                                        Description = c.GetString("description"),
                                        Tokenid = c.GetString("tokenid"),
                                    });
                                    index++;
                                }
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "GetAuthorizedDeviceList it fail, configVdfPath: {configVdfPath}", configVdfPath);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetAuthorizedDeviceList fail, configVdfPath: {configVdfPath}", configVdfPath);
        }
        return authorizeds;
    }

    /// <inheritdoc/>
    public async Task<bool> SetSteamCurrentUserAsync(string userName)
    {
        const uint rememberPassword = 1;
        try
        {

            var result = await SetSteamCurrentUserCoreAsync(userName, rememberPassword);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "SetSteamCurrentUser fail, userName: {userName}, rememberPassword: {rememberPassword}", userName, rememberPassword);
            return false;
        }
    }

    /// <inheritdoc/>
    public virtual Task SetPersonaStateAsync(string steamId32, PersonaState ePersonaState)
    {
        VdfHelper.SetPersonaState(steamId32, ePersonaState);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task<bool> DeleteLocalUserDataAsync(SteamUser user, bool isDeleteUserData = false)
    {
        var result = VdfHelper.DeleteLocalUserData(user, isDeleteUserData);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public virtual Task<bool> UpdateLocalUserDataAsync(IEnumerable<SteamUser> users)
    {
        var result = VdfHelper.UpdateLocalUserData(users);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public event Action<(WatcherChangeTypes changeType, string fullPath, string? name)>? OnWatchLocalUserDataChanged;

    IDisposable? localUserDataWatcher;

    /// <inheritdoc/>
    public virtual void StartWatchLocalUserDataChange()
    {
        localUserDataWatcher?.Dispose();
        localUserDataWatcher = VdfHelper.GetLocalUserDataWatcher((_, e) =>
        {
            OnWatchLocalUserDataChanged?.Invoke((e.ChangeType, e.FullPath, e.Name));
        });
    }

    /// <inheritdoc/>
    public virtual Task<List<SteamApp>?> GetAppInfos(bool isSaveProperties = false)
    {
        var hideGameList = HideGameList;
        var hideGameKeys = hideGameList?.Keys;
        var result = VdfHelper.GetAppInfos(hideGameKeys, isSaveProperties);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public List<ModifiedApp>? GetModifiedApps()
    {
        var result = GetModifiedAppsCore();
        return SafeResult(result);

        static List<ModifiedApp>? SafeResult(List<ModifiedApp>? list)
        {
            if (list == null)
            {
                return null;
            }
            else if (list.Contains(null!))
            {
                list.RemoveAll(static x => x == null);
                return list;
            }
            else
            {
                return list;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveAppInfosToSteam() // 函数名保持与旧代码兼容
    {
        var editApps = Conn.SteamApps.Where(s => s.IsEdited);

        var hideGameList = HideGameList;
        var hideGameKeys = hideGameList?.Keys;

        var result = await SaveAppInfosCoreAsync(hideGameKeys, editApps);
        return result;
    }

    /// <inheritdoc/>
    public string? GetAppImageFilePath(uint appId, LibCacheType type, bool checkFileExists)
    {
        var mostRecentUser = Conn.SteamUsers.Where(s => s.MostRecent).FirstOrDefault();
        if (mostRecentUser != null)
        {
            var customFilePath = GetAppCustomImageFilePath(appId, mostRecentUser, type);
            if (customFilePath != null)
            {
                if (!checkFileExists || File.Exists(customFilePath))
                {
                    return customFilePath;
                }
            }
        }

        var steamLanguageString = Conn.SteamLanguageString;
        var cacheFilePath = GetAppLibCacheFilePath(appId, type, steamLanguageString);
        if (cacheFilePath != null)
        {
            if (!checkFileExists || File.Exists(cacheFilePath))
            {
                return cacheFilePath;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public Task<bool> SaveAppImageToSteamFile(
        string? imageFilePath,
        string steamId32,
        uint appId,
        SteamGridItemType gridType)
    {
        var result = SaveAppImageToFileCoreAsync(imageFilePath, steamId32, appId, gridType);
        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SteamApp>? GetDownloadingAppList()
    {
        var result = VdfHelper.GetDownloadingAppList();
        return result;
    }

    /// <inheritdoc/>
    public event Action<(WatcherChangeTypes changeType, string fullPath, string? name, SteamApp)>? OnWatchSteamDownloadingChanged;

    /// <inheritdoc/>
    public event Action<(WatcherChangeTypes changeType, string fullPath, string? name, uint)>? OnWatchSteamDownloadingDeleted;

    IDisposable? steamDownloadingWatcher;

    /// <inheritdoc/>
    public virtual void StartWatchSteamDownloading()
    {
        steamDownloadingWatcher?.Dispose();
        steamDownloadingWatcher = VdfHelper.GetSteamDownloadingWatcher((_, e, app) =>
        {
            OnWatchSteamDownloadingChanged?.Invoke((e.ChangeType, e.FullPath, e.Name, app));
        },
        (_, e, appid) =>
        {
            OnWatchSteamDownloadingDeleted?.Invoke((e.ChangeType, e.FullPath, e.Name, appid));
        });
    }

    /// <inheritdoc/>
    public virtual void StopWatchSteamDownloading()
    {
        steamDownloadingWatcher?.Dispose();
        steamDownloadingWatcher = null;
    }
}

partial class SteamServiceImpl // protected
{
    // 写入操作通常需要提升权限，使用虚方法以及实现逻辑位于静态助手类中，子类重写为 IPC 调用

    /// <inheritdoc cref="SteamProcHelper.SteamMainProcessName"/>
    protected virtual string SteamMainProcessName => SteamProcHelper.SteamMainProcessName;

    /// <inheritdoc cref="SteamProcHelper.GetSteamProcessNames"/>
    protected virtual string[] SteamProcessNames => SteamProcHelper.GetSteamProcessNames();

    /// <summary>
    /// 获取所有的 Steam 主进程
    /// </summary>
    /// <returns></returns>
    protected virtual Process[] GetSteamProcesses() => Process.GetProcessesByName(SteamMainProcessName);

    /// <summary>
    /// 获取首个 Steam 主进程
    /// </summary>
    /// <returns></returns>
    protected virtual Process? GetSteamProcess() => GetSteamProcesses().FirstOrDefault();

    protected virtual Task<bool> TryKillSteamProcessCoreAsync()
    {
        KillSteamProcesses();
        return Task.FromResult(true);
    }

    protected void KillSteamProcesses()
    {
        var processNames = SteamProcessNames;
        SteamProcHelper.KillSteamProcesses(processNames);
    }

    /// <summary>
    /// 启动进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <param name="isPrivileged"></param>
    /// <returns></returns>
    protected virtual Process? StartProcess(string fileName, string? arguments = null, bool isPrivileged = false)
        => Process2.Start(fileName, arguments, useShellExecute: true);

    protected virtual Task<bool> ShutdownSteamCoreAsync()
    {
        var steamProgramPath = SteamPathHelper.GetSteamProgramPath();
        SteamProcHelper.KillSteamProcessesByCommandShutdown(steamProgramPath);
        return Task.FromResult(true);
    }

    protected virtual List<ModifiedApp>? GetModifiedAppsCore()
    {
        try
        {
            var configFilePathV2 = Path.Combine(IOPath.AppDataDirectory, VdfHelper.ModifiedFileNameV2);
            try
            {
                using var configFileStreamV2 = new FileStream(configFilePathV2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var result = JsonSerializer.Deserialize(configFileStreamV2, DefaultJsonSerializerContext_.Default.ListModifiedApp);
                return result;
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch
            {
            }

            var configFilePathV1 = Path.Combine(IOPath.AppDataDirectory, VdfHelper.ModifiedFileName);
            try
            {
                using var configFileStreamV1 = new FileStream(configFilePathV1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var result = Serializable.DMP<List<ModifiedApp>>(configFileStreamV1);
                return result;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetModifiedApps fail");
        }
        return null;
    }

    protected virtual async Task<bool> SaveAppInfosCoreAsync(ICollection<uint>? hideGameKeys, IEnumerable<SteamApp> editApps)
    {
        var result = await VdfHelper.SaveAppInfosAsync(hideGameKeys, editApps);
        return result;
    }

    protected virtual Task<bool> SaveAppImageToFileCoreAsync(
        string? imageFilePath,
        string steamId32,
        uint appId,
        SteamGridItemType gridType)
    {
        var result = SaveAppImageToFileAsync(imageFilePath, steamId32, appId, gridType, WriteImage);
        return result;
    }

    protected virtual Task<bool> IsSteamChinaLauncherCoreAsync()
    {
#if WINDOWS
        try
        {
            var result = SteamProcHelper.IsSteamChinaLauncherByProcCommandLine(SteamMainProcessName);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IsSteamChinaLauncherCoreAsync fail");
        }
#endif
        return Task.FromResult(false);
    }
}

partial class SteamServiceImpl // abstract
{
    /// <summary>
    /// 由子类实现的获取 Mvvm 连接服务
    /// </summary>
    protected abstract ISteamConnectService Conn { get; }

    /// <summary>
    /// 由子类实现的获取 Steam 启动默认参数字符串
    /// <para>SteamSettings.SteamStratParameter.Value</para>
    /// </summary>
    protected abstract string? StratSteamDefaultParameter { get; }

    /// <summary>
    /// 由子类实现的获取是否以管理员权限启动 Steam 进程，仅 Windows 支持
    /// <para>SteamSettings.IsRunSteamAdministrator.Value</para>
    /// </summary>
    protected virtual bool IsRunSteamAdministrator { get; }

    /// <summary>
    /// 由子类实现的获取隐藏的游戏列表
    /// <para>GameLibrarySettings.HideGameList.Value</para>
    /// </summary>
    protected abstract Dictionary<uint, string?>? HideGameList { get; }

    /// <summary>
    /// 设置当前登录的 Steam 用户
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="rememberPassword"></param>
    /// <returns></returns>
    protected virtual Task<bool> SetSteamCurrentUserCoreAsync(string userName, uint? rememberPassword)
    {
        SteamPathHelper.SetAutoLoginUser(userName, rememberPassword);
        return Task.FromResult(true);
    }
}

partial class SteamServiceImpl // static
{
    static string? GetAppCustomImageFilePath(uint appId, SteamUser user, LibCacheType type)
    {
        var steamDirPath = SteamPathHelper.GetSteamDirPath();
        if (string.IsNullOrEmpty(steamDirPath))
        {
            return null;
        }

        var path = Path.Combine(steamDirPath, "userdata", user.SteamId32.ToString(), "config", "grid");

        var fileName = type switch
        {
            LibCacheType.Header => $"{appId}.png",
            LibCacheType.Library_Grid => $"{appId}p.png",
            LibCacheType.Library_Hero => $"{appId}_hero.png",
            LibCacheType.Logo => $"{appId}_logo.png",
            _ => null,
        };

        if (fileName == null)
        {
            return null;
        }

        var filePath = Path.Combine(path, fileName);
        return filePath;
    }

    static string? GetAppLibCacheFilePath(uint appId, LibCacheType type, string? steamLanguageString)
    {
        var librarycacheDirPath = SteamPathHelper.GetLibrarycacheDirPath();
        if (string.IsNullOrEmpty(librarycacheDirPath))
        {
            return null;
        }

        var fileName = type switch
        {
            LibCacheType.Header => $"{appId}_header.jpg",
            LibCacheType.Icon => $"{appId}_icon.jpg",
            LibCacheType.Library_Grid => $"{appId}_library_600x900.jpg",
            LibCacheType.Library_Hero => $"{appId}_library_hero.jpg",
            LibCacheType.Library_Hero_Blur => $"{appId}_library_hero_blur.jpg",
            LibCacheType.Logo => $"{appId}_logo.png",
            _ => null,
        };

        if (fileName == null)
        {
            return null;
        }

        var filePath = Path.Combine(librarycacheDirPath, fileName);

        if (steamLanguageString != null && !File.Exists(filePath))
        {
            // 默认图没找到情况下尝试查找当前 Steam 客户端语言图片
            var lang = $"_{steamLanguageString}";
            filePath = filePath.Insert(filePath.LastIndexOf('.'), lang);
        }

        return filePath;
    }

    static async Task<bool> WriteImageCore(string saveFilePath, Stream sourceStream)
    {
        try
        {
            using FileStream saveFileStream = new(saveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);

            try
            {
                if (sourceStream.Position != 0)
                {
                    sourceStream.Position = 0;
                }
            }
            catch
            {
            }

            await sourceStream.CopyToAsync(saveFileStream);
            await saveFileStream.FlushAsync();
            saveFileStream.SetLength(saveFileStream.Position);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex,
                "WriteImage fail, saveFilePath: {saveFilePath}, sourceFilePath: {sourceFilePath}",
                saveFilePath,
                sourceStream is FileStream sourceFileStream ? sourceFileStream.Name : null);
        }
        return false;
    }

    public static async Task<bool> WriteImage(string saveFilePath, string sourceFilePath)
    {
        try
        {
            using FileStream sourceFileStream = new(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            var result = await WriteImageCore(saveFilePath, sourceFileStream);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "WriteImage fail, saveFilePath: {saveFilePath}, sourceFilePath: {sourceFilePath}", saveFilePath, sourceFilePath);
        }
        return false;
    }

    public static Task<bool> WriteImage(string saveFilePath, FileStream sourceFileStream)
    {
        var result = WriteImageCore(saveFilePath, sourceFileStream);
        return result;
    }

    public static Task<bool> WriteImage(string saveFilePath, Stream sourceStream)
    {
        if (sourceStream is FileStream sourceFileStream)
        {
            if (sourceFileStream.CanRead)
            {
                var result = WriteImage(saveFilePath, sourceFileStream);
                return result;
            }
            else
            {
                var result = WriteImage(saveFilePath, sourceFileStream.Name);
                return result;
            }
        }
        else
        {
            var result = WriteImageCore(saveFilePath, sourceStream);
            return result;
        }
    }

    public static async Task<bool> SaveAppImageToFileAsync<T>(
        T? imageSource,
        string steamId32,
        uint appId,
        SteamGridItemType gridType,
        Func<string, T, Task<bool>> writeImageDelegate) where T : notnull
    {
        // 泛型图片源使用文件路径或文件流或数据流，不使用字节数组等类型避免内存占用与 GC 压力
        try
        {
            var steamDirPath = SteamPathHelper.GetSteamDirPath();
            if (!string.IsNullOrWhiteSpace(steamDirPath))
            {
                var gridDirPath = Path.Combine(steamDirPath, "userdata", steamId32, "config", "grid");
                if (!Directory.Exists(gridDirPath))
                {
                    Directory.CreateDirectory(gridDirPath);
                }

                var filePath = gridType switch
                {
                    SteamGridItemType.Hero => Path.Combine(gridDirPath, $"{appId}_hero.png"),
                    SteamGridItemType.Logo => Path.Combine(gridDirPath, $"{appId}_logo.png"),
                    SteamGridItemType.Grid => Path.Combine(gridDirPath, $"{appId}p.png"),
                    _ => Path.Combine(gridDirPath, $"{appId}.png"),
                };

                if (imageSource is null)
                {
                    // 传递 null 图片源以删除文件
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                else if (writeImageDelegate != null)
                {
                    var result = await writeImageDelegate(filePath, imageSource);
                    return result;
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex,
                "SaveAppImageToFileAsync fail, steamId32: {steamId32}, appId: {appId}, gridType: {gridType}", steamId32, appId, gridType);
        }
        return false;
    }
}

partial class SteamServiceImpl : IDisposable
{
    protected bool DisposedValue { get; private set; }

    protected virtual void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                localUserDataWatcher?.Dispose();
                steamDownloadingWatcher?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            localUserDataWatcher = null;
            DisposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
#endif