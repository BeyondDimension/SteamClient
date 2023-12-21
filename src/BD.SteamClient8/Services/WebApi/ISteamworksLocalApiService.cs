namespace BD.SteamClient8.Services.WebApi;

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
    bool IsSupported => false;

    /// <summary>
    /// 释放 Steam 客户端资源
    /// </summary>
    void DisposeSteamClient() { }

    /// <summary>
    /// 初始化 Steamworks 本地 API 服务
    /// </summary>
    /// <returns>初始化是否成功</returns>
    bool Initialize() => default;

    /// <summary>
    /// 初始化 Steamworks 本地 API 服务并设置 App Id
    /// </summary>
    /// <param name="appid">要使用的 App ID</param>
    /// <returns>初始化是否成功</returns>
    bool Initialize(int appid) => default;

    /// <summary>
    /// 获取当前用户的 Steam ID（64 位）
    /// </summary>
    /// <returns>当前用户的 Steam ID</returns>
    long GetSteamId64() => default;

    /// <summary>
    /// 检查用户是否拥有指定的应用程序
    /// </summary>
    /// <param name="appid">AppId</param>
    /// <returns>如果用户拥有该应用程序，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool OwnsApps(uint appid) => default;

    /// <summary>
    /// 检查用户是否拥有指定的应用程序列表
    /// </summary>
    /// <param name="apps">Steam 应用程序列表</param>
    IEnumerable<SteamApp> OwnsApps(IEnumerable<SteamApp> apps) => Array.Empty<SteamApp>();

    /// <summary>
    /// 获取应用程序的指定数据
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="key"></param>
    string GetAppData(uint appid, string key) => string.Empty;

    /// <summary>
    /// 检查 Steam 是否是中国版本的启动器
    /// </summary>
    /// <returns></returns>
    bool IsSteamChinaLauncher() => default;

    /// <summary>
    /// 检查Steam是否处于大图模式
    /// </summary>
    /// <returns></returns>
    bool IsSteamInBigPictureMode() => default;

    /// <summary>
    /// 获取应用程序的活跃时间（单位：秒）
    /// </summary>
    /// <returns></returns>
    uint GetSecondsSinceAppActive() => default;

    /// <summary>
    /// 获取服务器实时时间
    /// </summary>
    /// <returns></returns>
    uint GetServerRealTime() => default;

    /// <summary>
    /// 检查应用程序是否已安装
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    bool IsAppInstalled(uint appid) => default;

    /// <summary>
    /// 获取应用程序的安装目录
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    string GetAppInstallDir(uint appid) => string.Empty;

    /// <summary>
    /// 获取IP所属国家
    /// </summary>
    /// <returns></returns>
    string GetIPCountry() => string.Empty;

    /// <summary>
    /// 获取当前游戏语言
    /// </summary>
    /// <returns></returns>
    string GetCurrentGameLanguage() => string.Empty;

    /// <summary>
    /// 获取可用的游戏语言
    /// </summary>
    /// <returns></returns>
    string GetAvailableGameLanguages() => string.Empty;

    /// <summary>
    /// 获取指定统计数据的整数值
    /// </summary>
    /// <param name="name">统计数据的名称</param>
    /// <param name="value">返回获取到的整数值</param>
    /// <returns>是否成功获取到统计数据的整数值</returns>
    bool GetStatValue(string name, out int value)
    {
        value = default;
        return default;
    }

    /// <summary>
    /// 获取指定统计数据的浮点数值
    /// </summary>
    /// <param name="name">统计数据的名称</param>
    /// <param name="value">返回获取到的浮点数值</param>
    /// <returns>是否成功获取到统计数据的浮点数值</returns>
    bool GetStatValue(string name, out float value)
    {
        value = default;
        return default;
    }

    /// <summary>
    /// 获取指定成就的完成状态
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <param name="isAchieved">返回是否完成该成就</param>
    /// <returns>是否成功获取到成就的完成状态</returns>
    bool GetAchievementState(string name, out bool isAchieved)
    {
        isAchieved = default;
        return default;
    }

    /// <summary>
    /// 获取指定成就的完成状态和解锁时间
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <param name="isAchieved">返回是否完成该成就</param>
    /// <param name="unlockTime">返回解锁该成就的时间</param>
    /// <returns>是否成功获取到成就的完成状态和解锁时间</returns>
    bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime)
    {
        isAchieved = default;
        unlockTime = default;
        return default;
    }

    /// <summary>
    /// 获取指定成就的完成百分比
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <param name="percent">返回成就的完成百分比</param>
    /// <returns>是否成功获取到成就的完成百分比</returns>
    bool GetAchievementAchievedPercent(string name, out float percent)
    {
        percent = default;
        return default;
    }

    /// <summary>
    /// 添加用户统计数据接收回调函数
    /// </summary>
    /// <param name="action"></param>
    void AddUserStatsReceivedCallback(Action<IUserStatsReceived> action) { }

    /// <summary>
    /// 请求当前玩家的统计数据是否成功
    /// </summary>
    /// <returns></returns>
    bool RequestCurrentStats() => default;

    /// <summary>
    /// 重置所有统计数据
    /// </summary>
    /// <param name="achievementsToo">是否同时重置成就</param>
    /// <returns>重置结果，成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool ResetAllStats(bool achievementsToo) => default;

    /// <summary>
    /// 设置成就状态
    /// </summary>
    /// <param name="name">成就名称</param>
    /// <param name="state">成就状态</param>
    /// <returns>成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool SetAchievement(string name, bool state) => default;

    /// <summary>
    /// 设置统计数据值（整型）
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>设置结果，成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool SetStatValue(string name, int value) => default;

    /// <summary>
    /// 设置统计数据值（浮点型）
    /// </summary>
    /// <param name="name">统计数据名称</param>
    /// <param name="value">统计数据值</param>
    /// <returns>成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool SetStatValue(string name, float value) => default;

    /// <summary>
    /// 将变动的统计与成就数据发送至服务器进行持久保存
    /// </summary>
    /// <returns>若失败，则不会发送任何数据至服务器</returns>
    bool StoreStats() => default;

    /// <summary>
    /// 异步获取全球获得该游戏各个成就的玩家百分比数据
    /// </summary>
    void RequestGlobalAchievementPercentages() { }

    /// <summary>
    /// 运行回调函数
    /// </summary>
    /// <param name="server"></param>
    void RunCallbacks(bool server) { }

    /// <summary>
    /// 接收用户统计信息模型
    /// </summary>
    interface IUserStatsReceived
    {
        /// <summary>
        /// 游戏 Id
        /// </summary>
        ulong GameId { get; }

        /// <summary>
        /// 结果
        /// </summary>
        int Result { get; }
    }

    #region SteamRemoteStorage

    /// <summary>
    /// 获得可用的字节数，以及在用户的 Steam 云存储中使用的字节数
    /// </summary>
    /// <param name="totalBytes">返回用户可访问的字节总量</param>
    /// <param name="availableBytes">返回可用的字节数</param>
    /// <returns>获取结果</returns>
    bool GetCloudArchiveQuota(out ulong totalBytes, out ulong availableBytes)
    {
        totalBytes = 0;
        availableBytes = 0;
        return false;
    }

    /// <summary>
    /// 获取云存档文件列表
    /// </summary>
    /// <returns>云存档文件列表</returns>
    List<SteamRemoteFile>? GetCloudArchiveFiles() => default;

    /// <summary>
    /// 打开一个二进制文件，将文件内容读取至一个字节数组，然后关闭文件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    int FileRead(string name, byte[] buffer) => default;

    /// <summary>
    /// 创建一个新文件，将字节写入文件，再关闭文件。 目标文件若已存在，将被覆盖
    /// </summary>
    /// <param name="name"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    bool FileWrite(string name, byte[] buffer) => default;

    /// <summary>
    /// 将文件从远程存储删除，但保留在本地磁盘上，且仍能从 API 访问
    /// </summary>
    /// <param name="name"></param>
    /// <returns><see langword="true"/> 表示文件存在且已被成功遗忘；否则返回 <see langword="false"/></returns>
    bool FileForget(string name) => default;

    /// <summary>
    /// 从本地磁盘中删除一个文件，并将该删除传播到云端
    /// 此函数应该只在用户主动删除文件时使用。 如果您希望将一个文件从 Steam 云中移除，但将其保留在用户的本地磁盘，则需使用 <see cref="FileForget"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns><see langword="true"/> 表示文件存在且已成功删除；否则，如果文件不存在，返回 <see langword="false"/></returns>
    bool FileDelete(string name) => default;

    #endregion
}
