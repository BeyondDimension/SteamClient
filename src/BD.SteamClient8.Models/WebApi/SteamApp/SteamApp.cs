namespace BD.SteamClient8.Models.WebApi.SteamApp;

#pragma warning disable SA1600
public class SteamApp
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    : IComparable<SteamApp>
#endif
{
    [SystemTextJsonIgnore]
    public string NodeAppInfo { get; } = "appinfo";

    [SystemTextJsonIgnore]
    public string NodeAppType { get; } = "type";

    [SystemTextJsonIgnore]
    public string NodeCommon { get; } = "common";

    [SystemTextJsonIgnore]
    public string NodeConfig { get; } = "config";

    [SystemTextJsonIgnore]
    public string NodeExtended { get; } = "extended";

    [SystemTextJsonIgnore]
    public string NodeId { get; } = "gameid";

    [SystemTextJsonIgnore]
    public string NodeName { get; } = "name";

    [SystemTextJsonIgnore]
    public string NodeParentId { get; } = "parent";

    [SystemTextJsonIgnore]
    public string NodePlatforms { get; } = "oslist";

    [SystemTextJsonIgnore]
    public string NodePlatformsLinux { get; } = "linux";

    [SystemTextJsonIgnore]
    public string NodePlatformsMac { get; } = "mac";

    [SystemTextJsonIgnore]
    public string NodePlatformsWindows { get; } = "windows";

    [SystemTextJsonIgnore]
    public string NodeSortAs { get; } = "sortas";

    [SystemTextJsonIgnore]
    public string NodeDeveloper { get; } = "developer";

    [SystemTextJsonIgnore]
    public string NodePublisher { get; } = "publisher";

    [SystemTextJsonIgnore]
    public string NodeLaunch { get; } = "launch";

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
            }
        }
    }

    public int Index { get; set; }

    public int State { get; set; }

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

    public bool IsEdited { get; set; }

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
    /// 是否支持 Steam 云存档
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
    /// 最后运行用户 SteamId64
    /// </summary>
    public long LastOwner { get; set; }

    /// <summary>
    /// 最后更新日期
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// 占用硬盘字节大小
    /// </summary>
    public long SizeOnDisk { get; set; }

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

    public IList<uint> ChildApp { get; set; } = new List<uint>();

    public ObservableCollection<SteamAppLaunchItem>? LaunchItems { get; set; }

    public ObservableCollection<SteamAppSaveFile>? SaveFiles { get; set; }

    [SystemTextJsonIgnore]
    public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
        string.Format(SteamApiUrls.STEAMAPP_LOGO_URL, AppId, Logo);

    [SystemTextJsonIgnore]
    public string LibraryGridUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARY_URL, AppId);

    [SystemTextJsonIgnore]
    public string LibraryHeroUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARYHERO_URL, AppId);

    [SystemTextJsonIgnore]
    public string LibraryHeroBlurUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARYHEROBLUR_URL, AppId);

    [SystemTextJsonIgnore]
    public string LibraryLogoUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARYLOGO_URL, AppId);

    [SystemTextJsonIgnore]
    public string HeaderLogoUrl => string.Format(SteamApiUrls.STEAMAPP_HEADIMAGE_URL, AppId);

    [SystemTextJsonIgnore]
    public string CAPSULELogoUrl => string.Format(SteamApiUrls.STEAMAPP_CAPSULE_URL, AppId);

    [SystemTextJsonIgnore]
    public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
        string.Format(SteamApiUrls.STEAMAPP_LOGO_URL, AppId, Icon);

    [SystemTextJsonIgnore]
    public Process? Process { get; set; }

    public bool IsWatchDownloading { get; set; }

    //public TradeCard? Card { get; set; }

    //public SteamAppInfo? Common { get; set; }

    internal byte[]? _stuffBeforeHash;

    internal uint _changeNumber;

    internal byte[]? _originalData;

    public byte[]? OriginalData { get => _originalData; set => _originalData = value; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    [SystemTextJsonIgnore]
    protected internal SteamAppPropertyTable? _properties;

    [SystemTextJsonIgnore]
    public SteamAppPropertyTable? ChangesData => _properties;

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

#endif

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    public bool IsDownloading => CheckDownloading(State);

    public bool IsInstalled => IsBitSet(State, 2);

    public int CompareTo(SteamApp? other) => string.Compare(Name, other?.Name);

    static bool IsBitSet(int b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }

    /// <summary>
    /// Returns a value indicating whether the game is being downloaded.
    /// </summary>
    public bool CheckDownloading(int appState)
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
}