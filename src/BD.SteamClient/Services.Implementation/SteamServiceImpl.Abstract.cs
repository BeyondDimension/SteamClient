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
                var kv = v["HKCU"]["Software"]["Valve"]["Steam"]["AutoLoginUser"] as KVObjectValue<string>;
                if (kv != null)
                {
                    kv.Value = userName;
                    var rememberPasswordKV = v["HKCU"]["Software"]["Valve"]["Steam"]["RememberPassword"] as KVObjectValue<string>;
                    if (rememberPasswordKV != null)
                    {
                        rememberPasswordKV.Value = "1";
                    }
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