#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using Polly;
using System.IO;
using ValveKeyValue;
using static SteamKit2.DepotManifest;
using static SteamKit2.Internal.CStoreBrowse_GetStoreCategories_Response;

namespace BD.SteamClient.Services.Implementation;

public abstract partial class SteamServiceImpl : ISteamService
{
    protected const string TAG = "SteamS";

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
    protected string? UserVdfPath => CombineSteamPath(SteamDirPath, "config", "loginusers.vdf");

    protected string? ConfigVdfPath => CombineSteamPath(SteamDirPath, "config", "config.vdf");

    protected const string UserDataDirectory = "userdata";

    protected virtual string? ConfiguredSteamProgramPath => null;

    readonly string? detectedSteamDirectory;
    readonly string? detectedSteamProgram;
    SteamDirectoryCache? steamDirectoryCache;

    sealed record SteamDirectoryCache(string? Configured, string? Directory);

    static string? CombineSteamPath(string? directory, string subdirectory, string name)
        => directory == null ? null : Path.Combine(directory, subdirectory, name);

    protected readonly string[] steamProcess = new[]
    {
#if MACCATALYST || MACOS
        "steam_osx",
#else
        "steam",
#endif
        "steamservice", "steamwebhelper", "GameOverlayUI",
    };

    protected readonly ILogger logger;

    protected List<FileSystemWatcher>? steamDownloadingWatchers;

    public SteamServiceImpl(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(TAG);
        detectedSteamDirectory = GetSteamDirPath();
        detectedSteamProgram = GetSteamProgramPath();
    }

    public string? SteamDirPath
    {
        get
        {
            var configured = ConfiguredSteamProgramPath;
            var cached = Volatile.Read(ref steamDirectoryCache);
            if (cached != null && string.Equals(cached.Configured, configured, StringComparison.Ordinal))
                return cached.Directory;
            var directory = GetEffectiveSteamDirPath(configured);
            Volatile.Write(ref steamDirectoryCache, new SteamDirectoryCache(configured, directory));
            return directory;
        }
    }

    /// <summary>非 Windows 平台 Steam 注册表配置路径。</summary>
    public string? RegistryVdfPath
    {
        get
        {
            var configured = ConfiguredSteamProgramPath;
            if (string.IsNullOrWhiteSpace(configured) || IsSystemSteamProgramPath(configured))
                return GetRegistryVdfPath();
#if LINUX
            var directory = GetEffectiveSteamDirPath(configured);
            return directory == null ? null : Path.Combine(directory, "registry.vdf");
#elif MACCATALYST || MACOS
            var directory = GetEffectiveSteamDirPath(configured);
            return directory == null ? null : Path.Combine(directory, "registry.vdf");
#else
            return null;
#endif
        }
    }

    public string? SteamProgramPath
    {
        get
        {
            var configured = ConfiguredSteamProgramPath;
            return string.IsNullOrWhiteSpace(configured) ? detectedSteamProgram : configured;
        }
    }

