#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

/// <summary>
/// Steam 同步文件
/// </summary>
[MP2Obj(MP2SerializeLayout.Sequential)]
public sealed partial record class SteamRemoteFile
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; private set; } = "";

    /// <summary>
    /// 是否已存在
    /// </summary>
    public bool Exists { get; }

    /// <summary>
    /// 是否持久化
    /// </summary>
    public bool IsPersisted { get; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// 文件时间戳
    /// </summary>
    public DateTimeOffset Timestamp { get; }

#if !(IOS || ANDROID)

    /// <summary>
    /// 系统平台
    /// </summary>
    public SteamKit2ERemoteStoragePlatform SyncPlatforms { get; set; }

#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamRemoteFile"/> class.
    /// </summary>
    [MPConstructor, MP2Constructor, SystemTextJsonConstructor]
    public SteamRemoteFile()
    {
    }

    /// <summary>
    /// 通过名称构造 <see cref="SteamRemoteFile"/> 实例
    /// </summary>
    /// <param name="name"></param>
    public SteamRemoteFile(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 通过名称，文件大小，是否存在，是否持久化，文件时间戳来构造 <see cref="SteamRemoteFile"/> 实例
    /// </summary>
    /// <param name="name"></param>
    /// <param name="length"></param>
    /// <param name="exists"></param>
    /// <param name="isPersisted"></param>
    /// <param name="timestamp"></param>
    public SteamRemoteFile(string name, long length, bool exists, bool isPersisted, long timestamp)
    {
        Name = name;
        Size = length;
        Exists = exists;
        IsPersisted = isPersisted;
        Timestamp = timestamp.ToDateTimeOffsetS();
    }
}