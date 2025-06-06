using BD.SteamClient8.Enums.WebApi;
using BD.SteamClient8.Enums.WebApi.SteamApps;
using BD.SteamClient8.Enums.WebApi.SteamGridDB;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;
using System.Text.RegularExpressions;
using static System.String2;

namespace BD.SteamClient8.Services.Abstractions.PInvoke;

/// <summary>
/// Steam 相关助手、工具类服务
/// </summary>
public partial interface ISteamService
{
}

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
partial interface ISteamService
{
    static ISteamService Instance => Ioc.Get<ISteamService>();

    /// <summary>
    /// Steam 文件夹目录
    /// </summary>
    string? SteamDirPath { get; }

    /// <summary>
    /// Steam 主程序的可执行文件路径
    /// </summary>
    string? SteamProgramPath { get; }

    /// <summary>
    /// 是否有正在运行的 Steam 进程
    /// </summary>
    bool IsRunningSteamProcess { get; }

    /// <summary>
    /// 尝试结束 Steam 进程
    /// </summary>
    /// <returns></returns>
    Task<bool> TryKillSteamProcessAsync();

    /// <summary>
    /// Steam 进程是否正在运行，如果正在运行，返回进程PID提示用户去任务管理器中结束进程
    /// </summary>
    /// <returns></returns>
    int GetSteamProcessPid();

    /// <summary>
    /// 是否已运行蒸汽中国程序进程
    /// </summary>
    /// <returns></returns>
    Task<bool> IsSteamChinaLauncherAsync();

    /// <summary>
    /// 启动 Steam
    /// </summary>
    /// <param name="arguments"></param>
    void StartSteam(string? arguments = null);

    /// <summary>
    /// 使用配置的参数启动 Steam
    /// </summary>
    void StartSteamWithDefaultParameter();

    /// <summary>
    /// 安全退出 Steam（如果有修改 Steam 数据的操作请退出后在执行不然 Steam 安全退出会还原修改）
    /// </summary>
    Task<bool> ShutdownSteamAsync();

    /// <summary>
    /// 获取最后一次自动登录 Steam 用户名称
    /// </summary>
    /// <returns></returns>
    string? GetLastLoginUserName();

    /// <summary>
    /// 获取所有记住登录 Steam 用户信息
    /// </summary>
    /// <returns></returns>
    List<SteamUser> GetRememberUserList();

    /// <summary>
    /// <see cref="GetRememberUserList"/> 返回结果的 Json 字符串形式
    /// </summary>
    /// <returns></returns>
    string? GetRememberUserListString(bool writeIndented = true);

    /// <summary>
    /// 更新授权设备列表
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<bool> UpdateAuthorizedDeviceListAsync(IEnumerable<AuthorizedDevice> model);

    /// <summary>
    /// 从授权设备列表中移除指定的授权设备
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    Task<bool> RemoveAuthorizedDeviceListAsync(AuthorizedDevice list);

    /// <summary>
    /// 获取所有当前 PC 共享授权信息
    /// </summary>
    /// <returns></returns>
    List<AuthorizedDevice> GetAuthorizedDeviceList();

    /// <summary>
    /// 设置下次登录 Steam 用户
    /// </summary>
    /// <param name="userName"></param>
    Task<bool> SetSteamCurrentUserAsync(string userName);

    /// <summary>
    /// 设置用户是否不可见
    /// </summary>
    /// <param name="steamId32">要更新的用户的 SteamId</param>
    /// <param name="ePersonaState">用户的 Persona 状态枚举（0-7）</param>
    Task SetPersonaStateAsync(string steamId32, PersonaState ePersonaState);

    /// <summary>
    /// 删除本地用户数据
    /// </summary>
    /// <param name="user"></param>
    /// <param name="isDeleteUserData"></param>
    Task<bool> DeleteLocalUserDataAsync(SteamUser user, bool isDeleteUserData = false);

    /// <summary>
    /// 更新本地用户数据
    /// </summary>
    /// <param name="users"></param>
    /// <returns></returns>
    Task<bool> UpdateLocalUserDataAsync(IEnumerable<SteamUser> users);

