#if WINDOWS || (PROJ_3RD_PARTY_SAM_API && !LINUX && !MACOS)
using Microsoft.Win32;
using System.Text;

namespace
#if PROJ_3RD_PARTY_SAM_API
    SAM.API;
#else
    BD.SteamClient8.Helpers;
#endif

#if PROJ_3RD_PARTY_SAM_API
partial class Helpers
#else
partial class SteamPathHelper
#endif
{
#if PROJ_3RD_PARTY_SAM_API && !WINDOWS
    static string? GetSteamDirPathByWin()
#else
    public static partial string? GetSteamDirPath()
#endif
    {
#if !PROJ_3RD_PARTY_SAM_API
        // 指定固定路径值替换注册表读取实现
        // 在引用 BD.SteamClient8 中通过 SetSteamDirPath 函数
        // 在仅引用 SAM.API 中通过 GetInstallPathDelegate 委托
        if (TryGetSteamDirPathBySet(out var steamDirPathBySet))
        {
            return steamDirPathBySet;
        }
#endif

        // c:/program files (x86)/steam -> GetFullPath = C:\Program Files (x86)\Steam

#pragma warning disable CA1416 // 验证平台兼容性
        {
            // 计算机\HKEY_CURRENT_USER\Software\Valve\Steam
            using var sk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam", false);
            var steamPath = GetFullPath(sk?.GetValue("SteamPath")?.ToString());
            if (!string.IsNullOrWhiteSpace(steamPath))
            {
                return steamPath;
            }
        }

        {
            // 计算机\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam
            using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry32 : RegistryView.Default);
            using var sk = hkcu.OpenSubKey(@"SOFTWARE\Valve\Steam", false);
            var installPath = GetFullPath(sk?.GetValue("InstallPath")?.ToString());
            if (!string.IsNullOrWhiteSpace(installPath))
            {
                return installPath;
            }
        }
#pragma warning restore CA1416 // 验证平台兼容性

        return null;
    }

#if PROJ_3RD_PARTY_SAM_API && !WINDOWS
    static string? GetSteamClientNativeLibraryPathByWin()
#else
    public static partial string? GetSteamClientNativeLibraryPath()
#endif
    {
        string? steamDirPath = GetSteamDirPath();
        if (steamDirPath == null)
        {
            return null;
        }

        // C:\Program Files (x86)\Steam\steamclient64.dll
        var libraryPath = Path.Combine(steamDirPath,
            Environment.Is64BitProcess ?
                "steamclient64.dll" :
                "steamclient.dll");
        return libraryPath;
    }
}

#if PROJ_3RD_PARTY_SAM_API
partial class Helpers // Windows Utils
#else
partial class SteamPathHelper // Windows Utils
#endif
{
    /// <summary>
    /// 在 Windows 平台中修正从注册表读取的路径大小写与路径分隔符
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string? GetFullPath(string? s)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            try
            {
                var path_splits = s.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries); // 按分隔符切割数组
                StringBuilder builder = new();
                bool pathExists = true;
                for (int i = 0; i < path_splits.Length; i++) // 循环路径数组
                {
                    var item = path_splits[i];
                    var is_first = i == 0;
                    var is_last = !is_first && (i == path_splits.Length - 1);
                    if (is_first) // 修正盘符
                    {
                        if (item.Any(x => x >= 'a' && x <= 'z'))
                        {
                            item = item.ToUpperInvariant(); // 盘符大写
                        }
                    }
                    else if (pathExists)
                    {
                        try
                        {
                            var path = builder.ToString();
                            var dirInfo = new DirectoryInfo(path);
                            pathExists = dirInfo.Exists;
                            if (pathExists)
                            {
                                IEnumerable<FileSystemInfo> infos = is_last ? dirInfo.EnumerateFileSystemInfos() : dirInfo.EnumerateDirectories();
                                foreach (var info in infos)
                                {
                                    if (string.Equals(info.Name,
                                        item, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // 修正文件名或文件夹名
                                        item = info.Name;
                                        break;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            pathExists = false;
                        }
                    }

                    builder.Append(item);
                    if (!is_last) builder.Append(Path.DirectorySeparatorChar);
                }
                return builder.ToString();
            }
            catch
            {
                return Path.GetFullPath(s);
            }
        }
        return null;
    }
}
#endif