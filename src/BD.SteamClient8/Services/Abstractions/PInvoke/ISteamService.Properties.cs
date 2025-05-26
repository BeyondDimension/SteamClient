using Microsoft.Win32;
using System.Extensions;
using System.Runtime.CompilerServices;
using System.Text;

namespace BD.SteamClient8.Services.Abstractions.PInvoke;

partial interface ISteamService
{
#if !(IOS || ANDROID)

#if WINDOWS
    /// <summary>
    /// 在 Windows 平台中修正从注册表读取的路径大小写与路径分隔符
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string? GetFullPath(string s)
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
                        if (item.HasLowerLetter())
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

    const string SteamRegistryPath = @"SOFTWARE\Valve\Steam";
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetSteamDirPath()
    {
#if WINDOWS
        return GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamPath"));
#elif MACCATALYST || MACOS
        return $"/Users/{Environment.UserName}/Library/Application Support/Steam";
#elif LINUX
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.steam/steam";
#else
        return null;
#endif
    }

    private static Lazy<string?> _SteamDirPath = new(GetSteamDirPath, true);

    /// <summary>
    /// Steam 文件夹目录
    /// </summary>
    static string? SteamDirPath => _SteamDirPath.Value;

    /// <summary>
    /// 获取 Steam 动态链接库 (DLL) 文件夹目录
    /// </summary>
    /// <returns></returns>
    static string? GetSteamDynamicLinkLibraryPath()
    {
#if MACCATALYST || MACOS
        return $"/Users/{Environment.UserName}/Library/Application Support/Steam/Steam.AppBundle/Steam/Contents/MacOS";
#else
        return _SteamDirPath.Value;
#endif
    }

    private static string? GetSteamProgramPath()
    {
#if WINDOWS
        return GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamExe"));
#elif MACCATALYST || MACOS
        return "/Applications/Steam.app/Contents/MacOS/steam_osx";
#elif LINUX
        return "/usr/bin/steam";
#else
        return null;
#endif
    }

    private static Lazy<string?> _SteamProgramPath = new(GetSteamProgramPath, true);

    /// <summary>
    /// Steam 程序的路径
    /// </summary>
    static string? SteamProgramPath => _SteamProgramPath.Value;

    private static string? GetRegistryVdfPath()
    {
#if MACCATALYST || MACOS
        return $"/Users/{Environment.UserName}/Library/Application Support/Steam/registry.vdf";
#elif LINUX
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.steam/registry.vdf";
#else
        return null;
#endif
    }

    private static Lazy<string?> _RegistryVdfPath = new(GetRegistryVdfPath, true);

    /// <summary>
    /// 注册表信息文件的路径
    /// </summary>
    static string? RegistryVdfPath => _RegistryVdfPath.Value;

#endif
}