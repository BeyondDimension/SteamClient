#if !(IOS || ANDROID)
#if WINDOWS
using System.Management;
using System.ComponentModel;
#endif
using System.Diagnostics;
using System.Extensions;

namespace BD.SteamClient8.Helpers;

public static partial class SteamProcHelper
{
    /// <summary>
    /// Steam 主进程名称
    /// </summary>
    public const string SteamMainProcessName =
#if MACCATALYST || MACOS
        "steam_osx";
#else
        "steam";
#endif

    /// <summary>
    /// Steam 相关进程名称列表
    /// </summary>
    public static string[] GetSteamProcessNames() => LazySteamProcessNames.V;

    public static void KillSteamProcesses(string[] processNames)
    {
        var processes = processNames.Select(static x =>
        {
            try
            {
                var process = Process.GetProcessesByName(x);
                return process;
            }
            catch
            {
                return [];
            }
        }).SelectMany(static x => x).ToArray();

        if (processes.Length > 0)
        {
            Parallel.ForEach(processes, static process =>
            {
                if (process == null)
                {
                    return;
                }
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(SteamProcHelper), ex,
                        $"KillSteamProcess fail, name: {process.ProcessName}, pid: {process.Id}");
                }
            });
        }
    }

    public static void KillSteamProcessesByCommandShutdown(string? steamProgramPath)
    {
        if (!string.IsNullOrWhiteSpace(steamProgramPath) && File.Exists(steamProgramPath))
        {
            ProcessStartInfo psi = new(steamProgramPath, "-shutdown")
            {
                UseShellExecute = true,
            };
            var shutdownProc = Process.Start(psi);
            if (shutdownProc != null)
            {
                if (!shutdownProc.WaitForExit(TimeSpan.FromSeconds(45)))
                {
                    shutdownProc.Kill(true);
                }
            }
        }
    }

#if WINDOWS
    public static bool IsSteamChinaLauncherByProcCommandLine(string steamMainProcessName)
    {
        var processes = Process.GetProcessesByName(SteamMainProcessName);
        var result = processes.Select(IsSteamChinaLauncherByProcCommandLine)
           .Any(static x => x);
        return result;
    }

    public static bool IsSteamChinaLauncherByProcCommandLine(Process process)
    {
        try
        {
            var commandLineArgs = GetCommandLineArgsCore(process);
            return commandLineArgs.Contains("-steamchina", StringComparison.OrdinalIgnoreCase);
        }
        catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
        {
            // 没有对该进程的安全访问权限。
            return false;
        }
        catch (InvalidOperationException)
        {
            // 进程已退出。
            return false;
        }
    }
#endif
}

file static class LazySteamProcessNames
{
    internal static readonly string[] V = [
        SteamProcHelper.SteamMainProcessName,
        "steamservice",
        "steamwebhelper",
        "GameOverlayUI",
    ];
}

#if WINDOWS
partial class SteamProcHelper
{
    static string GetCommandLineArgsCore(Process process)
    {
        using (var searcher = new ManagementObjectSearcher(
            "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
        using (var objects = searcher.Get())
        {
            var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
            return @object?["CommandLine"]?.ToString() ?? "";
        }
    }
}
#endif
#endif