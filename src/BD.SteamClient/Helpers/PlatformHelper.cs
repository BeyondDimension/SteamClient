namespace BD.SteamClient.Helpers;

static class PlatformHelper
{
    const string TAG = nameof(PlatformHelper);

    /// <inheritdoc cref="ISteamService.SteamDirPath"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetSteamDirPath()
    {
        return null;
    }

    /// <summary>
    /// 获取 Steam 动态链接库 (DLL) 文件夹目录
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetSteamDynamicLinkLibraryPath()
    {
        return GetSteamDirPath();
    }

    /// <inheritdoc cref="ISteamService.SteamProgramPath"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetSteamProgramPath()
    {
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetRegistryVdfPath()
    {
        return null;
    }

    /// <inheritdoc cref="ISteamService.GetLastLoginUserName"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetLastSteamLoginUserName()
    {
        return "";
    }

    /// <inheritdoc cref="ISteamService.SetCurrentUser(string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetSteamCurrentUser(string userName)
    {
#if LINUX || MACOS || MACCATALYST
        try
        {
            var registryVdfPath = GetRegistryVdfPath();
            if (!string.IsNullOrWhiteSpace(registryVdfPath) && File.Exists(registryVdfPath))
            {
                dynamic v = VdfHelper.Read(registryVdfPath);
                if (v.Value.HKCU.Software.Valve.Steam.AutoLoginUser != null)
                {
                    v.Value.HKCU.Software.Valve.Steam.AutoLoginUser = userName;
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
}
