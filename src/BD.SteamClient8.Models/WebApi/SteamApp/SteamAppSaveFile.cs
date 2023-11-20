namespace BD.SteamClient8.Models.WebApi.SteamApp;

#pragma warning disable SA1600
public class SteamAppSaveFile
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public SteamAppSaveFile(uint appid, string? root, string? path, string? pattern)
    {
        ParentAppId = appid;
        Root = root;
        Path = path;
        Pattern = pattern;
    }

    public uint ParentAppId { get; set; }

    public string? Root { get; set; }

    public string? Path { get; set; }

    public string? Pattern { get; set; }

    public string? FormatDirPath { get; set; }

    public string? FormatFilePath { get; set; }

    public bool Recursive { get; set; }

    public string? Platforms { get; set; }
#endif
}
