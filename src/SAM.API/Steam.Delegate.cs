namespace SAM.API;

static partial class Steam
{
    /// <summary>
    /// Customizing the delegate to get the Steam installation path.
    /// </summary>
    public static Func<string?>? GetInstallPathDelegate { private get; set; }

    /// <summary>
    /// Custom delegate to get path to steamclient64.dll, steamclient.dll, steamclient.dylib, steamclient.so native library.
    /// </summary>
    public static Func<string?>? GetSteamClientNativeLibraryPathDelegate { private get; set; }

    public static bool IsSetCustomPathDelegate => GetInstallPathDelegate != null || GetSteamClientNativeLibraryPathDelegate != null;
}
