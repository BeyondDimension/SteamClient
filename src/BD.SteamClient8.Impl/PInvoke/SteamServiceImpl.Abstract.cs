namespace BD.SteamClient8.Impl;

public abstract partial class SteamServiceImpl
{
#if !(IOS || ANDROID)

    /// <inheritdoc/>
    public virtual Task<ApiRspImpl> StartSteamWithParameter(CancellationToken cancellationToken = default)
        => StartSteam(StratSteamDefaultParameter, cancellationToken);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetLastSteamLoginUserName()
    {
#if WINDOWS
        return Registry.CurrentUser.Read(ISteamService.SteamRegistryPath, "AutoLoginUser");
#else
        return "";
#endif
    }

    /// <inheritdoc cref="ISteamService.SetSteamCurrentUserAsync(string, CancellationToken)"/>
    public virtual async Task<ApiRspImpl> SetSteamCurrentUserAsync(string userName, CancellationToken cancellationToken = default)
    {
        // override BD.WTTS.Services.Implementation.SteamServiceImpl2.SetSteamCurrentUser
#if WINDOWS
        Registry.CurrentUser.AddOrUpdate(ISteamService.SteamRegistryPath, "AutoLoginUser", userName, RegistryValueKind.String);
        Registry.CurrentUser.AddOrUpdate(ISteamService.SteamRegistryPath, "RememberPassword", 1, RegistryValueKind.DWord);
#elif LINUX || MACOS || MACCATALYST
        try
        {
            var registryVdfPath = ISteamService.RegistryVdfPath;
            if (!string.IsNullOrWhiteSpace(registryVdfPath) && File.Exists(registryVdfPath))
            {
                var v = VdfHelper.Read(registryVdfPath);

                if (v["HKCU"]["Software"]["Valve"]["Steam"] is KVCollectionValue steamItem)
                {
                    if (steamItem["AutoLoginUser"] is not KVObjectValue<string> kv)
                    {
                        steamItem.Add(new KVObject("AutoLoginUser", new KVObjectValue<string>(userName, KVValueType.String)));
                    }
                    else
                    {
                        kv.Value = userName;
                    }
                    if (steamItem["RememberPassword"] is not KVObjectValue<string> rememberPasswordKV)
                    {
                        steamItem?.Add(new KVObject("RememberPassword", new KVObjectValue<string>("1", KVValueType.String)));
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
        await ValueTask.CompletedTask;
        return ApiRspHelper.Ok();
    }

#endif
}