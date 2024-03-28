#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 游戏
/// </summary>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class SteamApp
#if !(IOS || ANDROID)
    : IComparable<SteamApp>
#endif
{
#pragma warning disable SA1600 // Elements should be documented
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

    /// <summary>
    /// 无参构造
    /// </summary>
    [SystemTextJsonConstructor]
    public SteamApp() { }

    /// <summary>
    /// 通过 app_id 构造 <see cref="SteamApp"/> 实例
    /// </summary>
    /// <param name="app_id"></param>
    public SteamApp(uint app_id)
    {
        AppId = app_id;
    }

    /// <inheritdoc cref="DebuggerDisplayAttribute"/>
    public string DebuggerDisplay() => $"{AppId}, {Name}";

    /// <summary>
    /// AppId 唯一标识
    /// </summary>
    [SystemTextJsonProperty("appid")]
    public uint AppId { get; set; }

    /// <summary>
    /// 基础名称
    /// </summary>
    public string? BaseName { get; set; }

    string? _Name;

    /// <summary>
    /// 名称
    /// </summary>
    [SystemTextJsonProperty("name")]
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

    /// <summary>
    /// 排序字段
    /// </summary>
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

    /// <summary>
    /// 索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int State { get; set; }

    /// <summary>
    /// 安装的驱动器地址
    /// </summary>
    public string? InstalledDrive => !string.IsNullOrEmpty(InstalledDir) ? Path.GetPathRoot(InstalledDir)?.ToUpper()?.Replace(Path.DirectorySeparatorChar.ToString(), "") : null;

    /// <summary>
    /// 安装的文件夹
    /// </summary>
    public string? InstalledDir { get; set; }

    /// <summary>
    /// 开发者
    /// </summary>
    public string? Developer { get; set; }

    /// <summary>
    /// 发布者
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Steam 发布日期
    /// </summary>
    public uint? SteamReleaseDate { get; set; }

    /// <summary>
    /// 原始发布日期
    /// </summary>
    public uint? OriginReleaseDate { get; set; }

    /// <summary>
    /// 支持系统列表
    /// </summary>
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

    /// <summary>
    /// 是否被编辑
    /// </summary>
    public bool IsEdited { get; set; }

    /// <summary>
    /// 展示名称
    /// </summary>
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

    /// <summary>
    /// Logo 地址
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// 图标 地址
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 游戏类型
    /// </summary>
    public SteamAppType Type { get; set; }

    /// <summary>
    /// 父级游戏 AppId
    /// </summary>
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

    /// <summary>
    /// 子项
    /// </summary>
    public IList<uint> ChildApp { get; set; } = new List<uint>();

    /// <summary>
    /// 正在运行的 App 列表
    /// </summary>
    public ObservableCollection<SteamAppLaunchItem>? LaunchItems { get; set; }

    /// <summary>
    /// <see cref="SteamAppSaveFile"/> 列表集合
    /// </summary>
    public ObservableCollection<SteamAppSaveFile>? SaveFiles { get; set; }

    /// <summary>
    /// Logo 图片路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
        string.Format(SteamApiUrls.STEAMAPP_LOGO_URL, AppId, Logo);

    /// <summary>
    /// LibraryGrid 图片路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string LibraryGridUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARY_URL, AppId);

    /// <summary>
    /// LibraryHero 图片路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string LibraryHeroUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARYHERO_URL, AppId);

    /// <summary>
    /// LibraryHeroBlur 图片路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string LibraryHeroBlurUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARYHEROBLUR_URL, AppId);

    /// <summary>
    /// LibraryLogo 图片路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string LibraryLogoUrl => string.Format(SteamApiUrls.STEAMAPP_LIBRARYLOGO_URL, AppId);

    /// <summary>
    /// HeaderLogo 图片路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string HeaderLogoUrl => string.Format(SteamApiUrls.STEAMAPP_HEADIMAGE_URL, AppId);

    /// <summary>
    /// CAPSULELogo 图片路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string CAPSULELogoUrl => string.Format(SteamApiUrls.STEAMAPP_CAPSULE_URL, AppId);

    /// <summary>
    /// 图标路径
    /// </summary>
    [SystemTextJsonIgnore]
    public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
        string.Format(SteamApiUrls.STEAMAPP_LOGO_URL, AppId, Icon);

    private Process? _process;

    /// <summary>
    /// App 启动进程
    /// </summary>
    [SystemTextJsonIgnore]
    public Process? Process { get => _process; set { _process = value; HasProcess = value is not null; } }

    /// <summary>
    /// App 是否启动进程（用于序列化传输获取状态）
    /// </summary>
    public bool HasProcess { get; set; }

    /// <summary>
    /// 是否正在观测下载
    /// </summary>
    public bool IsWatchDownloading { get; set; }

    //public TradeCard? Card { get; set; }

    //public SteamAppInfo? Common { get; set; }

    /// <summary>
    /// 内容填充前数据块
    /// </summary>
    internal byte[]? _stuffBeforeHash;

    /// <summary>
    /// 修改数量
    /// </summary>
    internal uint _changeNumber;

    /// <summary>
    /// 原始数据
    /// </summary>
    internal byte[]? _originalData;

    /// <summary>
    /// 原始数据
    /// </summary>
    public byte[]? OriginalData { get => _originalData; set => _originalData = value; }

#if !(IOS || ANDROID)

    /// <summary>
    /// 内部属性集合
    /// </summary>
    [SystemTextJsonIgnore]
    protected internal SteamAppPropertyTable? _properties;

    /// <summary>
    /// 修改的属性集合
    /// </summary>
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

#if !(IOS || ANDROID)

    /// <summary>
    /// 是否正在下载
    /// </summary>
    [SystemTextJsonIgnore]
    public bool IsDownloading => CheckDownloading(State);

    /// <summary>
    /// 是否已下载
    /// </summary>
    [SystemTextJsonIgnore]
    public bool IsInstalled => IsBitSet(State, 2);

    /// <summary>
    /// <see cref="SteamApp"/> Compare
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
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