    public bool IsRunningSteamProcess => GetSteamProcesses().Any();

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
            if (processes.Any())
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
                if (innerExceptions.Any())
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
        using var process = GetSteamProces();
        if (process != null)
        {
            try
            {
                // Query the native process handle directly; launcher detection must not require WMI.
                return WindowsProcessCommandLine.Read(process.Id).Contains("-steamchina", StringComparison.OrdinalIgnoreCase);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode is 5 or 87 or 1067)
            {
                // 没有进程访问权限，或进程已退出。
                return false;
            }
            catch (InvalidOperationException)
            {
                // 进程已退出。
                return false;
            }
        }
#endif
        return false;
    }

    public void StartSteam(string? arguments = null)
    {
        var steamProgramPath = this.SteamProgramPath;
        if (!string.IsNullOrWhiteSpace(steamProgramPath) && File.Exists(steamProgramPath))
        {
            if (OperatingSystem.IsWindows() && !IsRunSteamAdministrator)
            {
                StartAsInvoker(steamProgramPath, arguments);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process2.StartPath("Steam", steamProgramPath, arguments);
            }
            else
            {
                Process2.Start(steamProgramPath, arguments, useShellExecute: true);
            }
        }
    }

    public async Task ShutdownSteamAsync(CancellationToken cancellationToken = default)
    {
        var steamProgramPath = this.SteamProgramPath;
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
        TryGetRememberUserList(out var users);
        return users;
    }

    public bool TryGetRememberUserList(out List<SteamUser> users)
    {
        users = new List<SteamUser>();
        var steamDirectory = SteamDirPath;
        if (steamDirectory == null)
            return false;
        var userVdfPath = Path.Combine(steamDirectory, "config", "loginusers.vdf");
        var invalidEntry = false;
        try
        {
            var savedUsers = VdfHelper.Read(userVdfPath);
            foreach (var item in savedUsers)
            {
                try
                {
                    long timestamp = item["timestamp"] != null ? (long)item["timestamp"] : (long)item["Timestamp"];
                    var autoLogin = ReadAutoLogin(item);
                    var personaName = (string)item["PersonaName"];
                    users.Add(new SteamUser
                    {
                        SteamId64 = Convert.ToInt64(item.Name),
                        AccountName = (string)item["AccountName"],
                        SteamID = personaName,
                        PersonaName = personaName,
                        RememberPassword = item["RememberPassword"] != null && (bool)item["RememberPassword"],
                        AutoLogin = autoLogin,
                        AllowAutoLogin = item["AllowAutoLogin"] != null ? (bool)item["AllowAutoLogin"] : autoLogin,
                        MostRecent = item["mostrecent"] != null ? (bool)item["mostrecent"] :
                            item["MostRecent"] != null ? (bool)item["MostRecent"] : autoLogin,
                        Timestamp = timestamp,
                        LastLoginTime = timestamp.ToDateTimeS(),
                        WantsOfflineMode = (bool)item["WantsOfflineMode"],
                        SkipOfflineModeWarning = true,
                    });
                }
                catch (Exception e)
                {
                    invalidEntry = true;
                    logger.LogError(e, "Could not read a saved Steam user.");
                }
            }
        }
        catch (FileNotFoundException)
        {
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            return Directory.Exists(steamDirectory);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetRememberUserList fail(1).");
            return false;
        }

        return !invalidEntry || users.Count > 0;
    }

    public bool UpdateAuthorizedDeviceList(IEnumerable<AuthorizedDevice> items)
    {
        var configVdfPath = this.ConfigVdfPath;
        var authorizeds = new List<AuthorizedDevice>();
        try
        {
            if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
            {
                var v = VdfHelper.Read(ConfigVdfPath);
                var lists = new KVCollectionValue();
                foreach (var item in items.OrderBy(x => x.Index))
                {
                    KVObject itemTemp = new KVObject(item.SteamId3_Int.ToString(),
                        new KVObject[]
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
        var configVdfPath = this.ConfigVdfPath;
        try
        {
            if (!string.IsNullOrWhiteSpace(configVdfPath) && File.Exists(configVdfPath))
            {
                var v = VdfHelper.Read(configVdfPath);
                var authorizedDevices = v["AuthorizedDevice"] as KVCollectionValue;
                if (authorizedDevices != null)
                {
                    authorizedDevices.Remove(model.SteamId3_Int.ToString());
                    //v["AuthorizedDevice"] = authorizedDevices;
                    VdfHelper.Write(configVdfPath, v);
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
        var configVdfPath = this.ConfigVdfPath;
        var authorizeds = new List<AuthorizedDevice>();
        try
        {
            if (!string.IsNullOrWhiteSpace(configVdfPath) && File.Exists(configVdfPath))
            {
                var v = VdfHelper.Read(configVdfPath);
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
    /// Sets whether the user is invisible or not
    /// </summary>
    /// <param name="steamId32">SteamID of user to update</param>
    /// <param name="ePersonaState">Persona state enum for user (0-7)</param>
    public void SetPersonaState(string steamId32, PersonaState ePersonaState)
    {
        var steamDirPath = this.SteamDirPath;
        if (string.IsNullOrEmpty(SteamDirPath)) return;
        if (ePersonaState == PersonaState.Default) return;
        // Values:
        // 0: Offline, 1: Online, 2: Busy, 3: Away, 4: Snooze, 5: Looking to Trade, 6: Looking to Play, 7: Invisible
        var localConfigFilePath = Path.Combine(SteamDirPath, UserDataDirectory, steamId32, "config", "localconfig.vdf");
        if (!File.Exists(localConfigFilePath)) return;
        var localConfigText = File.ReadAllText(localConfigFilePath); // Read relevant localconfig.vdf

        // Find index of range needing to be changed.
        var positionOfVar =
            localConfigText.IndexOf("ePersonaState", StringComparison.Ordinal); // Find where the variable is being set
        if (positionOfVar == -1) return;
        var indexOfBefore =
            localConfigText.IndexOf(":", positionOfVar, StringComparison.Ordinal) +
            1; // Find where the start of the variable's value is
        var indexOfAfter =
            localConfigText.IndexOf(",", positionOfVar,
                StringComparison.Ordinal); // Find where the end of the variable's value is

        // The variable is now in-between the above numbers. Remove it and insert something different here.
        var sb = new StringBuilder(localConfigText);
        _ = sb.Remove(indexOfBefore, indexOfAfter - indexOfBefore);
        _ = sb.Insert(indexOfBefore, (int)ePersonaState);
        localConfigText = sb.ToString();

        // Output
        File.WriteAllText(localConfigFilePath, localConfigText);
    }

    public void DeleteLocalUserData(SteamUser user, bool isDeleteUserData = false)
    {
        var steamDirPath = this.SteamDirPath;
        var userVdfPath = CombineSteamPath(SteamDirPath, "config", "loginusers.vdf");
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

    static bool ReadAutoLogin(KVObject item)
    {
        if (item["AutoLogin"] != null)
            return (bool)item["AutoLogin"];
        return item["AllowAutoLogin"] != null && (bool)item["AllowAutoLogin"];
    }

    public void UpdateLocalUserData(IEnumerable<SteamUser> users)
    {
        var steamDirPath = this.SteamDirPath;
        var userVdfPath = CombineSteamPath(SteamDirPath, "config", "loginusers.vdf");
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

                        item["AutoLogin"] = Convert.ToInt16(itemUser.AutoLogin);
                        item["AllowAutoLogin"] = 1; //暂时保留旧版兼容，之后移除
                        item["MostRecent"] = Convert.ToInt16(itemUser.MostRecent); //暂时保留旧版兼容，之后移除
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
            UpdateAlwaysShowUserChooser(CombineSteamPath(SteamDirPath, "config", "config.vdf"));
        }
    }

    /// <summary>
    /// 关闭 Steam 每次启动 Steam 时询问使用哪个账户
    /// </summary>
    public void UpdateAlwaysShowUserChooser() => UpdateAlwaysShowUserChooser(ConfigVdfPath);

    void UpdateAlwaysShowUserChooser(string? ConfigVdfPath)
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

    public void WatchLocalUserDataChange(Action changedAction)
    {
        var steamDirPath = this.SteamDirPath;
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

    private const uint MagicNumber = 123094055U;
    private const uint MagicNumberV2 = 123094056U;
    private const uint MagicNumberV3 = 123094057U;

    private static readonly Lazy<uint[]> MagicNumbers = new([MagicNumber, MagicNumberV2, MagicNumberV3]);

    /// <summary>
    /// 从 Steam 本地客户端缓存文件中读取游戏数据；解析在后台执行，不阻塞 UI 调用线程。
    /// </summary>
    public Task<List<SteamApp>> GetAppInfos(bool isSaveProperties = false)
    {
        var steamDirectory = SteamDirPath;
        return Task.Run(() =>
        {
            var apps = new List<SteamApp>();
            try
            {
                ReadAppInfos(steamDirectory, apps.Add, isSaveProperties, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(GetAppInfos));
            }

            return apps;
        });
    }

    public Task ReadAppInfosInBatchesAsync(Action<IReadOnlyList<SteamApp>> onBatch,
        CancellationToken cancellationToken = default)
    {
        if (onBatch == null)
            throw new ArgumentNullException(nameof(onBatch));
        var steamDirectory = SteamDirPath;

        return Task.Run(() =>
        {
            const int batchSize = 128;
            var batch = new List<SteamApp>(batchSize);
            ReadAppInfos(steamDirectory, app =>
            {
                batch.Add(app);
                if (batch.Count == batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    onBatch(batch);
                    batch.Clear();
                }
            }, false, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (batch.Count != 0)
                onBatch(batch);
            cancellationToken.ThrowIfCancellationRequested();
        }, cancellationToken);
    }

    private void ReadAppInfos(string? steamDirectory, Action<SteamApp> onApp, bool isSaveProperties,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (steamDirectory == null)
            throw new FileNotFoundException("Steam appinfo path is not available.");

        var installedAppIds = GetInstalledAppIds(steamDirectory);
        var hiddenApps = isSaveProperties ? null : HideGameList;
        // Scope sharing to this read; the values from previous libraries are not rooted.
        var sharedMetadata = isSaveProperties ? null : new Dictionary<string, string>(StringComparer.Ordinal);
        void Publish(SteamApp app)
        {
            if (app.AppId == 0 || hiddenApps?.ContainsKey(app.AppId) == true)
                return;

            if (sharedMetadata != null)
            {
                app.Developer = ShareMetadataString(app.Developer, sharedMetadata);
                app.Publisher = ShareMetadataString(app.Publisher, sharedMetadata);
                app.OSList = ShareMetadataString(app.OSList, sharedMetadata);
            }
            onApp(app);
        }

        // After migration an older appinfo.vdf can remain; don't mix its entries with the shards.
        var hasShards = false;
        foreach (var path in EnumerateAppInfoShards(steamDirectory))
        {
            hasShards = true;
            ReadAppInfoShard(path, installedAppIds, Publish, isSaveProperties, cancellationToken);
        }
        if (!hasShards)
            ReadLegacyAppInfos(Path.Combine(steamDirectory, "appcache", "appinfo.vdf"), installedAppIds,
                Publish, isSaveProperties, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
    }

    HashSet<uint> GetInstalledAppIds(string? steamDirectory) => GetDownloadingAppList(steamDirectory)
        .Where(static app => app.IsInstalled).Select(static app => app.AppId).ToHashSet();

    uint ReadLegacyAppInfos(string appInfoPath, IReadOnlySet<uint> installedAppIds, Action<SteamApp> onApp,
        bool isSaveProperties, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(appInfoPath, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        if (stream.Length == 0)
            return 0;

        using BinaryReader binaryReader = new(stream, Encoding.UTF8, true);
        uint num = binaryReader.ReadUInt32();
        if (!MagicNumbers.Value.Contains(num))
        {
            var msg = string.Format("\"{0}\" magic code is not supported: 0x{1:X8}", Path.GetFileName(appInfoPath), num);
            logger.LogError($"{nameof(GetAppInfos)} msg: {{msg}}", msg);
            Toast.Show(ToastIcon.Error, msg);
            throw new InvalidDataException(msg);
        }

        var universe = binaryReader.ReadUInt32();

        string[]? stringPool = null;
        if (num == MagicNumberV3)
        {
            var stringTableOffset = binaryReader.ReadInt64();
            var offset = binaryReader.BaseStream.Position;
            binaryReader.BaseStream.Position = stringTableOffset;
            var stringCount = binaryReader.ReadUInt32();
            stringPool = new string[stringCount];
            for (var i = 0; i < stringCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                stringPool[i] = ReadNullTermUtf8String(binaryReader.BaseStream);
            }

            binaryReader.BaseStream.Position = offset;
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var app = SteamApp.FromReader(binaryReader, stringPool, installedAppIds, isSaveProperties,
                num == MagicNumberV2, num == MagicNumberV3);
            if (app == null)
                break;
            onApp(app);
        }

        return universe;
    }

    static string? ShareMetadataString(string? value, Dictionary<string, string> sharedMetadata)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        if (sharedMetadata.TryGetValue(value, out var shared))
            return shared;
        sharedMetadata.Add(value, value);
        return value;
    }

    static string ReadNullTermUtf8String(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(32);

        try
        {
            var position = 0;

            do
            {
                var b = stream.ReadByte();

                if (b <= 0) // null byte or stream ended
                {
                    break;
                }

                if (position >= buffer.Length)
                {
                    var newBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                    Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = newBuffer;
                }

                buffer[position++] = (byte)b;
            } while (true);

            return Encoding.UTF8.GetString(buffer, 0, position);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public uint[] GetInstalledAppIds()
    {
        return GetDownloadingAppList().Where(x => x.IsInstalled).Select(x => x.AppId).ToArray();
    }

    private static string? GetAppCustomImageFilePath(string? SteamDirPath, uint appId, SteamUser user,
        SteamApp.LibCacheType type)
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

    private string? GetAppLibCacheFilePath(string? steamDirectory, uint appId, SteamApp.LibCacheType type)
    {
        var librarycacheDirPath = CombineSteamPath(steamDirectory, "appcache", "librarycache");
        if (librarycacheDirPath == null) return null;
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

        var filePath = Path.Combine(librarycacheDirPath, fileName);

        if (Conn.SteamLanguageString != null && !File.Exists(filePath))
        {
            //默认图没找到情况下尝试查找当前Steam客户端语言图片
            var lang = $"_{Conn.SteamLanguageString}";
            filePath = filePath.Insert(filePath.LastIndexOf('.'), lang);
        }

        return filePath;
    }

    public async Task<Stream?> GetAppImageAsync(SteamApp app, SteamApp.LibCacheType type,
        CancellationToken token = default)
    {
        var steamDirectory = SteamDirPath;
        var mostRecentUser = Conn.SteamUsers.Items.FirstOrDefault(s => s.MostRecent)
                             ?? Conn.SteamUsers.Items.FirstOrDefault(s => s.AutoLogin);
        //var mostRecentUser = Conn.CurrentSteamUser;
        if (mostRecentUser != null)
        {
            var customFilePath = GetAppCustomImageFilePath(steamDirectory, app.AppId, mostRecentUser, type);
            if (customFilePath != null && File.Exists(customFilePath))
            {
                return new FileStream(customFilePath!, FileMode.Open, FileAccess.Read, IOPath.FileShareReadWriteDelete);
            }
        }

        var cacheFilePath = GetAppLibCacheFilePath(steamDirectory, app.AppId, type);
        if (cacheFilePath != null && File.Exists(cacheFilePath))
            return new FileStream(cacheFilePath!, FileMode.Open, FileAccess.Read, IOPath.FileShareReadWriteDelete);

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

        if (string.IsNullOrEmpty(url))
            return default;
        //var value = await ImageSource.GetAsync(url, cache: true, cacheFirst: true, cancellationToken: token);

        var imageHttpClientService = Ioc.Get_Nullable<IImageHttpClientService>();
        if (imageHttpClientService == default)
            return default;

        var imageMemoryStream =
            await imageHttpClientService.GetImageMemoryStreamAsync(url, cache: true, cacheFirst: true,
                cancellationToken: token);
        if (imageMemoryStream == default)
            return default;

        return imageMemoryStream;
    }

    /// <summary>
    /// 保存图片流到 Steam 自定义封面文件夹
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SaveAppImageToSteamFile(object? imageObject, SteamUser user, long appId,
        SteamGridItemType gridType)
    {
        var steamDirPath = this.SteamDirPath;
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

    string[]? GetLibraryPaths(string? SteamDirPath)
    {
        const string dirname_steamapps = "steamapps"; // 文件夹名，linux上区分大小写

        if (string.IsNullOrEmpty(SteamDirPath) || !Directory.Exists(SteamDirPath))
        {
            return null;
        }

        List<string> paths = new() { Path.Combine(SteamDirPath, dirname_steamapps), };

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
        => GetDownloadingAppList(SteamDirPath);

    List<SteamApp> GetDownloadingAppList(string? steamDirectory)
    {
        var appInfos = new List<SteamApp>();
        try
        {
            var libraryPaths = GetLibraryPaths(steamDirectory);
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

        var libraryPaths = GetLibraryPaths(SteamDirPath);
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
                } while (counter++ <= 5);
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