namespace BD.SteamClient.Services.Implementation;

abstract partial class SteamServiceImpl
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public abstract ISteamConnectService Conn { get; }

    public virtual void StartSteamWithParameter()
        => StartSteam(SteamSettingsStratParameter);

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
    protected abstract string SaveEditedAppInfo_SaveFailed { get; }

    /// <summary>
    /// SteamSettings.SteamStratParameter.Value
    /// </summary>
    protected abstract string SteamSettingsStratParameter { get; }

    /// <summary>
    /// SteamSettings.IsRunSteamAdministrator.Value
    /// </summary>
    protected abstract bool SteamSettingsIsRunSteamAdministrator { get; }

    /// <summary>
    /// GameLibrarySettings.HideGameList.Value
    /// </summary>
    protected abstract Dictionary<uint, string?> GameLibrarySettingsHideGameList { get; }

#endif
}