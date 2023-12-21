#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

namespace BD.SteamClient8.Impl.PInvoke;

/// <summary>
/// <see cref="ISteamService"/> 类实现
/// </summary>
public abstract partial class SteamServiceImpl : ISteamService
{
    /// <summary>
    /// 用于标识和记录日志信息
    /// </summary>
    protected const string TAG = "SteamS";

    /// <summary>
    /// 修改的文件名
    /// </summary>
    protected const string ModifiedFileName = "modifications.vdf";

    /// <summary>
    /// <list type="bullet">
    ///   <item>
    ///     Windows：~\Steam\config\loginusers.vdf
    ///   </item>
    ///   <item>
    ///     Linux：~/.steam/steam/config/loginusers.vdf
    ///   </item>
    ///   <item>
    ///     Mac：~/Library/Application Support/Steam/config/loginusers.vdf
    ///   </item>
    /// </list>
    /// </summary>
    protected readonly string? UserVdfPath;

    /// <summary>
    /// 配置 Vdf 文件路径
    /// </summary>
    protected readonly string? ConfigVdfPath;

    /// <summary>
    /// 应用程序信息文件的路径
    /// </summary>
    protected readonly string? AppInfoPath;

    /// <summary>
    /// 程序库缓存目录的路径
    /// </summary>
    protected readonly string? LibrarycacheDirPath;

    /// <summary>
    /// 用户数据目录名称
    /// </summary>
    protected const string UserDataDirectory = "userdata";

    /// <summary>
    /// Steam 目录的路径
    /// </summary>
    protected readonly string? mSteamDirPath;

    /// <summary>
    /// Steam 程序的路径
    /// </summary>
    protected readonly string? mSteamProgramPath;

    /// <summary>
    /// 注册表信息文件的路径
    /// </summary>
    protected readonly string? mRegistryVdfPath;

    /// <summary>
    /// Steam 相关进程的列表
    /// </summary>
    protected readonly string[] steamProcess = new[] {
#if MACCATALYST || MACOS
        "steam_osx",
#else
        "steam",
#endif
        "steamservice",
        "steamwebhelper",
        "GameOverlayUI",
    };

    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger logger;

    /// <summary>
    /// Steam 下载文件的监听器列表
    /// </summary>
    protected List<FileSystemWatcher>? steamDownloadingWatchers;

