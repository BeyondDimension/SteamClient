namespace BD.SteamClient.Models;

public class SteamApp : ReactiveObject
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    , IComparable<SteamApp>
#endif
{
    const string NodeAppInfo = "appinfo";

    const string NodeAppType = "type";

    const string NodeCommon = "common";

    const string NodeConfig = "config";

    const string NodeExtended = "extended";

    const string NodeId = "gameid";

    const string NodeName = "name";

    const string NodeParentId = "parent";

    const string NodePlatforms = "oslist";

    const string NodePlatformsLinux = "linux";

    const string NodePlatformsMac = "mac";

    const string NodePlatformsWindows = "windows";

    const string NodeSortAs = "sortas";

    const string NodeDeveloper = "developer";

    const string NodePublisher = "publisher";

    const string NodeLaunch = "launch";

    public SteamApp() { }

    public SteamApp(uint appid)
    {
        AppId = appid;
    }

    public uint AppId { get; set; }

    public string? BaseName { get; set; }

    string? _Name;

    public string? Name
    {
        get => _Name;
        set
        {
            if (_Name != value)
            {
                _Name = value;
                SortAs = value;
                this.RaisePropertyChanged();
            }
        }
    }

    string? _SortAs;

    public string? SortAs
    {
        get => _SortAs;
        set
        {
            if (_SortAs != value)
            {
                _SortAs = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public int Index { get; set; }

    public int State { get; set; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// Returns a value indicating whether the game is being downloaded.
    /// </summary>
    public bool IsDownloading => CheckDownloading(State);

    //public bool IsInstalled { get; set; }
    public bool IsInstalled => IsBitSet(State, 2);

#endif

    public string? InstalledDrive => !string.IsNullOrEmpty(InstalledDir) ? Path.GetPathRoot(InstalledDir)?.ToUpper()?.Replace(Path.DirectorySeparatorChar.ToString(), "") : null;

    public string? InstalledDir { get; set; }

    public string? Developer { get; set; }

    public string? Publisher { get; set; }

    public uint? SteamReleaseDate { get; set; }

    public uint? OriginReleaseDate { get; set; }

    public string? OSList { get; set; }

    #region 暂时不用

    //public string? EditName
    //{
    //    get
    //    {
    //        return _properties?.GetPropertyValue<string>(null, NodeAppInfo,
    //            NodeCommon,
    //            NodeName);
    //    }
    //    set
    //    {
    //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo,
    //            NodeCommon,
    //            NodeName);
    //    }
    //}

    //public string? EditSortAs
    //{
    //    get
    //    {
    //        return this._properties?.GetPropertyValue<string>(this.Name, NodeAppInfo, NodeCommon, NodeSortAs);
    //    }
    //    set
    //    {
    //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo, NodeCommon, NodeSortAs);
    //    }
    //}

    //public string? EditDeveloper
    //{
    //    get
    //    {
    //        return _properties?.GetPropertyValue<string>(null, NodeAppInfo,
    //             NodeExtended,
    //             NodeDeveloper);
    //    }
    //    set
    //    {
    //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo,
    //            NodeExtended,
    //            NodeDeveloper);
    //    }
    //}

    //public string? EditPublisher
    //{
    //    get
    //    {
    //        return _properties?.GetPropertyValue<string>(null, NodeAppInfo,
    //             NodeExtended,
    //             NodePublisher);
    //    }
    //    set
    //    {
    //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo,
    //            NodeExtended,
    //            NodePublisher);
    //    }
    //}

    #endregion

    private bool _IsEdited;

    public bool IsEdited
    {
        get => _IsEdited;
        set => this.RaiseAndSetIfChanged(ref _IsEdited, value);
    }

    public string DisplayName => string.IsNullOrEmpty(Name) ? AppId.ToString() : Name;

    //string? _baseDLSSVersion;

    //public string? BaseDLSSVersion
    //{
    //    get => _baseDLSSVersion;
    //    set
    //    {
    //        if (_baseDLSSVersion != value)
    //        {
    //            _baseDLSSVersion = value;
    //            this.RaisePropertyChanged();
    //        }
    //    }
    //}

    //string? _currentDLSSVersion;

    //public string? CurrentDLSSVersion
    //{
    //    get => _currentDLSSVersion;
    //    set
    //    {
    //        if (_currentDLSSVersion != value)
    //        {
    //            _currentDLSSVersion = value;
    //            this.RaisePropertyChanged();
    //        }
    //    }
    //}

    //public bool HasDLSS { get; set; }

    public string? Logo { get; set; }

    public string? Icon { get; set; }

    public SteamAppType Type { get; set; }

    public uint ParentId { get; set; }

    /// <summary>
    /// 是否支持Steam云存档
    /// </summary>
    public bool IsCloudArchive => CloudQuota > 0;

    /// <summary>
    /// 云存档字节大小
    /// </summary>
    public long CloudQuota { get; set; }

    /// <summary>
    /// 云存档文件数量上限
    /// </summary>
    public int CloudMaxnumFiles { get; set; }

    /// <summary>
    /// 最后运行用户SteamId64
    /// </summary>
    public long LastOwner { get; set; }

    /// <summary>
    /// 最后更新日期
    /// </summary>
    public DateTime LastUpdated { get; set; }

    private long _SizeOnDi;

    /// <summary>
    /// 占用硬盘字节大小
    /// </summary>
    public long SizeOnDisk
    {
        get => _SizeOnDi;
        set => this.RaiseAndSetIfChanged(ref _SizeOnDi, value);
    }

    /// <summary>
    /// 需要下载字节数
    /// </summary>
    public long BytesToDownload { get; set; }

    /// <summary>
    /// 已下载字节数 
    /// </summary>
    public long BytesDownloaded { get; set; }

    /// <summary>
    /// 需要安装字节数
    /// </summary>
    public long BytesToStage { get; set; }

    /// <summary>
    /// 已安装字节数 
    /// </summary>
    public long BytesStaged { get; set; }

    IList<uint>? _ChildApp;

    // Most library entries have no children. Preserve the mutable collection contract
    // without allocating a separate empty List for every app during inventory reads.
    public IList<uint> ChildApp
    {
        get => _ChildApp ??= new List<uint>();
        set => _ChildApp = value;
    }

    private ObservableCollection<SteamAppLaunchItem>? _LaunchItems;

    public ObservableCollection<SteamAppLaunchItem>? LaunchItems
    {
        get => _LaunchItems;
        set => this.RaiseAndSetIfChanged(ref _LaunchItems, value);
    }

    private ObservableCollection<SteamAppSaveFile>? _SaveFiles;

    public ObservableCollection<SteamAppSaveFile>? SaveFiles
    {
        get => _SaveFiles;
        set => this.RaiseAndSetIfChanged(ref _SaveFiles, value);
    }

    public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
        string.Format(STEAMAPP_LOGO_URL, AppId, Logo);

    public string LibraryGridUrl => string.Format(STEAMAPP_LIBRARY_URL, AppId);

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public Task<Stream?> LibraryGridStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Library_Grid);

#endif

    public string LibraryHeroUrl => string.Format(STEAMAPP_LIBRARYHERO_URL, AppId);

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public Task<Stream?> LibraryHeroStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Library_Hero);

#endif

    public string LibraryHeroBlurUrl => string.Format(STEAMAPP_LIBRARYHEROBLUR_URL, AppId);

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public Task<Stream?> LibraryHeroBlurStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Library_Hero_Blur);

#endif

    public string LibraryLogoUrl => string.Format(STEAMAPP_LIBRARYLOGO_URL, AppId);

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public Task<Stream?> LibraryLogoStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Logo);

#endif

    public string HeaderLogoUrl => string.Format(STEAMAPP_HEADIMAGE_URL, AppId);

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public Task<Stream?> HeaderLogoStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Header);

