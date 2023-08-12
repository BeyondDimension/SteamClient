#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using ValveKeyValue;
#endif

namespace BD.SteamClient.Services.Implementation;

abstract partial class SteamServiceImpl
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public abstract ISteamConnectService Conn { get; }

    public virtual void StartSteamWithParameter() => StartSteam(StratSteamDefaultParameter);

    /// <summary>
    /// 以正常权限启动进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    protected virtual Process? StartAsInvoker(string fileName, string? arguments = null)
         => Process2.Start(fileName, arguments);

    /// <summary>
    /// SteamSettings.SteamStratParameter.Value
    /// </summary>
    protected abstract string? StratSteamDefaultParameter { get; }

    /// <summary>
    /// SteamSettings.IsRunSteamAdministrator.Value
    /// </summary>
    protected abstract bool IsRunSteamAdministrator { get; }

    /// <summary>
    /// GameLibrarySettings.HideGameList.Value
    /// </summary>
    protected abstract Dictionary<uint, string?>? HideGameList { get; }

    protected abstract string? GetString(string name);

#if WINDOWS
    /// <summary>
    /// 在 Windows 平台中修正从注册表读取的路径大小写与路径分隔符
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string? GetFullPath(string s)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            try
            {
                var path_splits = s.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries); // 按分隔符切割数组
                StringBuilder builder = new();
                bool pathExists = true;
                for (int i = 0; i < path_splits.Length; i++) // 循环路径数组
                {
                    var item = path_splits[i];
                    var is_first = i == 0;
                    var is_last = !is_first && (i == path_splits.Length - 1);
                    if (is_first) // 修正盘符
                    {
                        if (item.HasLowerLetter())
                        {
                            item = item.ToUpperInvariant(); // 盘符大写
                        }
                    }
                    else if (pathExists)
                    {
                        try
                        {
                            var path = builder.ToString();
                            var dirInfo = new DirectoryInfo(path);
                            pathExists = dirInfo.Exists;
                            if (pathExists)
                            {
                                IEnumerable<FileSystemInfo> infos = is_last ? dirInfo.EnumerateFileSystemInfos() : dirInfo.EnumerateDirectories();
                                foreach (var info in infos)
                                {
                                    if (string.Equals(info.Name,
                                        item, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // 修正文件名或文件夹名
                                        item = info.Name;
                                        break;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            pathExists = false;
                        }
                    }

                    builder.Append(item);
                    if (!is_last) builder.Append(Path.DirectorySeparatorChar);
                }
                return builder.ToString();
            }
            catch
            {
                return Path.GetFullPath(s);
            }
        }
        return null;
    }

    const string SteamRegistryPath = @"SOFTWARE\Valve\Steam";
#endif

    /// <inheritdoc cref="ISteamService.SteamDirPath"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetSteamDirPath()
    {
#if WINDOWS
        return GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamPath"));
#elif MACCATALYST || MACOS
        return $"/Users/{Environment.UserName}/Library/Application Support/Steam";
#elif LINUX
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.steam/steam";
#else
        return null;
#endif
    }

    /// <summary>
    /// 获取 Steam 动态链接库 (DLL) 文件夹目录
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetSteamDynamicLinkLibraryPath()
    {
#if MACCATALYST || MACOS
        return $"/Users/{Environment.UserName}/Library/Application Support/Steam/Steam.AppBundle/Steam/Contents/MacOS";
#else
        return GetSteamDirPath();
#endif
    }

    /// <inheritdoc cref="ISteamService.SteamProgramPath"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetSteamProgramPath()
    {
#if WINDOWS
        return GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamExe"));
#elif MACCATALYST || MACOS
        return "/Applications/Steam.app/Contents/MacOS/steam_osx";
#elif LINUX
        return "/usr/bin/steam";
#else
        return null;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetRegistryVdfPath()
    {
#if MACCATALYST || MACOS
        return $"/Users/{Environment.UserName}/Library/Application Support/Steam/registry.vdf";
#elif LINUX
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.steam/registry.vdf";
#else
        return null;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetLastSteamLoginUserName()
    {
#if WINDOWS
        return Registry.CurrentUser.Read(SteamRegistryPath, "AutoLoginUser");
#else
        return "";
#endif
    }

    /// <inheritdoc cref="ISteamService.SetCurrentUser(string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void SetSteamCurrentUser(string userName)
    {
        // override BD.WTTS.Services.Implementation.SteamServiceImpl2.SetSteamCurrentUser
#if WINDOWS
        Registry.CurrentUser.AddOrUpdate(SteamRegistryPath, "AutoLoginUser", userName, RegistryValueKind.String);
        Registry.CurrentUser.AddOrUpdate(SteamRegistryPath, "RememberPassword", 1, RegistryValueKind.DWord);
#elif LINUX || MACOS || MACCATALYST
        try
        {
            var registryVdfPath = GetRegistryVdfPath();
            if (!string.IsNullOrWhiteSpace(registryVdfPath) && File.Exists(registryVdfPath))
            {
                var v = VdfHelper.Read(registryVdfPath);

                var steamItem = v["HKCU"]["Software"]["Valve"]["Steam"] as KVCollectionValue;
                if (steamItem != null)
                {

                    var kv = steamItem["AutoLoginUser"] as KVObjectValue<string>;
                    if (kv == null)
                    {
                        steamItem.Add(new KVObject("AutoLoginUser", new KVObjectValue<string>(userName, KVValueType.String)));
                    }
                    else
                    {
                        kv.Value = userName;
                    }
                    var rememberPasswordKV = steamItem["RememberPassword"] as KVObjectValue<string>;
                    if (rememberPasswordKV == null)
                    {
                        if (steamItem != null)
                            steamItem.Add(new KVObject("RememberPassword", new KVObjectValue<string>("1", KVValueType.String)));
                    }
                    else
                    {
                        rememberPasswordKV.Value = "1";
                    }
                    VdfHelper.Write(registryVdfPath, v);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "SetSteamCurrentUser fail(0).");
        }
#endif
    }

#endif
}