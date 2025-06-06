#if WINDOWS
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BD.SteamClient8.Helpers;

partial class SteamPathHelper
{
    static bool TryGetSteamDirPathBySet([NotNullWhen(true)] out string? steamDirPath)
    {
        if (SteamPathHelper_.Paths.HasValue)
        {
            steamDirPath = SteamPathHelper_.Paths.Value.steamDirPath;
            return true;
        }
        else
        {
            steamDirPath = null;
            return false;
        }
    }

    internal static string? GetSteamProgramPathByPathCombine(string steamDirPath)
    {
        var steamExe = Path.Combine(steamDirPath, "steam.exe");
        if (File.Exists(steamExe))
        {
            return steamExe;
        }
        return null;
    }

    public static partial string? GetSteamProgramPath()
    {
        if (SteamPathHelper_.Paths.HasValue)
        {
            return SteamPathHelper_.Paths.Value.steamProgramPath;
        }

        {
            // 计算机\HKEY_CURRENT_USER\Software\Valve\Steam
            using var sk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam", false);
            var steamExe = GetFullPath(sk?.GetValue("SteamExe")?.ToString());
            if (!string.IsNullOrWhiteSpace(steamExe))
            {
                return steamExe;
            }
        }

        string? steamDirPath = GetSteamDirPath();
        if (steamDirPath != null)
        {
            var steamExe = GetSteamProgramPathByPathCombine(steamDirPath);
            return steamExe;
        }

        return null;
    }

    /// <summary>
    /// 赋值覆盖注册表读取行为
    /// </summary>
    /// <param name="steamDirPath"></param>
    /// <param name="steamProgramPath"></param>
    /// <returns></returns>
    public static bool SetSteamDirPath(string steamDirPath, string? steamProgramPath = null)
    {
        if (!string.IsNullOrWhiteSpace(steamDirPath))
        {
            if (Directory.Exists(steamDirPath))
            {
                steamProgramPath ??= GetSteamProgramPathByPathCombine(steamDirPath);
                if (steamProgramPath != null)
                {
                    SteamPathHelper_.Paths = (steamDirPath, steamProgramPath);
                    return true;
                }
            }
        }
        return false;
    }

    public static partial string? GetAutoLoginUser(bool steamchina)
    {
        // 计算机\HKEY_CURRENT_USER\Software\Valve\Steam
        using var sk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam", false);
        var kAutoLoginUser = steamchina ? "AutoLoginUser_steamchina" : "AutoLoginUser";
        var autoLoginUser = sk?.GetValue(kAutoLoginUser)?.ToString();
        return autoLoginUser;
    }

    public static partial void SetAutoLoginUser(string userName, uint? rememberPassword, bool steamchina)
    {
        using var sk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
        if (sk != null)
        {
            var kAutoLoginUser = steamchina ? "AutoLoginUser_steamchina" : "AutoLoginUser";
            sk.SetValue(kAutoLoginUser, userName, RegistryValueKind.String);
            if (rememberPassword.HasValue)
            {
                sk.SetValue("RememberPassword", rememberPassword.Value, RegistryValueKind.DWord);
            }
        }
    }
}

file static class SteamPathHelper_
{
    /// <summary>
    /// 通过调用 <see cref="SteamPathHelper.SetSteamDirPath"/> 赋值覆盖注册表读取行为
    /// </summary>
    internal static (string steamDirPath, string steamProgramPath)? Paths { get; set; }
}
#endif