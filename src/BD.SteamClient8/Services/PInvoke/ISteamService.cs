using static System.String2;

namespace BD.SteamClient8.Services;

/// <summary>
/// Steam 相关助手、工具类服务
/// </summary>
public partial interface ISteamService
{
    static ISteamService Instance => Ioc.Get<ISteamService>();

    protected const string url_localhost_auth_public = Prefix_HTTP + "127.0.0.1:27060/auth/?u=public";
    const string url_steamcommunity_ = "steamcommunity.com";
    const string url_store_steampowered_ = "store.steampowered.com";
    const string url_steamcommunity = Prefix_HTTPS + url_steamcommunity_;
    const string url_store_steampowered = Prefix_HTTPS + url_store_steampowered_;
    const string url_store_steampowered_checkclientautologin = url_store_steampowered + "/login/checkclientautologin";
    const string url_steamcommunity_checkclientautologin = url_steamcommunity + "/login/checkclientautologin";
    static readonly Uri uri_store_steampowered_checkclientautologin = new(url_store_steampowered_checkclientautologin);

#if !(IOS || ANDROID)

    /// <summary>
    /// 连接 SteamClient 是否成功
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> IsConnectToSteamAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Steam 语言
    /// </summary>
    Task<ApiRspImpl<string?>> GetSteamLanguageString(CancellationToken cancellationToken = default);

    /// <summary>
    /// 本机的 Steam 游戏
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamApp[]?>> GetSteamApps(CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载的 Steam 游戏
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamApp[]?>> GetDownloadApps(CancellationToken cancellationToken = default);

    /// <summary>
    /// Steam 用户数组
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamUser[]?>> GetSteamUsers(CancellationToken cancellationToken = default);

    /// <summary>
    /// Steam 客户端连接的用户
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<SteamUser?>> GetCurrentSteamUser(CancellationToken cancellationToken = default);

    /// <summary>
    /// Steam 进程是否正在运行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> IsRunningSteamProcess(CancellationToken cancellationToken = default);

    /// <summary>
    /// 尝试结束 Steam 进程
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> TryKillSteamProcess(CancellationToken cancellationToken = default);

    /// <summary>
    /// Steam 进程是否正在运行，如果正在运行，返回进程PID提示用户去任务管理器中结束进程
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<int>> GetSteamProcessPid(CancellationToken cancellationToken = default);

    Task<ApiRspImpl<bool>> IsSteamChinaLauncher(CancellationToken cancellationToken = default);

    /// <summary>
    /// 启动 Steam
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl> StartSteam(string? arguments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用配置的参数启动 Steam
    /// </summary>
    Task<ApiRspImpl> StartSteamWithParameter(CancellationToken cancellationToken = default) /*=> StartSteam(SteamSettings.SteamStratParameter.Value)*/;

    /// <summary>
    /// 安全退出 Steam（如果有修改 Steam 数据的操作请退出后在执行不然 Steam 安全退出会还原修改）
    /// </summary>
    Task<ApiRspImpl> ShutdownSteamAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最后一次自动登录 Steam 用户名称
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<string?>> GetLastLoginUserName(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有记住登录 Steam 用户信息
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<List<SteamUser>>> GetRememberUserList(CancellationToken cancellationToken = default);

    Task<ApiRspImpl> UpdateAuthorizedDeviceList(IEnumerable<AuthorizedDevice> model, CancellationToken cancellationToken = default);

    Task<ApiRspImpl> RemoveAuthorizedDeviceList(AuthorizedDevice list, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有当前PC共享授权信息
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<List<AuthorizedDevice>>> GetAuthorizedDeviceList(CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置下次登录 Steam 用户
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl> SetSteamCurrentUserAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets whether the user is invisible or not
    /// </summary>
    /// <param name="steamId32">SteamID of user to update</param>
    /// <param name="ePersonaState">Persona state enum for user (0-7)</param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl> SetPersonaState(string steamId32, PersonaState ePersonaState, CancellationToken cancellationToken = default);

    Task<ApiRspImpl> DeleteLocalUserData(SteamUser user, bool isDeleteUserData = false, CancellationToken cancellationToken = default);

    Task<ApiRspImpl> UpdateLocalUserData(IEnumerable<SteamUser> users, CancellationToken cancellationToken = default);

    Task<ApiRspImpl> WatchLocalUserDataChange(Action changedAction, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从 Steam 本地客户端缓存文件中读取游戏数据
    /// </summary>
    Task<ApiRspImpl<List<SteamApp>?>> GetAppInfos(bool isSaveProperties = false, CancellationToken cancellationToken = default);

    Task<ApiRspImpl<List<ModifiedApp>?>> GetModifiedApps(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存修改后的游戏数据到 Steam 本地客户端缓存文件
    /// </summary>
    Task<ApiRspImpl> SaveAppInfosToSteam(CancellationToken cancellationToken = default);

    Task<ApiRspImpl<string?>> GetAppImageAsync(uint appId, LibCacheType type, SteamUser? mostRecentUser = null, CancellationToken cancellationToken = default);

    //ValueTask<ApiRspImpl> LoadAppImageAsync(SteamApp app, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存图片数据到 Steam 自定义封面文件夹
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl> SaveAppImageToSteamFileByByteArray(byte[]? imageBytes, SteamUser user, long appId, SteamGridItemType gridType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存图片数据到 Steam 自定义封面文件夹
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl> SaveAppImageToSteamFileByFilePath(string? imagePath, SteamUser user, long appId, SteamGridItemType gridType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取已安装的 SteamApp 列表(包括正在下载的项)
    /// </summary>
    Task<ApiRspImpl<List<SteamApp>?>> GetDownloadingAppList(CancellationToken cancellationToken = default);

    /// <summary>
    /// 监听 Steam 下载
    /// </summary>
    IAsyncEnumerable<ApiRspImpl<string>> StartWatchSteamDownloading(CancellationToken cancellationToken = default);

    /// <summary>
    /// 结束监听 Steam 下载
    /// </summary>
    Task<ApiRspImpl> StopWatchSteamDownloading(CancellationToken cancellationToken = default);

    /// <summary>
    /// 从任意文本中匹配批量提取 SteamKey
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    static IEnumerable<string> ExtractKeysFromString(string source)
    {
        var m = ExtractKeysFromStringRegex().Matches(source);
        var keys = new List<string>();
        if (m.Count > 0)
        {
            foreach (Match v in m.Cast<Match>())
            {
                keys.Add(v.Value);
            }
        }
        return keys!;
    }

    [GeneratedRegex("([0-9A-Z]{5})(?:\\-[0-9A-Z]{5}){2,4}", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ExtractKeysFromStringRegex();

#endif
}