    /// <summary>
    /// 初始化 <see cref="SteamServiceImpl"/> 类的新实例
    /// </summary>
    /// <param name="loggerFactory"></param>
    public SteamServiceImpl(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(TAG);
        mSteamDirPath = GetSteamDirPath();
        mSteamProgramPath = GetSteamProgramPath();
        UserVdfPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "config", "loginusers.vdf");
        AppInfoPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "appcache", "appinfo.vdf");
        LibrarycacheDirPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "appcache", "librarycache");
        mRegistryVdfPath = GetRegistryVdfPath();
        // SteamDirPath == null ? null : Path.Combine(SteamDirPath, "registry.vdf");
        //RegistryVdfPath  = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "registry.vdf");
        ConfigVdfPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "config", "config.vdf");

        if (!File.Exists(UserVdfPath)) UserVdfPath = null;
        if (!File.Exists(AppInfoPath)) AppInfoPath = null;
        if (!File.Exists(ConfigVdfPath)) ConfigVdfPath = null;
        if (!Directory.Exists(LibrarycacheDirPath)) LibrarycacheDirPath = null;
    }

    /// <inheritdoc/>
    public string? SteamDirPath => mSteamDirPath;

    /// <summary>
    /// 非windows平台 steam 注册表配置路径
    /// </summary>
    public string? RegistryVdfPath => mRegistryVdfPath;

    /// <inheritdoc/>
    public string? SteamProgramPath => mSteamProgramPath;

    /// <summary>
    /// 检测 Steam 是否正在运行
    /// </summary>
    public bool IsRunningSteamProcess => GetSteamProcesses().Length > 0;

    /// <summary>
    /// 结束 Steam 进程
    /// </summary>
    /// <returns></returns>
    protected virtual ValueTask<bool> KillSteamProcess()
    {
        var r = KillSteamProcessCore();
        return ValueTask.FromResult(r);
    }

    bool KillSteamProcessCore()
    {
        var processNames = steamProcess;
        var processes = processNames.Select(static x =>
        {
            try
            {
                var process = Process.GetProcessesByName(x);
                return process;
            }
            catch
            {
                return Array.Empty<Process>();
            }
        }).SelectMany(static x => x).ToArray();

        static ApplicationException? KillProcess(Process? process)
        {
            if (process == null)
                return default;
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                return new ApplicationException(
                    $"KillProcesses fail, name: {process?.ProcessName}", ex);
            }
            return default;
        }

        try
        {
            if (processes.Length > 0)
            {
                var tasks = processes.Select(x =>
                {
                    return Task.Run(() =>
                    {
                        return KillProcess(x);
                    });
                }).ToArray();
                Task.WaitAll(tasks);

                var innerExceptions = tasks.Select(x => x.Result!)
                    .Where(x => x != null).ToArray();
                if (innerExceptions.Length > 0)
                {
                    throw new AggregateException(
                        "KillProcess fail", innerExceptions);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "KillSteamProcess fail");
            throw;
        }

        return true;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryKillSteamProcess()
    {
        try
        {
            return await KillSteamProcess();
            //if (IsRunningSteamProcess)
            //{
            //    Process closeProc = Process.Start(new ProcessStartInfo(SteamProgramPath, "-shutdown"));
            //    bool closeProcSuccess = closeProc != null && closeProc.WaitForExit(3000);
            //    return closeProcSuccess;
            //}
            //return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            Conn.IsConnectToSteam = false;
        }
    }

    /// <summary>
    /// 获取 Steam 主进程 Id
    /// </summary>
    /// <returns></returns>
    public int GetSteamProcessPid()
    {
        var steamProces = GetSteamProces();
        return steamProces != null ? steamProces.Id : default;
    }

    /// <summary>
    /// 获取所有的 Steam 主进程
    /// </summary>
    /// <returns></returns>
    private Process[] GetSteamProcesses() => Process.GetProcessesByName(steamProcess[0]);

    /// <summary>
    /// 获取首个 Steam 主进程
    /// </summary>
    /// <returns></returns>
    private Process? GetSteamProces() => GetSteamProcesses().FirstOrDefault();

    /// <summary>
    /// 检测是否是中国启动器
    /// </summary>
    /// <returns></returns>
    public bool IsSteamChinaLauncher()
    {
#if WINDOWS
        var process = GetSteamProces();
        if (process != null)
        {
            try
            {
                return GetCommandLineArgsCore().Contains("-steamchina", StringComparison.OrdinalIgnoreCase);
            }
            catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                // 没有对该进程的安全访问权限。
                return false;
            }
            catch (InvalidOperationException)
            {
                // 进程已退出。
                return false;
            }
        }
        string GetCommandLineArgsCore()
        {
            using (var searcher = new ManagementObjectSearcher(
                "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (var objects = searcher.Get())
            {
                var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                return @object?["CommandLine"]?.ToString() ?? "";
            }
        }
#endif
        return false;
    }

    /// <inheritdoc/>
    public void StartSteam(string? arguments = null)
    {
        if (!string.IsNullOrWhiteSpace(SteamProgramPath) && File.Exists(SteamProgramPath))
        {
            if (OperatingSystem.IsWindows() && !IsRunSteamAdministrator)
            {
                StartAsInvoker(SteamProgramPath, arguments);
            }
            else
            {
                Process2.Start(SteamProgramPath, arguments, useShellExecute: true);
            }
        }
    }

    /// <inheritdoc/>
    public async Task ShutdownSteamAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(SteamProgramPath) && File.Exists(SteamProgramPath))
        {
            var steamProces = GetSteamProces();
            if (steamProces != null)
            {
                Process2.Start(SteamProgramPath, "-shutdown", useShellExecute: true);
                await steamProces.WaitForExitAsync(cancellationToken);
            }
        }
    }

    /// <inheritdoc/>
    public string GetLastLoginUserName() => GetLastSteamLoginUserName();

    /// <inheritdoc/>
    public List<SteamUser> GetRememberUserList()
    {
        var users = new List<SteamUser>();
        try
        {
            if (!string.IsNullOrWhiteSpace(UserVdfPath) && File.Exists(UserVdfPath))
            {
                var v = VdfHelper.Read(UserVdfPath);
                foreach (var item in v)
                {
                    try
                    {
                        long timestamp = item["timestamp"] != null ?
                                (long)item["timestamp"] :
                                (long)item["Timestamp"];
                        var user = new SteamUser
                        {
                            SteamId64 = Convert.ToInt64(item.Name),
                            AccountName = (string)item["AccountName"],
                            SteamID = (string)item["PersonaName"],
                            PersonaName = (string)item["PersonaName"],
                            RememberPassword = (bool)item["RememberPassword"],
                            AllowAutoLogin = (bool)item["AllowAutoLogin"],

                            // 老版本 Steam 数据 小写 mostrecent 支持
                            MostRecent = item["mostrecent"] != null ?
                                (bool)item["mostrecent"] :
                                (bool)item["MostRecent"],

                            Timestamp = timestamp,
                            LastLoginTime = timestamp.ToDateTimeS(),

                            WantsOfflineMode = (bool)item["WantsOfflineMode"],

                            // 因为警告这个东西应该都不需要所以直接默认跳过好了
                            // SkipOfflineModeWarning = i.SkipOfflineModeWarning != null ?
                            //    Convert.ToBoolean(Convert.ToByte(i.SkipOfflineModeWarning.ToString())) : false,
                            SkipOfflineModeWarning = true,
                        };

                        users.Add(user);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "GetRememberUserList fail(0).");
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetRememberUserList fail(1).");
        }
        return users;
    }

    /// <summary>
    /// 更新授权设备列表
    /// </summary>
    public bool UpdateAuthorizedDeviceList(IEnumerable<AuthorizedDevice> items)
    {
        var authorizeds = new List<AuthorizedDevice>();
        try
        {
            if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
            {
                var v = VdfHelper.Read(ConfigVdfPath);
                var lists = new KVCollectionValue();
                foreach (var item in items.OrderBy(x => x.Index))
                {
                    KVObject itemTemp = new KVObject(item.SteamId3_Int.ToString(), new KVObject[]
                    {
                            new KVObject("timeused", (KVValue)item.Timeused),
                            new KVObject("description", (KVValue)item.Description!),
                            new KVObject("tokenid", (KVValue)item.Tokenid!),
                    });
                    lists.Add(itemTemp);
                }
                v.Set("AuthorizedDevice", lists);
                VdfHelper.Write(ConfigVdfPath, v);
                return true;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateAuthorizedDeviceList fail(0).");
            return false;
        }
        return false;
    }

    /// <summary>
    /// 从授权设备列表中移除指定的授权设备
    /// </summary>
    /// <param name="model">要移除的授权设备模型</param>
    /// <returns>如果成功移除授权设备，则返回 <see langword="true"/>；否则返回 <see langword="true"/></returns>
    public bool RemoveAuthorizedDeviceList(AuthorizedDevice model)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
            {
                var v = VdfHelper.Read(ConfigVdfPath);
                var authorizedDevices = v["AuthorizedDevice"] as KVCollectionValue;
                if (authorizedDevices != null)
                {
                    authorizedDevices.Remove(model.SteamId3_Int.ToString());
                    //v["AuthorizedDevice"] = authorizedDevices;
                    VdfHelper.Write(ConfigVdfPath, v);
                }
                return true;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "RemoveAuthorizedDeviceList fail(0).");
            return false;
        }
        return false;
    }

    /// <summary>
    /// 获取授权设备列表
    /// </summary>
    /// <returns></returns>
    public List<AuthorizedDevice> GetAuthorizedDeviceList()
    {
        var authorizeds = new List<AuthorizedDevice>();
        try
        {
            if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
            {
                var v = VdfHelper.Read(ConfigVdfPath);
                var authorizedDevice = v["AuthorizedDevice"];
                if (authorizedDevice != null)
                {
                    var index = 0;
                    foreach (var item in (IEnumerable<KVObject>)authorizedDevice)
                    {
                        try
                        {
                            authorizeds.Add(new AuthorizedDevice()
                            {
                                Index = index,
                                SteamId3_Int = Convert.ToInt64(item.Name.ToString()),
                                Timeused = (long)item["timeused"],
                                Description = (string)item["description"],
                                Tokenid = (string)item["tokenid"],
                            });
                            index++;
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "GetAuthorizedDeviceList fail(0).");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetAuthorizedDeviceList fail(1).");
        }
        return authorizeds;
    }

    /// <summary>
    /// 设置用户是否不可见
    /// </summary>
    /// <param name="steamId32">要更新的用户的 SteamID </param>
    /// <param name="ePersonaState">用户的 Persona 状态枚举（0-7）</param>
    public void SetPersonaState(string steamId32, PersonaState ePersonaState)
    {
        if (string.IsNullOrEmpty(SteamDirPath)) return;
        if (ePersonaState == PersonaState.Default) return;
        // Values:
        // 0: Offline, 1: Online, 2: Busy, 3: Away, 4: Snooze, 5: Looking to Trade, 6: Looking to Play, 7: Invisible
        var localConfigFilePath = Path.Combine(SteamDirPath, UserDataDirectory, steamId32, "config", "localconfig.vdf");
        if (!File.Exists(localConfigFilePath)) return;
        var localConfigText = File.ReadAllText(localConfigFilePath); // Read relevant localconfig.vdf

        // Find index of range needing to be changed.
        var positionOfVar = localConfigText.IndexOf("ePersonaState", StringComparison.Ordinal); // Find where the variable is being set
        if (positionOfVar == -1) return;
        var indexOfBefore = localConfigText.IndexOf(":", positionOfVar, StringComparison.Ordinal) + 1; // Find where the start of the variable's value is
        var indexOfAfter = localConfigText.IndexOf(",", positionOfVar, StringComparison.Ordinal); // Find where the end of the variable's value is

        // The variable is now in-between the above numbers. Remove it and insert something different here.
        var sb = new StringBuilder(localConfigText);
        _ = sb.Remove(indexOfBefore, indexOfAfter - indexOfBefore);
        _ = sb.Insert(indexOfBefore, (int)ePersonaState);
        localConfigText = sb.ToString();

        // Output
        File.WriteAllText(localConfigFilePath, localConfigText);
    }

    /// <summary>
    /// 删除本地用户数据
    /// </summary>
    /// <param name="user"></param>
    /// <param name="isDeleteUserData"></param>
    public void DeleteLocalUserData(SteamUser user, bool isDeleteUserData = false)
    {
        if (string.IsNullOrWhiteSpace(UserVdfPath) || string.IsNullOrWhiteSpace(SteamDirPath))
        {
            return;
        }
        else
        {
            try
            {
                var v = VdfHelper.Read(UserVdfPath);
                var users = v.Children;
                if (users.Any_Nullable())
                {
                    var item = users.FirstOrDefault(s => s.Name == user.SteamId64.ToString());
                    if (item != null)
                    {
                        v.Remove(user.SteamId64.ToString());
                        VdfHelper.Write(UserVdfPath, v);

                        if (isDeleteUserData)
                        {
                            var temp = Path.Combine(SteamDirPath, UserDataDirectory, user.SteamId32.ToString());
                            if (Directory.Exists(temp))
                            {
                                Directory.Delete(temp, true);
                            }
                        }
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "GetUserVdfPath for Delete catch.");
            }
        }
    }

    /// <summary>
    /// 更新本地用户数据
    /// </summary>
    /// <param name="users"></param>
    public void UpdateLocalUserData(IEnumerable<SteamUser> users)
    {
        if (string.IsNullOrWhiteSpace(UserVdfPath) || !File.Exists(UserVdfPath))
        {
            return;
        }
        else
        {
            var v = VdfHelper.Read(UserVdfPath);
            var models = v.Children;
            if (models.Any_Nullable())
            {
                foreach (var item in models)
                {
                    try
                    {
                        var itemUser = users.FirstOrDefault(x => x.SteamId64.ToString() == item.Name);
                        if (itemUser == null)
                        {
                            item["MostRecent"] = 0;
                            continue;
                        }
                        item["AllowAutoLogin"] = 1;
                        item["MostRecent"] = Convert.ToInt16(itemUser.MostRecent);
                        item["WantsOfflineMode"] = Convert.ToInt16(itemUser.WantsOfflineMode);
                        item["SkipOfflineModeWarning"] = Convert.ToInt16(itemUser.SkipOfflineModeWarning);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "GetUserVdfPath for catch.");
                    }
                }

                VdfHelper.Write(UserVdfPath, v);
            }
            //关闭 Steam 询问
            UpdateAlwaysShowUserChooser();
        }
    }

    /// <summary>
    /// 关闭 Steam 每次启动 Steam 时询问使用哪个账户
    /// </summary>
    public void UpdateAlwaysShowUserChooser()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
            {
                var v = VdfHelper.Read(ConfigVdfPath);
                var webStorage = v.Children.FirstOrDefault(x => x.Name == "WebStorage");
                if (webStorage != null)
                {
                    var auth = webStorage.Children.FirstOrDefault(x => x.Name == "Auth");
                    if (auth != null)
                    {
                        auth.Set("AlwaysShowUserChooser", 0);
                        VdfHelper.Write(ConfigVdfPath, v);
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateAlwaysShowUserChooser fail(0).");
        }
    }

    /// <summary>
    /// 监视本地用户数据更改
    /// </summary>
    /// <param name="changedAction"></param>
    /// <exception cref="Exception"></exception>
    public void WatchLocalUserDataChange(Action changedAction)
    {
        if (string.IsNullOrWhiteSpace(SteamDirPath))
        {
            throw new Exception("Steam Dir Path is null or empty.");
        }

        var fsw = new FileSystemWatcher(Path.Combine(SteamDirPath, "config"), "loginusers.vdf")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
        };

        fsw.Created += Fsw_Changed;
        fsw.Renamed += Fsw_Changed;
        fsw.Changed += Fsw_Changed;
        fsw.EnableRaisingEvents = true;

        void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            changedAction.Invoke();
        }
    }

    private uint univeseNumber;
    private const uint MagicNumber = 123094055U;
    private const uint MagicNumberV2 = 123094056U;

    private static readonly Lazy<uint[]> MagicNumbers = new(new uint[] { MagicNumber, MagicNumberV2 });

    /// <summary>
    /// 从steam本地客户端缓存文件中读取游戏数据
    /// </summary>
    public /*async*/ Task<List<SteamApp>> GetAppInfos(bool isSaveProperties = false)
    {
        return Task.FromResult(GetAppInfos_());
        List<SteamApp> GetAppInfos_()
        {
            var apps = new List<SteamApp>();
            try
            {
                if (string.IsNullOrEmpty(AppInfoPath) && !File.Exists(AppInfoPath))
                    return apps;
                using var stream = IOPath.OpenRead(AppInfoPath);
                if (stream == null)
                {
                    return apps;
                }
                using BinaryReader binaryReader = new(stream);
                uint num = binaryReader.ReadUInt32();
                if (!MagicNumbers.Value.Contains(num))
                {
                    var msg = string.Format("\"{0}\" magic code is not supported: 0x{1:X8}", Path.GetFileName(AppInfoPath), num);
                    logger.LogError($"{nameof(GetAppInfos)} msg: {{msg}}", msg);
                    Toast.Show(ToastIcon.Info, msg, ToastLength.Long);
                    return apps;
                }
                SteamApp? app = new();
                univeseNumber = binaryReader.ReadUInt32();
                var installAppIds = GetInstalledAppIds();
                while ((app = SteamAppExtensions.FromReader(binaryReader, installAppIds, isSaveProperties, num == MagicNumberV2)) != null)
                {
                    if (app.AppId > 0)
                    {
                        if (!isSaveProperties)
                        {
                            //if (GameLibrarySettings.DefaultIgnoreList.Value.Contains(app.AppId))
                            //    continue;
                            if (HideGameList != null && HideGameList.ContainsKey(app.AppId))
                                continue;
                            //if (app.ParentId > 0)
                            //{
                            //    var parentApp = apps.FirstOrDefault(f => f.AppId == app.ParentId);
                            //    if (parentApp != null)
                            //        parentApp.ChildApp.Add(app.AppId);
                            //    //continue;
                            //}
                        }
                        apps.Add(app);
                        //app.Modified += (s, e) =>
                        //{
                        //};
                    }
                }
                return apps;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(GetAppInfos));
                return apps;
            }
        }
    }

    /// <summary>
    /// 获取修改的应用程序
    /// </summary>
    /// <returns></returns>
    public List<ModifiedApp>? GetModifiedApps()
    {
        try
        {
            var file = Path.Combine(IOPath.AppDataDirectory, ModifiedFileName);
            if (!File.Exists(file))
            {
                return null;
            }

            using FileStream modifiedFileStream = File.Open(file, FileMode.Open, FileAccess.Read);
            return Serializable.DMP<List<ModifiedApp>>(modifiedFileStream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, nameof(GetModifiedApps));
        }
        return null;
    }

    /// <summary>
    /// 保存修改后的游戏数据到 Steam 本地客户端缓存文件
    /// </summary>
    public async Task<bool> SaveAppInfosToSteam()
    {
        if (string.IsNullOrEmpty(AppInfoPath) || !File.Exists(AppInfoPath) || !Conn.SteamApps.Items.Any_Nullable())
            return false;

        //var bakFile = AppInfoPath + ".bak";

        try
        {
            //File.Copy(AppInfoPath, bakFile, true);

            var applist = await GetAppInfos(true);
            var editApps = Conn.SteamApps.Items.Where(s => s.IsEdited);
            var modifiedApps = new List<ModifiedApp>();

            using BinaryWriter binaryWriter = new(new MemoryStream());
            binaryWriter.Write(MagicNumber);
            binaryWriter.Write(univeseNumber);

            foreach (SteamApp app in applist)
            {
                var editApp = editApps.FirstOrDefault(s => s.AppId == app.AppId);
                if (editApp != null)
                {
                    app.SetEditProperty(editApp);
                    modifiedApps.Add(new ModifiedApp(app));
                }
                app.Write(binaryWriter);
            }

            binaryWriter.Write(0);
            using FileStream fileStream = File.Open(AppInfoPath, FileMode.Create, FileAccess.Write);
            binaryWriter.BaseStream.Position = 0L;
            await binaryWriter.BaseStream.CopyToAsync(fileStream);
            fileStream.Close();
            binaryWriter.Close();

            using FileStream modifiedFileStream = File.Open(Path.Combine(IOPath.AppDataDirectory, ModifiedFileName), FileMode.Create, FileAccess.Write);
            modifiedFileStream.Write(Serializable.SMP(modifiedApps));
            modifiedFileStream.Close();

            applist = null;
            modifiedApps = null;
            editApps = null;

            GC.Collect();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(SaveAppInfosToSteam)} 保存 AppInfos 出现错误");
            Toast.Show(ToastIcon.Error, GetString("SaveEditedAppInfo_SaveFailed")!);
            //if (File.Exists(bakFile))
            //    File.Copy(bakFile, AppInfoPath, true);

            return false;
        }
        return true;
    }

    /// <summary>
    /// 获取已安装程序的 AppId
    /// </summary>
    /// <returns></returns>
    public uint[] GetInstalledAppIds()
    {
        return GetDownloadingAppList().Where(x => x.IsInstalled).Select(x => x.AppId).ToArray();
    }

    private string? GetAppCustomImageFilePath(uint appId, SteamUser user, LibCacheType type)
    {
        if (string.IsNullOrEmpty(SteamDirPath)) return null;

        var path = Path.Combine(SteamDirPath, UserDataDirectory,
            user.SteamId32.ToString(), "config", "grid");

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

    private string? GetAppLibCacheFilePath(uint appId, LibCacheType type)
    {
        if (LibrarycacheDirPath == null) return null;
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

        var filePath = Path.Combine(LibrarycacheDirPath, fileName);

        if (Conn.SteamLanguageString != null && !File.Exists(filePath))
        {
            //默认图没找到情况下尝试查找当前Steam客户端语言图片
            var lang = $"_{Conn.SteamLanguageString}";
            filePath = filePath.Insert(filePath.LastIndexOf('.'), lang);
        }

        return filePath;
    }

    /// <summary>
    /// 获取应用程序图片源
    /// </summary>
    /// <param name="app"></param>
    /// <param name="type"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<CommonImageSource?> GetAppImageAsync(SteamApp app, LibCacheType type, CancellationToken token = default)
    {
        var mostRecentUser = Conn.SteamUsers.Items.Where(s => s.MostRecent).FirstOrDefault();
        //var mostRecentUser = Conn.CurrentSteamUser;
        if (mostRecentUser != null)
        {
            var customFilePath = GetAppCustomImageFilePath(app.AppId, mostRecentUser, type);
            if (customFilePath != null && File.Exists(customFilePath))
                return customFilePath;
        }

        var cacheFilePath = GetAppLibCacheFilePath(app.AppId, type);
        if (cacheFilePath != null && File.Exists(cacheFilePath))
            return cacheFilePath;

        var url = type switch
        {
            LibCacheType.Header => app.HeaderLogoUrl,
            LibCacheType.Icon => app.IconUrl,
            LibCacheType.Library_Grid => app.LibraryGridUrl,
            LibCacheType.Library_Hero => app.LibraryHeroUrl,
            LibCacheType.Library_Hero_Blur => app.LibraryHeroBlurUrl,
            LibCacheType.Logo => app.LibraryLogoUrl,
            _ => null,
        };

        if (string.IsNullOrEmpty(url))
            return default;
        var value = await CommonImageSourceExtensions.GetAsync(url, cache: true, cacheFirst: true, cancellationToken: token);

        return value;
    }

    /// <summary>
    /// 保存图片流到 Steam 自定义封面文件夹
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SaveAppImageToSteamFile(object? imageObject, SteamUser user, long appId, SteamGridItemType gridType)
    {
        if (!string.IsNullOrEmpty(SteamDirPath))
        {
            var path = Path.Combine(SteamDirPath, UserDataDirectory,
                user.SteamId32.ToString(), "config", "grid");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filePath = gridType switch
            {
                SteamGridItemType.Hero => Path.Combine(path, $"{appId}_hero.png"),
                SteamGridItemType.Logo => Path.Combine(path, $"{appId}_logo.png"),
                SteamGridItemType.Grid => Path.Combine(path, $"{appId}p.png"),
                _ => Path.Combine(path, $"{appId}.png"),
            };
            try
            {
                if (imageObject == null)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        return true;
                    }
                    return false;
                }

                if (imageObject is Stream imageStream)
                {
                    using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    if (imageStream.Position > 0)
                    {
                        imageStream.Position = 0;
                    }
                    await imageStream.CopyToAsync(fs);
                    await fs.FlushAsync();
                    fs.Close();
                }
                else if (imageObject is string imagePath && File.Exists(imagePath))
                {
                    File.Copy(imagePath, filePath, true);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(SaveAppImageToSteamFile));
                return false;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 加载应用程序图片
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public /*async*/ ValueTask LoadAppImageAsync(SteamApp app)
    {
        return default(ValueTask);
        //if (app.LibraryLogoStream == null)
        //{
        //    app.LibraryLogoStream = await GetAppImageAsync(app, LibCacheType.Library_600x900);
        //}
        //if (app.LibraryHeaderStream == null)
        //{
        //    app.LibraryHeaderStream = await GetAppImageAsync(app, LibCacheType.Library_Hero);
        //}
        //if (app.LibraryHeaderBlurStream == null)
        //{
        //    app.LibraryHeaderBlurStream = await GetAppImageAsync(app, LibCacheType.Library_Hero_Blur);
        //}
        //if (app.LibraryNameStream == null)
        //{
        //    app.LibraryNameStream = await GetAppImageAsync(app, LibCacheType.Logo);
        //}
        //if (app.HeaderLogoStream == null)
        //{
        //    app.HeaderLogoStream = await GetAppImageAsync(app, LibCacheType.Header);
        //}
    }

    string[]? GetLibraryPaths()
    {
        const string dirname_steamapps = "steamapps"; // 文件夹名，linux上区分大小写

        if (string.IsNullOrEmpty(SteamDirPath) || !Directory.Exists(SteamDirPath))
        {
            return null;
        }

        List<string> paths = [Path.Combine(SteamDirPath, dirname_steamapps)];

        try
        {
            string libraryFoldersPath = Path.Combine(SteamDirPath, dirname_steamapps, "libraryfolders.vdf");
            if (File.Exists(libraryFoldersPath))
            {
                var v = VdfHelper.Read(libraryFoldersPath);

                for (int i = 1; ; i++)
                {
                    try
                    {
                        var pathNode = v[i.ToString()];

                        if (pathNode == null) break;

                        var path = pathNode["path"].ToString();

                        if (!string.IsNullOrEmpty(path))
                        {
                            // New format
                            // Valve introduced a new format for the "libraryfolders.vdf" file
                            // In the new format, the node "1" not only contains a single value (the path),
                            // but multiple values: path, label, mounted, contentid

                            // If a library folder is removed in the Steam settings, the path persists, but its 'mounted' value is set to 0 (disabled)
                            // We consider only the value '1' as that the path is actually enabled.
                            if (pathNode["mounted"] != null && pathNode["mounted"].ToString() != "1")
                                continue;

                            path = Path.Combine(path, dirname_steamapps);

                            if (Directory.Exists(path))
                                paths.Add(path);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "GetLibraryPaths for catch");
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetLibraryPaths Read libraryFoldersPath catch");
        }

        return paths.ToArray();
    }

    /// <summary>
    /// acf 文件转 SteamApp
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public SteamApp? FileToAppInfo(string filename)
    {
        try
        {
            string[] content = File.ReadAllLines(filename);
            // Skip if file contains only NULL bytes (this can happen sometimes, example: download crashes, resulting in a corrupted file)
            if (content.Length == 1 && string.IsNullOrWhiteSpace(content[0].TrimStart('\0'))) return null;

            var v = VdfHelper.Read(filename);

            if (v.Value == null)
            {
                Toast.Show(ToastIcon.Error, $"{filename}{Environment.NewLine}contains unexpected content.{Environment.NewLine}This game will be ignored.");
                return null;
            }

            string? installdir = v["installdir"].ToString();
            installdir.ThrowIsNull();
            var filenameDir = Path.GetDirectoryName(filename);
            var newInfo = new SteamApp
            {
                AppId = (uint)(v["appid"] ?? v["appID"] ?? v["AppId"]),
                Name = v["name"].ToString() ?? installdir,
                InstalledDir = Path.Combine(filenameDir.ThrowIsNull(), "common", installdir),
                State = (int)v["StateFlags"],
                SizeOnDisk = (long)v["SizeOnDisk"],
                LastOwner = (long)v["LastOwner"],
                BytesToDownload = (long)v["BytesToDownload"],
                BytesDownloaded = (long)v["BytesDownloaded"],
                BytesToStage = (long)v["BytesToStage"],
                BytesStaged = (long)v["BytesStaged"],
                LastUpdated = ((long)v["LastUpdated"]).ToDateTimeS(),
            };
            return newInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(FileToAppInfo)} filename: {{filename}}", filename);
            return null;
        }
    }

    /// <summary>
    /// 获取正在下载和已下载的 SteamApp 列表
    /// </summary>
    public List<SteamApp> GetDownloadingAppList()
    {
        var appInfos = new List<SteamApp>();
        try
        {
            var libraryPaths = GetLibraryPaths();
            if (!libraryPaths.Any_Nullable())
            {
                Toast.Show(ToastIcon.Info, $"No game library found.");
                return appInfos;
            }

            foreach (string path in libraryPaths)
            {
                var di = new DirectoryInfo(path);
                if (!di.Exists) continue;

                foreach (var fileInfo in di.EnumerateFiles("*.acf"))
                {
                    // Skip if file is empty
                    if (fileInfo.Length == 0) continue;

                    var ai = FileToAppInfo(fileInfo.FullName);
                    if (ai == null) continue;

                    appInfos.Add(ai);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetDownloadingAppList error.");
        }
        return appInfos;
    }

    /// <summary>
    /// acf 文件名格式中提取 appid
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    static uint IdFromAcfFilename(string filename)
    {
        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        int loc = filenameWithoutExtension.IndexOf('_');
        _ = uint.TryParse(filenameWithoutExtension[(loc + 1)..], out uint appid);
        return appid;
    }

    /// <summary>
    /// 监听 Steam 下载
    /// </summary>
    public void StartWatchSteamDownloading(Action<SteamApp> changedAction, Action<uint> deleteAction)
    {
        if (!steamDownloadingWatchers.Any_Nullable())
        {
            steamDownloadingWatchers = [];
        }
        var libraryPaths = GetLibraryPaths();
        if (!libraryPaths.Any_Nullable())
        {
            ToastHelper.Show(ToastIcon.Info, "No game library found.");
        }

        foreach (string libraryFolder in libraryPaths!)
        {
            var fsw = new FileSystemWatcher(libraryFolder, "*.acf")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            };
            //fsw.Created += Fsw_Changed
            fsw.Renamed += Fsw_Changed;
            fsw.Changed += Fsw_Changed;
            fsw.Deleted += Fsw_Deleted;
            fsw.EnableRaisingEvents = true;
            steamDownloadingWatchers.Add(fsw);
        }

        void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            SteamApp? app = null;
            try
            {
                // This is necessary because sometimes the file is still accessed by steam, so let's wait for 10 ms and try again.
                // Maximum 5 times
                int counter = 1;
                do
                {
                    try
                    {
                        app = FileToAppInfo(e.FullPath);
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50);
                    }
                }
                while (counter++ <= 5);
            }
            catch
            {
                return;
            }

            // Shouldn't happen, but might occur if Steam holds the acf file too long
            if (app == null) return;

            // Search for changed app, if null it's a new app
            //SteamApp info = Apps.FirstOrDefault(x => x.ID == newID);
            //uint appId = GetAppId(v);
            changedAction.Invoke(app);

            //if (info != null) // Download state changed
            //{
            //    eventArgs = new AppInfoChangedEventArgs(info, info.State);
            //    // Only update existing AppInfo
            //    info.State = int.Parse(v.StateFlags.ToString());
            //}
            //else // New download started
            //{
            //    // Add new AppInfo
            //    info = JsonToAppInfo(newJson);
            //    Apps.Add(info);
            //    eventArgs = new AppInfoChangedEventArgs(info, -1);
            //}

            //OnAppInfoChanged(info, eventArgs);
        }

        void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            uint id = IdFromAcfFilename(e.FullPath);

            //SteamApp info = Apps.FirstOrDefault(x => x.ID == id);
            //if (info == null) return;

            //var eventArgs = new AppInfoEventArgs(info);
            deleteAction.Invoke(id);
        }
    }

    /// <summary>
    /// 结束监听 Steam 下载
    /// </summary>
    public void StopWatchSteamDownloading()
    {
        if (steamDownloadingWatchers.Any_Nullable())
        {
            foreach (var fsw in steamDownloadingWatchers)
            {
                fsw.EnableRaisingEvents = false;
                fsw.Dispose();
            }
            steamDownloadingWatchers.Clear();
            steamDownloadingWatchers = null;
        }
    }
}
#endif