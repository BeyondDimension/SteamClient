#if !(IOS || ANDROID || MACCATALYST)
using BD.Common8.Models;
using BD.Common8.Models.Abstractions;
using BD.SteamClient8.Models.PInvoke;
using BD.SteamClient8.Models.WebApi;
using BD.SteamClient8.Models.WebApi.SteamApps;
using System.Runtime.InteropServices;

namespace BD.SteamClient8.Services.Abstractions.PInvoke;

partial interface ISteamworksLocalApiService
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
    /// 释放 Steam 客户端资源
    /// </summary>
    void DisposeSteamClient();

    /// <summary>
    /// 初始化 Steamworks 本地 API 服务并设置 App Id
    /// </summary>
    /// <param name="appid">要使用的 App ID</param>
    /// <param name="processPathIsReadOnly"></param>
    /// <returns>初始化是否成功</returns>
    Exception? Initialize(int appid, bool processPathIsReadOnly);

    /// <summary>
    /// 获取当前用户的 Steam ID（64 位）
    /// </summary>
    /// <returns>当前用户的 Steam ID</returns>
    ulong GetSteamId64();

    /// <summary>
    /// 检查用户是否拥有指定的应用程序
    /// </summary>
    /// <param name="appid">AppId</param>
    /// <returns>如果用户拥有该应用程序，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool OwnsApp(uint appid);

    /// <summary>
    /// 检查用户是否拥有指定的应用程序列表
    /// </summary>
    /// <param name="apps">Steam 应用程序列表</param>
    List<SteamApp>? OwnsApps(List<SteamApp> apps);

    /// <summary>
    /// 获取应用程序的指定数据
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="key"></param>
    string? GetAppData(uint appid, string key);

    /// <summary>
    /// 检查 Steam 是否是中国版本的启动器
    /// </summary>
    /// <returns></returns>
    bool IsSteamChinaLauncher();

    /// <summary>
    /// 检查 Steam 是否处于大图模式
    /// </summary>
    /// <returns></returns>
    bool IsSteamInBigPictureMode();

    /// <summary>
    /// 获取应用程序的活跃时间（单位：秒）
    /// </summary>
    /// <returns></returns>
    uint GetSecondsSinceAppActive();

    /// <summary>
    /// 获取服务器实时时间
    /// </summary>
    /// <returns></returns>
    uint GetServerRealTime();

    /// <summary>
    /// 检查应用程序是否已安装
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    bool IsAppInstalled(uint appid);

    /// <summary>
    /// 获取应用程序的安装目录
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    string? GetAppInstallDir(uint appid);

    /// <summary>
    /// 获取 IP 所属国家或地区
    /// </summary>
    /// <returns></returns>
    string? GetCountryOrRegionByIP();

    /// <summary>
    /// 获取当前游戏语言
    /// </summary>
    /// <returns></returns>
    string? GetCurrentGameLanguage();

    /// <summary>
    /// 获取可用的游戏语言
    /// </summary>
    /// <returns></returns>
    string? GetAvailableGameLanguages();

    /// <summary>
    /// 获取指定统计数据的整数值
    /// </summary>
    /// <param name="name">统计数据的名称</param>
    /// <returns>是否成功获取到统计数据的整数值</returns>
    int? GetStatValueInt(string name);

    /// <summary>
    /// 获取指定统计数据的浮点数值
    /// </summary>
    /// <param name="name">统计数据的名称</param>
    /// <returns>是否成功获取到统计数据的浮点数值</returns>
    float? GetStatValueFloat(string name);

    /// <summary>
    /// 获取指定成就的完成状态
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <returns>是否成功获取到成就的完成状态</returns>
    bool? GetAchievementState(string name);

    /// <summary>
    /// 获取指定成就的完成状态和解锁时间
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <returns>是否成功获取到成就的完成状态和解锁时间</returns>
    AchievementAndUnlockTimeResult? GetAchievementAndUnlockTime(string name);

    /// <summary>
    /// 获取指定成就的完成百分比
    /// </summary>
    /// <param name="name">成就的名称</param>
    /// <returns>是否成功获取到成就的完成百分比</returns>
    float? GetAchievementAchievedPercent(string name);

    /// <summary>
    /// 用户统计数据接收事件，需要先调用一次 <see cref="RegisterUserStatsReceivedCallback"/> 方法注册回调函数
    /// </summary>
    event Action<UserStatsReceivedResult>? OnUserStatsReceived;

    /// <summary>
    /// 注册用户统计数据接收回调函数
    /// </summary>
    void RegisterUserStatsReceivedCallback();

    /// <summary>
    /// 请求当前玩家的统计数据是否成功
    /// </summary>
    /// <returns></returns>
    bool RequestCurrentStats();

    /// <summary>
    /// 重置所有统计数据
    /// </summary>
    /// <param name="achievementsToo">是否同时重置成就</param>
    /// <returns>重置结果，成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool ResetAllStats(bool achievementsToo);

    /// <summary>
    /// 设置成就状态
    /// </summary>
    /// <param name="name">成就名称</param>
    /// <param name="state">成就状态</param>
    /// <returns>成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool SetAchievement(string name, bool state);

    /// <summary>
    /// 设置统计数据值（整型）
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>设置结果，成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool SetStatInt32Value(string name, int value);

    /// <summary>
    /// 设置统计数据值（浮点型）
    /// </summary>
    /// <param name="name">统计数据名称</param>
    /// <param name="value">统计数据值</param>
    /// <returns>成功返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    bool SetStatSingleValue(string name, float value);

    /// <summary>
    /// 将变动的统计与成就数据发送至服务器进行持久保存
    /// </summary>
    /// <returns>若失败，则不会发送任何数据至服务器</returns>
    bool StoreStats();

    /// <summary>
    /// 异步获取全球获得该游戏各个成就的玩家百分比数据
    /// </summary>
    nint RequestGlobalAchievementPercentages();

    /// <summary>
    /// 运行回调函数
    /// </summary>
    /// <param name="server"></param>
    void RunCallbacks(bool server);

    #region SteamRemoteStorage

    /// <summary>
    /// 获得可用的字节数，以及在用户的 Steam 云存储中使用的字节数
    /// </summary>
    /// <returns>获取结果</returns>
    CloudArchiveQuotaResult? GetCloudArchiveQuota();

    /// <summary>
    /// 获取云存档文件列表
    /// </summary>
    /// <returns>云存档文件列表</returns>
    SteamRemoteFile[]? GetCloudArchiveFiles();

    /// <summary>
    /// 打开一个二进制文件，将文件内容读取至一个字节数组，然后关闭文件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="length"></param>
    /// <returns>如果文件不存在或读取失败，则返回内容为 <see langword="null"/></returns>
    [Obsolete("use int FileRead(string, byte[])", true)]
    byte[]? FileRead(string name, long length);

    /// <summary>
    /// 打开一个二进制文件，将文件内容读取至一个字节数组，然后关闭文件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="buffer"></param>
    /// <returns>如果文件不存在或读取失败，则返回 -1</returns>
    int FileRead(string name, byte[] buffer);

    /// <summary>
    /// 创建一个新文件，将字节写入文件，再关闭文件。 目标文件若已存在，将被覆盖
    /// </summary>
    /// <param name="name"></param>
    /// <param name="buffer"></param>
    bool FileWrite(string name, byte[] buffer);

    /// <summary>
    /// 将文件从远程存储删除，但保留在本地磁盘上，且仍能从 API 访问
    /// </summary>
    /// <param name="name"></param>
    /// <returns><see langword="true"/> 表示文件存在且已被成功遗忘；否则返回 <see langword="false"/></returns>
    bool FileForget(string name);

    /// <summary>
    /// 从本地磁盘中删除一个文件，并将该删除传播到云端
    /// 此函数应该只在用户主动删除文件时使用。 如果您希望将一个文件从 Steam 云中移除，但将其保留在用户的本地磁盘，则需使用 <see cref="FileForget"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns><see langword="true"/> 表示文件存在且已成功删除；否则，如果文件不存在，返回 <see langword="false"/></returns>
    bool FileDelete(string name);

    #endregion
}
#endif