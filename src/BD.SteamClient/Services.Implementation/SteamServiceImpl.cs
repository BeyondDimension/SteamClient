#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using ValveKeyValue;

namespace BD.SteamClient.Services.Implementation;

public abstract partial class SteamServiceImpl : ISteamService
{
    const string TAG = "SteamS";

    const string ModifiedFileName = "modifications.vdf";

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
    readonly string? UserVdfPath;
    readonly string? ConfigVdfPath;
    readonly string? AppInfoPath;
    readonly string? LibrarycacheDirPath;
    const string UserDataDirectory = "userdata";
    readonly string? mSteamDirPath;
    readonly string? mSteamProgramPath;
    readonly string? mRegistryVdfPath;
    readonly string[] steamProcess = new[] {
#if MACCATALYST || MACOS
        "steam_osx",
#else
        "steam",
#endif
        "steamservice",
        "steamwebhelper",
        "GameOverlayUI",
    };

    readonly ILogger logger;

    List<FileSystemWatcher>? steamDownloadingWatchers;

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

    public string? SteamDirPath => mSteamDirPath;

    /// <summary>
    /// 非windows平台 steam 注册表配置路径
    /// </summary>
    public string? RegistryVdfPath => mRegistryVdfPath;

    public string? SteamProgramPath => mSteamProgramPath;

    public bool IsRunningSteamProcess => GetSteamProcesses().Any();

    public void KillSteamProcess()
    {
        foreach (var p in steamProcess)
        {
            var process = Process.GetProcessesByName(p);
            foreach (var item in process)
            {
                if (!item.HasExited)
                {
                    item.Kill();
                    item.WaitForExit();
                }
            }
        }
    }

    public bool TryKillSteamProcess()
    {
        try
        {
            KillSteamProcess();
            return true;
            //if (IsRunningSteamProcess)
            //{
            //    Process closeProc = Process.Start(new ProcessStartInfo(SteamProgramPath, "-shutdown"));
            //    bool closeProcSuccess = closeProc != null && closeProc.WaitForExit(3000);
            //    return closeProcSuccess;
            //}
            //return false;
        }
        catch (Exception e)
        {
            logger.LogError(e, "KillSteamProcess fail.");
            return false;
        }
        finally
        {
            Conn.IsConnectToSteam = false;
        }
    }

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

    public string GetLastLoginUserName() => GetLastSteamLoginUserName();

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