    /// <summary>
    /// 本地用户数据更改时事件
    /// </summary>
    event Action<(WatcherChangeTypes changeType, string fullPath, string? name)>? OnWatchLocalUserDataChanged;

    /// <summary>
    /// 开始监视本地用户数据更改，开始后会调用 <see cref="OnWatchLocalUserDataChanged"/> 事件通知更改
    /// </summary>
    void StartWatchLocalUserDataChange();

    /// <summary>
    /// 从 Steam 本地客户端缓存文件中读取游戏数据
    /// </summary>
    Task<List<SteamApp>?> GetAppInfos(bool isSaveProperties = false);

    /// <summary>
    /// 获取修改的应用程序
    /// </summary>
    /// <returns></returns>
    List<ModifiedApp>? GetModifiedApps();

    /// <summary>
    /// 保存修改后的游戏数据到 Steam 本地客户端缓存文件
    /// </summary>
    Task<bool> SaveAppInfosToSteam();

    /// <summary>
    /// 获取应用程序图片本地文件路径
    /// </summary>
    string? GetAppImageFilePath(uint appId, LibCacheType type, bool checkFileExists = true);

    //ValueTask LoadAppImageAsync(SteamApp app);

    /// <summary>
    /// 保存图片流到 Steam 自定义封面文件夹
    /// </summary>
    /// <returns></returns>
    Task<bool> SaveAppImageToSteamFile(string? imageFilePath, string steamId32, uint appId, SteamGridItemType gridType);

    Task<bool> SaveAppImageToSteamFile(string? imageFilePath, SteamUser user, uint appId, SteamGridItemType gridType)
    {
        var steamId32 = user.SteamId32.ToString();
        var result = SaveAppImageToSteamFile(imageFilePath, steamId32, appId, gridType);
        return result;
    }

    /// <summary>
    /// 获取已安装的 SteamApp 列表(包括正在下载的项)
    /// </summary>
    IReadOnlyList<SteamApp>? GetDownloadingAppList();

    /// <summary>
    /// Steam 下载变更时事件
    /// </summary>
    event Action<(WatcherChangeTypes changeType, string fullPath, string? name, SteamApp)>? OnWatchSteamDownloadingChanged;

    /// <summary>
    /// Steam 下载删除文件时事件
    /// </summary>
    event Action<(WatcherChangeTypes changeType, string fullPath, string? name, uint)>? OnWatchSteamDownloadingDeleted;

    /// <summary>
    /// 开始监听 Steam 下载，开始后会调用 <see cref="OnWatchSteamDownloadingChanged"/> 与 <see cref="OnWatchSteamDownloadingDeleted"/> 事件通知更改
    /// </summary>
    void StartWatchSteamDownloading();

    /// <summary>
    /// 结束监听 Steam 下载
    /// </summary>
    void StopWatchSteamDownloading();
}

partial interface ISteamService // 常量
{
    //protected const string url_localhost_auth_public = Prefix_HTTP + "127.0.0.1:27060/auth/?u=public";
    //const string url_steamcommunity_ = "steamcommunity.com";
    //const string url_store_steampowered_ = "store.steampowered.com";
    //const string url_steamcommunity = Prefix_HTTPS + url_steamcommunity_;
    //const string url_store_steampowered = Prefix_HTTPS + url_store_steampowered_;
    //const string url_store_steampowered_checkclientautologin = url_store_steampowered + "/login/checkclientautologin";
    //const string url_steamcommunity_checkclientautologin = url_steamcommunity + "/login/checkclientautologin";
    //static readonly Uri uri_store_steampowered_checkclientautologin = new(url_store_steampowered_checkclientautologin);
}
#endif

partial interface ISteamService // 正则表达式
{
    /// <summary>
    /// 从任意文本中匹配批量提取 SteamKey
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    static IReadOnlyList<string> ExtractKeysFromString(string source)
    {
        var m = ExtractKeysFromStringRegex().Matches(source);
        var keys = new List<string>();
        if (m.Count > 0)
        {
            foreach (Match v in m)
            {
                keys.Add(v.Value);
            }
        }
        return keys;
    }

    [GeneratedRegex("([0-9A-Z]{5})(?:\\-[0-9A-Z]{5}){2,4}", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ExtractKeysFromStringRegex();
}