#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using SysIOPath = System.IO.Path;
#endif

namespace BD.SteamClient8.ViewModels;

#pragma warning disable SA1600
[ViewModelWrapperGenerated(typeof(SteamAppSaveFile)
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
#pragma warning disable SA1115// Parameter should follow comma
#pragma warning disable SA1001 // Commas should be spaced correctly
#pragma warning disable SA1113 // Comma should be on the same line as previous parameter
    , Properties = [
        nameof(SteamAppSaveFile.FormatDirPath),
        nameof(SteamAppSaveFile.FormatFilePath),
    ]
#pragma warning restore SA1113 // Comma should be on the same line as previous parameter
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore SA1115 // Parameter should follow comma
#endif
)]
public partial class SteamAppSaveFileViewModel
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public void FormatPathGenerate()
    {
        if (string.IsNullOrEmpty(Root) || string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(Pattern))
        {
            return;
        }

        var path = "";

        if (string.Equals(Root, "WinAppDataRoaming", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (string.Equals(Root, "WinAppDataLocal", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else if (string.Equals(Root, "WinAppDataLocalLow", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low";
        }
        else if (string.Equals(Root, "WinMyDocuments", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        else if (string.Equals(Root, "gameinstall", StringComparison.OrdinalIgnoreCase))
        {
            path = ISteamService.Instance.Conn.SteamApps.Lookup(ParentAppId).Value?.InstalledDir;
        }
        else
        {
            path = "unknown";
        }

        if (string.IsNullOrEmpty(path))
            return;

        var p = Path.Replace("{64BitSteamID}", ISteamService.Instance.Conn.CurrentSteamUser?.SteamId64.ToString())
                .Replace("{Steam3AccountID}", ISteamService.Instance.Conn.CurrentSteamUser?.SteamId32.ToString());

        if (SysIOPath.IsPathRooted(p))
        {
            p = p.Remove(0, SysIOPath.GetPathRoot(p)!.Length);
        }

        path = SysIOPath.Combine(path, p);

        FormatDirPath = SysIOPath.GetFullPath(path);

        FormatFilePath = SysIOPath.GetFullPath(SysIOPath.Combine(path, Pattern));
    }
#endif
}