#endif

    public string CAPSULELogoUrl => string.Format(STEAMAPP_CAPSULE_URL, AppId);

    public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
        string.Format(STEAMAPP_LOGO_URL, AppId, Icon);

    private Process? _Process;

    private object? _ProcessLock;

    private object ProcessLock => LazyInitializer.EnsureInitialized(ref _ProcessLock);

    /// <summary>The model-owned idle child. Achievement and cloud-manager windows are independent.</summary>
    public Process? Process
    {
        get => Volatile.Read(ref _Process);
        set
        {
            lock (ProcessLock)
            {
                if (ReferenceEquals(_Process, value))
                    return;
                _Process = value;
            }
            this.RaisePropertyChanged();
        }
    }

    private bool _IsWatchDownloading;

    public bool IsWatchDownloading
    {
        get => _IsWatchDownloading;
        set => this.RaiseAndSetIfChanged(ref _IsWatchDownloading, value);
    }

    //public TradeCard? Card { get; set; }

    //public SteamAppInfo? Common { get; set; }

    public string GetIdAndName()
    {
        return $"{AppId} | {DisplayName}";
    }

    public int CompareTo(SteamApp? other) => string.Compare(Name, other?.Name);

    byte[]? _stuffBeforeHash;

    uint _changeNumber;

    byte[]? _originalData;

    public byte[]? OriginalData { get => _originalData; set => _originalData = value; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    SteamAppPropertyTable? _properties;

    public SteamAppPropertyTable? ChangesData => _properties;

    SteamAppEdit? originalEditValues;

    /// <summary>Returns editable values before Watt Toolkit's overrides.</summary>
    public SteamAppEdit GetOriginalEditValues() => SteamAppEdit.Capture(this).WithOverrides(originalEditValues);

    /// <summary>Replaces the current overrides, restoring fields no longer edited.</summary>
    public void ApplyAppInfoEdit(SteamAppEdit edit)
    {
        originalEditValues?.ApplyTo(this);
        originalEditValues = edit.IsEmpty ? null : edit.OriginalFields(this);
        edit.ApplyTo(this);
        IsEdited = !edit.IsEmpty;
    }

    /// <summary>
    /// Ensures one idle child is running, or opens an independent achievement/cloud-manager window.
    /// The returned idle handle is borrowed; a GUI handle belongs to the caller.
    /// </summary>
    public Process? StartSteamAppProcess(SteamAppRunType runType = SteamAppRunType.Idle)
    {
        if (runType is SteamAppRunType.UnlockAchievement or SteamAppRunType.CloudManager)
            return SteamAppProcessLauncher.Start(AppId, runType);

        return SetIdleProcessRunning(true);
    }

    /// <summary>Stops and releases the owned idle child, if any, without touching GUI windows.</summary>
    public void StopSteamAppProcess() => SetIdleProcessRunning(false);

    /// <summary>Atomically toggles the owned idle child.</summary>
    public void RunOrStopSteamAppProcess() => SetIdleProcessRunning(null);

    Process? SetIdleProcessRunning(bool? shouldRun)
    {
        Process? oldProcess = null;
        var changed = false;
        try
        {
            lock (ProcessLock)
            {
                var process = _Process;
                var isRunning = process != null && !process.HasExited;
                if (shouldRun ?? !isRunning)
                {
                    if (isRunning)
                        return process;

                    oldProcess = process;
                    _Process = null;
                    changed = process != null;
                    var started = SteamAppProcessLauncher.Start(AppId, SteamAppRunType.Idle);
                    _Process = started;
                    changed |= started != null;
                    return started;
                }

                if (process == null)
                    return null;

                if (isRunning)
                {
                    try
                    {
                        process.KillEntireProcessTree();
                    }
                    catch (InvalidOperationException) when (process.HasExited)
                    {
                        // The child may have exited between the liveness check and Kill.
                    }
                    catch (Win32Exception) when (process.HasExited)
                    {
                        // Some platforms report the same exit race as a native error.
                    }
                    // Kill is asynchronous. Do not permit another start before this child exits.
                    process.WaitForExit();
                }

                _Process = null;
                oldProcess = process;
                changed = true;
                return null;
            }
        }
        finally
        {
            // Notifications and disposal callbacks must not re-enter while ownership is changing.
            oldProcess?.Dispose();
            if (changed)
                this.RaisePropertyChanged(nameof(Process));
        }
    }

    #region Replace DLSS dll files methods

    //        public void DetectDLSS()
    //        {
    //            BaseDLSSVersion = string.Empty;
    //            CurrentDLSSVersion = "N/A";
    //            var dlssDlls = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll", SearchOption.AllDirectories);
    //            if (dlssDlls.Length > 0)
    //            {
    //                HasDLSS = true;

    //                // TODO: Handle a single folder with various versions of DLSS detected.
    //                // Currently we are just using the first.

    //                foreach (var dlssDll in dlssDlls)
    //                {
    //                    var dllVersionInfo = FileVersionInfo.GetVersionInfo(dlssDll);
    //                    CurrentDLSSVersion = dllVersionInfo.FileVersion?.Replace(",", ".") ?? string.Empty;
    //                    break;
    //                }

    //                dlssDlls = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll.dlsss", SearchOption.AllDirectories);
    //                if (dlssDlls.Length > 0)
    //                {
    //                    foreach (var dlssDll in dlssDlls)
    //                    {
    //                        var dllVersionInfo = FileVersionInfo.GetVersionInfo(dlssDll);
    //                        BaseDLSSVersion = dllVersionInfo.FileVersion?.Replace(",", ".") ?? string.Empty;
    //                        break;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                HasDLSS = false;
    //            }
    //        }

    //        internal bool ResetDll()
    //        {
    //            var foundDllBackups = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll.dlsss", SearchOption.AllDirectories);
    //            if (foundDllBackups.Length == 0)
    //            {
    //                return false;
    //            }

    //            var versionInfo = FileVersionInfo.GetVersionInfo(foundDllBackups.First());
    //            var resetToVersion = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";

    //            foreach (var dll in foundDllBackups)
    //            {
    //                try
    //                {
    //                    var dllPath = Path.GetDirectoryName(dll);
    //                    var targetDllPath = Path.Combine(dllPath!, "nvngx_dlss.dll");
    //#if NETSTANDARD
    //                    File.Move(dll, targetDllPath);
    //#else
    //                    File.Move(dll, targetDllPath, true);
    //#endif
    //                }
    //                catch (Exception err)
    //                {
    //                    Debug.WriteLine($"ResetDll Error: {err.Message}");
    //                    return false;
    //                }
    //            }

    //            CurrentDLSSVersion = resetToVersion;
    //            BaseDLSSVersion = string.Empty;

    //            return true;
    //        }

    //        internal bool UpdateDll(LocalDlssDll localDll)
    //        {
    //            if (localDll == null)
    //            {
    //                return false;
    //            }

    //            var foundDlls = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll", SearchOption.AllDirectories);
    //            if (foundDlls.Length == 0)
    //            {
    //                return false;
    //            }

    //            var versionInfo = FileVersionInfo.GetVersionInfo(localDll.Filename);
    //            var targetDllVersion = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";

    //            var baseDllVersion = string.Empty;

    //            // Backup old dlls.
    //            foreach (var dll in foundDlls)
    //            {
    //                var dllPath = Path.GetDirectoryName(dll);
    //                var targetDllPath = Path.Combine(dllPath!, "nvngx_dlss.dll.dlsss");
    //                if (File.Exists(targetDllPath) == false)
    //                {
    //                    try
    //                    {
    //                        var defaultVersionInfo = FileVersionInfo.GetVersionInfo(dll);
    //                        baseDllVersion = $"{defaultVersionInfo.FileMajorPart}.{defaultVersionInfo.FileMinorPart}.{defaultVersionInfo.FileBuildPart}.{defaultVersionInfo.FilePrivatePart}";

    //                        File.Copy(dll, targetDllPath, true);
    //                    }
    //                    catch (Exception err)
    //                    {
    //                        Debug.WriteLine($"UpdateDll Error: {err.Message}");
    //                        return false;
    //                    }
    //                }
    //            }

    //            foreach (var dll in foundDlls)
    //            {
    //                try
    //                {
    //                    File.Copy(localDll.Filename, dll, true);
    //                }
    //                catch (Exception err)
    //                {
    //                    Debug.WriteLine($"UpdateDll Error: {err.Message}");
    //                    return false;
    //                }
    //            }

    //            CurrentDLSSVersion = targetDllVersion;
    //            if (!string.IsNullOrEmpty(baseDllVersion))
    //            {
    //                BaseDLSSVersion = baseDllVersion;
    //            }
    //            return true;
    //        }

    #endregion

    public SteamApp ExtractReaderProperty(SteamAppPropertyTable properties, IReadOnlySet<uint>? installedAppIds = null)
    {
        if (properties != null)
        {
            //var installpath = properties.GetPropertyValue<string>(null, NodeAppInfo, NodeConfig, "installdir");

            //if (!string.IsNullOrEmpty(installpath))
            //{
            //    app.InstalledDir = Path.Combine(ISteamService.Instance.SteamDirPath, ISteamService.dirname_steamapps, NodeCommon, installpath);
            //}

            var appInfo = properties.GetPropertyValue<SteamAppPropertyTable?>(NodeAppInfo);
            var common = appInfo?.GetPropertyValue<SteamAppPropertyTable?>(NodeCommon);
            var extended = appInfo?.GetPropertyValue<SteamAppPropertyTable?>(NodeExtended);
            var ufs = appInfo?.GetPropertyValue<SteamAppPropertyTable?>("ufs");

            Name = common?.GetPropertyValue(NodeName, string.Empty) ?? string.Empty;
            SortAs = common?.GetPropertyValue(NodeSortAs, string.Empty) ?? string.Empty;
            if (string.IsNullOrEmpty(SortAs) || string.Equals(SortAs, Name, StringComparison.Ordinal))
            {
                SortAs = Name;
            }
            ParentId = common?.GetPropertyValue<uint>(NodeParentId) ?? 0;
            Developer = extended?.GetPropertyValue(NodeDeveloper, string.Empty) ?? string.Empty;
            Publisher = extended?.GetPropertyValue(NodePublisher, string.Empty) ?? string.Empty;
            //SteamReleaseDate = properties.GetPropertyValue<uint>(0, NodeAppInfo, NodeCommon, "steam_release_date");
            //OriginReleaseDate = properties.GetPropertyValue<uint>(0, NodeAppInfo, NodeCommon, "original_release_date");

            var type = common?.GetPropertyValue(NodeAppType, string.Empty) ?? string.Empty;
            if (Enum.TryParse(type, true, out SteamAppType apptype))
            {
                Type = apptype;
            }
            else
            {
                Type = SteamAppType.Unknown;
                Debug.WriteLineIf(!string.IsNullOrEmpty(type), string.Format("AppInfo: New AppType '{0}'", type));
            }

            OSList = common?.GetPropertyValue(NodePlatforms, string.Empty) ?? string.Empty;

            if (installedAppIds != null)
            {
                if (installedAppIds.Contains(AppId) &&
                    (Type == SteamAppType.Application ||
                    Type == SteamAppType.Game ||
                    Type == SteamAppType.Tool ||
                    Type == SteamAppType.Demo))
                {
                    // This is an installed app.
                    State = 4;
                }
            }

            if (IsInstalled)
            {
                var launchTable = appInfo?.GetPropertyValue<SteamAppPropertyTable?>(NodeConfig)
                    ?.GetPropertyValue<SteamAppPropertyTable?>(NodeLaunch);

                if (launchTable != null)
                {
                    var launchItems = from table in from prop in (from prop in launchTable.Properties
                                                                  where prop.PropertyType == SteamAppPropertyType.Table
                                                                  select prop).OrderBy((SteamAppProperty prop) => prop.Name, StringComparer.OrdinalIgnoreCase)
                                                    select prop.GetValue<SteamAppPropertyTable>()
                                      select new SteamAppLaunchItem
                                      {
                                          Label = table.GetPropertyValue<string?>("description"),
                                          Executable = table.GetPropertyValue<string?>("executable"),
                                          Arguments = table.GetPropertyValue<string?>("arguments"),
                                          WorkingDir = table.GetPropertyValue<string?>("workingdir"),
                                          Platform = table.TryGetPropertyValue<SteamAppPropertyTable>(NodeConfig, out var propertyTable) ?
                                          propertyTable.TryGetPropertyValue<string>(NodePlatforms, out var os) ? os : null : null,
                                      };

                    LaunchItems = new ObservableCollection<SteamAppLaunchItem>(launchItems);
                }
            }

            CloudQuota = ufs?.GetPropertyValue<int>("quota") ?? 0;
            CloudMaxnumFiles = ufs?.GetPropertyValue<int>("maxnumfiles") ?? 0;

            var savefilesTable = ufs?.GetPropertyValue<SteamAppPropertyTable?>("savefiles");

            if (savefilesTable != null)
            {
                var savefiles = from table in from prop in (from prop in savefilesTable.Properties
                                                            where prop.PropertyType == SteamAppPropertyType.Table
                                                            select prop).OrderBy((SteamAppProperty prop) => prop.Name, StringComparer.OrdinalIgnoreCase)
                                              select prop.GetValue<SteamAppPropertyTable>()
                                select new SteamAppSaveFile
                                (
                                    AppId,
                                    table.GetPropertyValue<string?>("root"),
                                    table.GetPropertyValue<string?>("path"),
                                    table.GetPropertyValue<string?>("pattern")
                                )
                                {
                                    Recursive = table.GetPropertyValue(false, "recursive"),
                                };

                SaveFiles = new ObservableCollection<SteamAppSaveFile>(savefiles);
            }

            originalEditValues = SteamAppEditProperties.ReadOriginals(appInfo);
            BaseName = originalEditValues?.Name ?? Name;
            IsEdited = originalEditValues != null;
        }
        return this;
    }

    internal static SteamApp FromShardedAppInfo(uint id, SteamAppPropertyTable properties,
        IReadOnlySet<uint> installedAppIds, bool isSaveProperties) => new SteamApp
        {
            AppId = id,
            // Keep editable properties for backups, not raw bytes using a shard-local string table.
            _properties = isSaveProperties ? properties : null,
        }.ExtractReaderProperty(properties, installedAppIds);

    public static SteamApp? FromReader(BinaryReader reader, string[]? stringPool = null, IReadOnlySet<uint>? installedAppIds = null, bool isSaveProperties = false, bool isMagicNumberV2 = false, bool isMagicNumberV3 = false)
    {
        uint id = reader.ReadUInt32();
        if (id == 0)
        {
            return null;
        }
        SteamApp app = new()
        {
            AppId = id,
        };
        try
        {
            int count = reader.ReadInt32();
            long nextEntry = reader.BaseStream.Position + count;
            // Display loads need extracted fields only; keep the raw entry solely for edit/save.
            byte[]? originalData = isSaveProperties ? reader.ReadBytes(count) : null;
            using var dataStream = originalData == null ? null : new MemoryStream(originalData);
            using var dataReader = dataStream == null ? null : new BinaryReader(dataStream, Encoding.UTF8, true);
            var propertyReader = dataReader ?? reader;
            try
            {
                if (isSaveProperties)
                    app._stuffBeforeHash = propertyReader.ReadBytes(16);
                else
                    propertyReader.BaseStream.Seek(16, SeekOrigin.Current);
                propertyReader.BaseStream.Seek(20, SeekOrigin.Current);
                app._changeNumber = propertyReader.ReadUInt32();

                if (isMagicNumberV2 || isMagicNumberV3)
                    propertyReader.BaseStream.Seek(20, SeekOrigin.Current);

                var properties = propertyReader.ReadPropertyTable(stringPool);
                if (isSaveProperties)
                {
                    app._properties = properties;
                    app._originalData = originalData;
                }
                app.ExtractReaderProperty(properties, installedAppIds);
            }
            finally
            {
                reader.BaseStream.Position = nextEntry;
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(SteamApp), ex, string.Format("Failed to load entry with appId {0}", app.AppId));
        }
        return app;
    }

    public void Write(BinaryWriter writer, List<string> stringTable, Dictionary<string, uint> stringLookup)
    {
        if (_properties == null)
            throw new ArgumentNullException($"SteamApp Write Failed. {nameof(_properties)} is null.");
        var propertyTable = _properties;
        string s = propertyTable.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        using SHA1 sha = SHA1.Create();
        byte[] array = sha.ComputeHash(bytes);
        writer.Write(this.AppId);
        byte[] array2 = propertyTable.GetPropertyTableByte_0x07564429(stringTable, stringLookup);
        byte[] array3 = sha.ComputeHash(array2);

        using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(), Encoding.UTF8, true);
        binaryWriter.Write(_stuffBeforeHash.ThrowIsNull());
        binaryWriter.Write(array);
        binaryWriter.Write(_changeNumber);
        binaryWriter.Write(array3);
        binaryWriter.Write(array2);
        MemoryStream memoryStream = (MemoryStream)binaryWriter.BaseStream;
        writer.Write((int)memoryStream.Length);
        writer.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }

    static bool IsBitSet(int b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }

    /// <summary>
    /// Returns a value indicating whether the game is being downloaded.
    /// </summary>
    public static bool CheckDownloading(int appState)
    {
        return (IsBitSet(appState, 1) || IsBitSet(appState, 10)) && !IsBitSet(appState, 9);

        /* Counting from zero and starting from the right
         * Bit 1 indicates if a download is running
         * Bit 3 indicates if a preloaded game download 
         * Bit 2 indicates if a game is installed
         * Bit 9 indicates if the download has been stopped by the user. The download will not happen, so don't wait for it.
         * Bit 10 (or maybe Bit 5) indicates if a DLC is downloaded for a game
         * 
         * All known stateFlags while a download is running so far:
         * 00000000110
         * 10000000010
         * 10000010010
         * 10000100110
         * 10000000110
         * 10000010100 Bit 1 not set, but Bit 5 and Bit 10. Happens if downloading a DLC for an already downloaded game.
         *             Because for a very short time after starting the download for this DLC the stateFlags becomes 20 = 00000010100
         *             I think Bit 5 indicates if "something" is happening with a DLC and Bit 10 indicates if it is downloading.
         */
    }

#endif

    public enum LibCacheType : byte
    {
        /// <summary>
        /// <see cref="HeaderLogoUrl"/>
        /// </summary>
        Header,

        /// <summary>
        /// <see cref="IconUrl"/>
        /// </summary>
        Icon,

        /// <summary>
        /// <see cref="LibraryGridUrl"/>
        /// </summary>
        Library_Grid,

        /// <summary>
        /// <see cref="LibraryHeroUrl"/>
        /// </summary>
        Library_Hero,

        /// <summary>
        /// <see cref="LibraryHeroBlurUrl"/>
        /// </summary>
        Library_Hero_Blur,

        /// <summary>
        /// <see cref="LibraryLogoUrl"/>
        /// </summary>
        Logo,
    }
}