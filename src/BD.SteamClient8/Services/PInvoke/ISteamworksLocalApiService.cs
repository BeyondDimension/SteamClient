#if !(IOS || ANDROID)
namespace BD.SteamClient8.Services;

/// <summary>
/// Steamworks 本地 API 服务
/// <para>https://partner.steamgames.com/doc/home</para>
/// </summary>
public interface ISteamworksLocalApiService
{
    /// <summary>
    /// 获取 <see cref="ISteamworksLocalApiService"/> 服务实例
    /// </summary>
    static ISteamworksLocalApiService Instance => Ioc.Get<ISteamworksLocalApiService>();

    /// <summary>
    /// 用于标识和记录日志信息
    /// </summary>
    protected const string TAG = "SteamworksLocalApiS";

    /// <summary>
    /// 当前平台是否支持
    /// </summary>
    static bool IsSupported { get; } = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X86 => true,
        Architecture.X64 => true,
#if MACOS
        Architecture.Arm64 => true,
#endif
        _ => false,
    };

    /// <summary>
    /// 释放 Steam 客户端资源
    /// </summary>
    Task<ApiRspImpl> DisposeSteamClient(CancellationToken cancellationToken = default);

    /// <summary>
    /// 初始化 Steamworks 本地 API 服务并设置 App Id
    /// </summary>
    /// <param name="appid">要使用的 App ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>初始化是否成功</returns>
    Task<ApiRspImpl> Initialize(int appid = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前用户的 Steam ID（64 位）
    /// </summary>
    /// <returns>当前用户的 Steam ID</returns>
    Task<ApiRspImpl<long>> GetSteamId64(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户是否拥有指定的应用程序
    /// </summary>
    /// <param name="appid">AppId</param>
    /// <param name="cancellationToken"></param>
    /// <returns>如果用户拥有该应用程序，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    Task<ApiRspImpl<bool>> OwnsApp(uint appid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户是否拥有指定的应用程序列表
    /// </summary>
    /// <param name="apps">Steam 应用程序列表</param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl<List<SteamApp>?>> OwnsApps(List<SteamApp> apps, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取应用程序的指定数据
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl<string>> GetAppData(uint appid, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查 Steam 是否是中国版本的启动器
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> IsSteamChinaLauncher(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查 Steam 是否处于大图模式
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> IsSteamInBigPictureMode(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取应用程序的活跃时间（单位：秒）
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<uint>> GetSecondsSinceAppActive(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务器实时时间
    /// </summary>
    /// <returns></returns>
    Task<ApiRspImpl<uint>> GetServerRealTime(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查应用程序是否已安装
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> IsAppInstalled(uint appid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取应用程序的安装目录
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<string>> GetAppInstallDir(uint appid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 IP 所属国家或地区
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<string>> GetCountryOrRegionByIP(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前游戏语言
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<string>> GetCurrentGameLanguage(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取可用的游戏语言
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<string>> GetAvailableGameLanguages(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定统计数据的整数值
    /// </summary>
    /// <param name="name">统计数据的名称</param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否成功获取到统计数据的整数值</returns>
    Task<ApiRspImpl<int?>> GetStatValueInt(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定统计数据的浮点数值
    /// </summary>
    /// <param name="name">统计数据的名称</param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否成功获取到统计数据的浮点数值</returns>
    Task<ApiRspImpl<float?>> GetStatValueFloat(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定成就的完成状态
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否成功获取到成就的完成状态</returns>
    Task<ApiRspImpl<bool?>> GetAchievementState(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定成就的完成状态和解锁时间
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否成功获取到成就的完成状态和解锁时间</returns>
    Task<ApiRspImpl<AchievementAndUnlockTimeResult?>> GetAchievementAndUnlockTime(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定成就的完成百分比
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <param name="cancellationToken"></param>
    /// <returns>是否成功获取到成就的完成百分比</returns>
    Task<ApiRspImpl<float?>> GetAchievementAchievedPercent(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加用户统计数据接收回调函数
    /// </summary>
    /// <param name="cancellationToken"></param>
    IAsyncEnumerable<ApiRspImpl<UserStatsReceivedResult>> AddUserStatsReceivedCallback(CancellationToken cancellationToken = default);

    /// <summary>
    /// 请求当前玩家的统计数据是否成功
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRspImpl<bool>> RequestCurrentStats(CancellationToken cancellationToken = default);

    /// <summary>
    /// 重置所有统计数据
    /// </summary>
    /// <param name="achievementsToo">是否同时重置成就</param>
    /// <param name="cancellationToken"></param>
    /// <returns>重置结果，成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    Task<ApiRspImpl<bool>> ResetAllStats(bool achievementsToo, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置成就状态
    /// </summary>
    /// <param name="name">成就名称</param>
    /// <param name="state">成就状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    Task<ApiRspImpl<bool>> SetAchievement(string name, bool state, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置统计数据值（整型）
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>设置结果，成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    Task<ApiRspImpl<bool>> SetStatInt32Value(string name, int value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置统计数据值（浮点型）
    /// </summary>
    /// <param name="name">统计数据名称</param>
    /// <param name="value">统计数据值</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    Task<ApiRspImpl<bool>> SetStatSingleValue(string name, float value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将变动的统计与成就数据发送至服务器进行持久保存
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>若失败，则不会发送任何数据至服务器</returns>
    Task<ApiRspImpl<bool>> StoreStats(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步获取全球获得该游戏各个成就的玩家百分比数据
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl<nint>> RequestGlobalAchievementPercentages(CancellationToken cancellationToken = default);

    /// <summary>
    /// 运行回调函数
    /// </summary>
    /// <param name="server"></param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl> RunCallbacks(bool server, CancellationToken cancellationToken = default);

    #region SteamRemoteStorage

    /// <summary>
    /// 获得可用的字节数，以及在用户的 Steam 云存储中使用的字节数
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>获取结果</returns>
    Task<ApiRspImpl<CloudArchiveQuotaResult?>> GetCloudArchiveQuota(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取云存档文件列表
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>云存档文件列表</returns>
    Task<ApiRspImpl<List<SteamRemoteFile>?>> GetCloudArchiveFiles(CancellationToken cancellationToken = default);

    /// <summary>
    /// 打开一个二进制文件，将文件内容读取至一个字节数组，然后关闭文件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="length"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>如果文件不存在或读取失败，则返回 <see cref="IApiRsp{TContent}.Content"/> 内容为 <see langword="null"/></returns>
    Task<ApiRspImpl<byte[]?>> FileRead(string name, long length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建一个新文件，将字节写入文件，再关闭文件。 目标文件若已存在，将被覆盖
    /// </summary>
    /// <param name="name"></param>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    Task<ApiRspImpl<bool>> FileWrite(string name, byte[] buffer, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将文件从远程存储删除，但保留在本地磁盘上，且仍能从 API 访问
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see langword="true"/> 表示文件存在且已被成功遗忘；否则返回 <see langword="false"/></returns>
    Task<ApiRspImpl<bool>> FileForget(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从本地磁盘中删除一个文件，并将该删除传播到云端
    /// 此函数应该只在用户主动删除文件时使用。 如果您希望将一个文件从 Steam 云中移除，但将其保留在用户的本地磁盘，则需使用 <see cref="FileForget"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see langword="true"/> 表示文件存在且已成功删除；否则，如果文件不存在，返回 <see langword="false"/></returns>
    Task<ApiRspImpl<bool>> FileDelete(string name, CancellationToken cancellationToken = default);

    #endregion
}
#endif