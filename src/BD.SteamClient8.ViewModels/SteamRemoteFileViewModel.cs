namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600
public class SteamRemoteFileViewModel : ReactiveObject
{
    public string Name { get; private set; }

    public bool Exists { get; }

    public bool IsPersisted { get; }

    public long Size { get; }

    public DateTimeOffset Timestamp { get; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public SteamKit2ERemoteStoragePlatform SyncPlatforms { get; set; }

#endif

    public SteamRemoteFileViewModel(string name)
    {
        Name = name;
    }

    public SteamRemoteFileViewModel(string name, long length, bool exists, bool isPersisted, long timestamp)
    {
        Name = name;
        Size = length;
        Exists = exists;
        IsPersisted = isPersisted;
        Timestamp = timestamp.ToDateTimeOffsetS();
    }

    public int Read(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileRead(Name, buffer);
    }

    public byte[] ReadAllBytes()
    {
        byte[] buffer = new byte[Size];
        int read = Read(buffer);
        if (read != buffer.Length)
            throw new IOException("Could not read entire file.");
        return buffer;
    }

    public bool WriteAllBytes(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileWrite(Name, buffer);
    }

    public bool Forget()
    {
        return ISteamworksLocalApiService.Instance.FileForget(Name);
    }

    public bool Delete()
    {
        return ISteamworksLocalApiService.Instance.FileDelete(Name);
    }

    /// <summary>
    /// <see cref="SteamRemoteFile"/> 隐式转换 <see cref="SteamRemoteFileViewModel"/>
    /// </summary>
    /// <param name="steamRemoteFile"></param>
    public static implicit operator SteamRemoteFileViewModel(SteamRemoteFile steamRemoteFile) => new(steamRemoteFile);

    /// <summary>
    /// <see cref="SteamRemoteFile"/> 构造实例 <see cref="SteamRemoteFileViewModel"/>
    /// </summary>
    /// <param name="steamRemoteFile"></param>
    public SteamRemoteFileViewModel(SteamRemoteFile steamRemoteFile)
    {
        Name = steamRemoteFile.Name;
        Exists = steamRemoteFile.Exists;
        IsPersisted = steamRemoteFile.IsPersisted;
        Size = steamRemoteFile.Size;
        Timestamp = steamRemoteFile.Timestamp;
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

        SyncPlatforms = steamRemoteFile.SyncPlatforms;
#endif
    }
}
