#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using ValveKeyValue;
#endif

namespace BD.SteamClient.Services.Implementation;

abstract partial class SteamServiceImpl
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public abstract ISteamConnectService Conn { get; }

    public virtual void StartSteamWithParameter()
        => StartSteam(SteamSettings_StratParameter);

    /// <summary>
    /// 以正常权限启动进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    protected virtual Process? StartAsInvoker(string fileName, string? arguments = null)
         => Process2.Start(fileName, arguments);

    /// <summary>
    /// 保存 AppInfos 出现错误
    /// <para>AppResources.SaveEditedAppInfo_SaveFailed</para>
    /// </summary>
    protected abstract string AppResources_SaveEditedAppInfo_SaveFailed { get; }

    /// <summary>
    /// SteamSettings.SteamStratParameter.Value
    /// </summary>
    protected abstract string? SteamSettings_StratParameter { get; }

    /// <summary>
    /// SteamSettings.IsRunSteamAdministrator.Value
    /// </summary>
    protected abstract bool SteamSettings_IsRunSteamAdministrator { get; }

    /// <summary>
    /// GameLibrarySettings.HideGameList.Value
    /// </summary>
    protected abstract Dictionary<uint, string?> GameLibrarySettings_HideGameList { get; }

#if WINDOWS
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string? GetFullPath(string s)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            return Path.GetFullPath(s);
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
#if WINDOWS
        //if (DesktopBridge.IsRunningAsUwp)
        //{
        //    var reg = $"Windows Registry Editor Version 5.00{Environment.NewLine}[HKEY_CURRENT_USER\\Software\\Valve\\Steam]{Environment.NewLine}\"AutoLoginUser\"=\"{userName}\"";
        //    var path = IOPath.GetCacheFilePath(CacheTempDirName, "SwitchSteamUser", FileEx.Reg);
        //    File.WriteAllText(path, reg, Encoding.UTF8);
        //    var p = StartProcessRegedit($"/s \"{path}\"");
        //    IOPath.TryDeleteInDelay(p, path);
        //}
        //else
        //{
        Registry.CurrentUser.AddOrUpdate(SteamRegistryPath, "AutoLoginUser", userName, RegistryValueKind.String);
        //}
#elif LINUX || MACOS || MACCATALYST
        try
        {
            var registryVdfPath = GetRegistryVdfPath();
            if (!string.IsNullOrWhiteSpace(registryVdfPath) && File.Exists(registryVdfPath))
            {
                var v = VdfHelper.Read(registryVdfPath);
                if (v["HKCU"]["Software"]["Valve"]["Steam"]["AutoLoginUser"] != null)
                {
                    //v["HKCU"]["Software"]["Valve"]["Steam"]["AutoLoginUser"] = userName;
                    VdfHelper.Write(registryVdfPath, v);
                }
                else
                {
                    Log.Error(TAG, "SetSteamCurrentUser fail(1), AutoLoginUser is null.");
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