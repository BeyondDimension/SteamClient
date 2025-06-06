#if !(IOS || ANDROID)
using ValveKeyValue;

namespace BD.SteamClient8.Helpers;

public static partial class SteamPathHelper // SteamProgramPath | UnixRegistryVdfFilePath | AutoLoginUser
{
    public static partial string? GetSteamProgramPath();

#if !WINDOWS
    public static partial string? GetUnixRegistryVdfFilePath();

    public static KVObject GetDefaultUnixRegistryVdf()
    {
        KVValue val = new KVCollectionValue();
        const string rootName = "Registry";
        return new(rootName, val);
        // 👇 registry.vdf
        //"Registry"
        //{
        //	"HKLM"
        //	{
        //		"Software"
        //		{
        //			"Valve"
        //			{
        //				"Steam"
        //				{
        //					"SteamPID"		"12345"
        //					"TempAppCmdLine"		""
        //					"ReLaunchCmdLine"		""
        //					"ClientLauncherType"		"0"
        //				}
        //			}
        //		}
        //	}
        //	"HKCU"
        //	{
        //		"Software"
        //		{
        //			"Valve"
        //			{
        //				"Steam"
        //				{
        //					"RunningAppID"		"0"
        //					"steamglobal"
        //					{
        //						"language"		"schinese"
        //					}
        //					"language"		"schinese"
        //					"AutoLoginUser"		"qwertyuiop"
        //					"RememberPassword"		"1"
        //					"SourceModInstallPath"		"/Users/{username}/Library/Application Support/Steam/steamapps\\sourcemods"
        //					"Rate"		"12345"
        //					"AlreadyRetriedOfflineMode"		"0"
        //					"StartupMode"		"0"
        //				}
        //			}
        //		}
        //	}
        //}
    }
#endif

    public static partial string? GetAutoLoginUser(bool steamchina = false);

    public static partial void SetAutoLoginUser(string userName, uint? rememberPassword = 1, bool steamchina = false);
}

partial class SteamPathHelper
{
#if !WINDOWS && !(MACCATALYST || MACOS) && !LINUX
    public static partial string? GetSteamProgramPath() => throw new PlatformNotSupportedException("Unsupported platform for SteamPathHelper.");
#endif

#if !WINDOWS && !(MACCATALYST || MACOS) && !LINUX
    public static partial string? GetUnixRegistryVdfFilePath() => throw new PlatformNotSupportedException("Unsupported platform for SteamPathHelper.");
#endif
}
#endif