    public bool UpdateAuthorizedDeviceList(IEnumerable<AuthorizedDevice> model)
    {
        var authorizeds = new List<AuthorizedDevice>();
        try
        {
            if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
            {
                var v = VdfHelper.Read(ConfigVdfPath);
                var lists = new KVCollectionValue();
                foreach (var item in model.OrderBy(x => x.Index))
                {
                    KVObject itemTemp = new KVObject(item.SteamId3_Int.ToString(), new KVObject[]
                    {
                            new KVObject("timeused", (KVValue)item.Timeused),
                            new KVObject("description", (KVValue)item.Description),
                            new KVObject("tokenid", (KVValue)item.Tokenid),
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCurrentUser(string userName) => SetSteamCurrentUser(userName);

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
                var users = (IEnumerable<KVObject>)v["users"];
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

    public void UpdateLocalUserData(IEnumerable<SteamUser> users)
    {
        if (string.IsNullOrWhiteSpace(UserVdfPath) || !File.Exists(UserVdfPath))
        {
            return;
        }
        else
        {
            var v = VdfHelper.Read(UserVdfPath);
            var models = v["users"] as KVCollectionValue;
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
                            break;
                        }
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
        }
    }

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
                    Toast.Show(msg, ToastLength.Long);
                    return apps;
                }
                SteamApp? app = new();
                univeseNumber = binaryReader.ReadUInt32();
                var installAppIds = GetInstalledAppIds();
                while ((app = SteamApp.FromReader(binaryReader, installAppIds, isSaveProperties, num == MagicNumberV2)) != null)
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
            Toast.Show(GetString("SaveEditedAppInfo_SaveFailed")!);
            //if (File.Exists(bakFile))
            //    File.Copy(bakFile, AppInfoPath, true);

            return false;
        }
        return true;
    }

    public uint[] GetInstalledAppIds()
    {
        return GetDownloadingAppList().Where(x => x.IsInstalled).Select(x => x.AppId).ToArray();
    }

    private string? GetAppCustomImageFilePath(uint appId, SteamUser user, SteamApp.LibCacheType type)
    {
        if (string.IsNullOrEmpty(SteamDirPath)) return null;

        var path = Path.Combine(SteamDirPath, UserDataDirectory,
            user.SteamId32.ToString(), "config", "grid");

        var fileName = type switch
        {
            SteamApp.LibCacheType.Header => $"{appId}.png",
            SteamApp.LibCacheType.Library_Grid => $"{appId}p.png",
            SteamApp.LibCacheType.Library_Hero => $"{appId}_hero.png",
            SteamApp.LibCacheType.Logo => $"{appId}_logo.png",
            _ => null,
        };

        if (fileName == null)
        {
            return null;
        }

        var filePath = Path.Combine(path, fileName);
        return filePath;
    }

    private string? GetAppLibCacheFilePath(uint appId, SteamApp.LibCacheType type)
    {
        if (LibrarycacheDirPath == null) return null;
        var fileName = type switch
        {
            SteamApp.LibCacheType.Header => $"{appId}_header.jpg",
            SteamApp.LibCacheType.Icon => $"{appId}_icon.jpg",
            SteamApp.LibCacheType.Library_Grid => $"{appId}_library_600x900.jpg",
            SteamApp.LibCacheType.Library_Hero => $"{appId}_library_hero.jpg",
            SteamApp.LibCacheType.Library_Hero_Blur => $"{appId}_library_hero_blur.jpg",
            SteamApp.LibCacheType.Logo => $"{appId}_logo.png",
            _ => null,
        };

        if (fileName == null)
        {
            return null;
        }

        var filePath = Path.Combine(LibrarycacheDirPath, fileName);
        return filePath;
    }

    public async Task<ImageSource.ClipStream?> GetAppImageAsync(SteamApp app, SteamApp.LibCacheType type)
    {
        var mostRecentUser = Conn.SteamUsers.Items.Where(s => s.MostRecent).FirstOrDefault();
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
            SteamApp.LibCacheType.Header => app.HeaderLogoUrl,
            SteamApp.LibCacheType.Icon => app.IconUrl,
            SteamApp.LibCacheType.Library_Grid => app.LibraryGridUrl,
            SteamApp.LibCacheType.Library_Hero => app.LibraryHeroUrl,
            SteamApp.LibCacheType.Library_Hero_Blur => app.LibraryHeroBlurUrl,
            SteamApp.LibCacheType.Logo => app.LibraryLogoUrl,
            _ => null,
        };

        if (url == default)
            return default;
        var value = await ImageSource.GetAsync(url);

        return value;
    }

    /// <summary>
    /// 保存图片流到 Steam 自定义封面文件夹
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SaveAppImageToSteamFile(object? imageObject, SteamUser user, long appId, SteamGridItemType gridType)
    {
        if (!string.IsNullOrEmpty(SteamDirPath) && imageObject != null)
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

    public /*async*/ ValueTask LoadAppImageAsync(SteamApp app)
    {
        return default(ValueTask);
        //if (app.LibraryLogoStream == null)
        //{
        //    app.LibraryLogoStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Library_600x900);
        //}
        //if (app.LibraryHeaderStream == null)
        //{
        //    app.LibraryHeaderStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Library_Hero);
        //}
        //if (app.LibraryHeaderBlurStream == null)
        //{
        //    app.LibraryHeaderBlurStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Library_Hero_Blur);
        //}
        //if (app.LibraryNameStream == null)
        //{
        //    app.LibraryNameStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Logo);
        //}
        //if (app.HeaderLogoStream == null)
        //{
        //    app.HeaderLogoStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Header);
        //}
    }

    string[]? GetLibraryPaths()
    {
        const string dirname_steamapps = "steamapps"; // 文件夹名，linux上区分大小写

        if (string.IsNullOrEmpty(SteamDirPath) || !Directory.Exists(SteamDirPath))
        {
            return null;
        }

        List<string> paths = new()
            {
                Path.Combine(SteamDirPath, dirname_steamapps),
            };

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
                Toast.Show(
                    $"{filename}{Environment.NewLine}contains unexpected content.{Environment.NewLine}This game will be ignored.");
                return null;
            }

            string? installdir = v["installdir"].ToString();
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
                Toast.Show($"No game library found.");
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
            steamDownloadingWatchers = new List<FileSystemWatcher>();
        }
        var libraryPaths = GetLibraryPaths();
        if (!libraryPaths.Any_Nullable())
        {
            Toast.Show("No game library found.");
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