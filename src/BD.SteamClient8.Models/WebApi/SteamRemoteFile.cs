namespace BD.SteamClient8.Models.WebApi;

#pragma warning disable SA1600 // Elements should be documented

public record class SteamRemoteFile
{
    public string Name { get; private set; }

    public bool Exists { get; }

    public bool IsPersisted { get; }

    public long Size { get; }

    public DateTimeOffset Timestamp { get; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public SteamKit2ERemoteStoragePlatform SyncPlatforms { get; set; }

#endif

    public SteamRemoteFile(string name)
    {
        Name = name;
    }

    public SteamRemoteFile(string name, long length, bool exists, bool isPersisted, long timestamp)
    {
        Name = name;
        Size = length;
        Exists = exists;
        IsPersisted = isPersisted;
        Timestamp = timestamp.ToDateTimeOffsetS();
    }
}