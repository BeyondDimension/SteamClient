using BD.SteamClient8.Enums.WebApi.SteamApps;
using System.Diagnostics;
using System.Extensions;

namespace BD.SteamClient8.Models.WebApi.SteamApps;

partial class SteamApp
{
    const SteamAppRunType DefaultSteamAppRunType = SteamAppRunType.Idle;

    public interface IMobiusClientAppRunService
    {
        static IMobiusClientAppRunService? Instance => Ioc.Get_Nullable<IMobiusClientAppRunService>();

        Process? StartSteamAppProcess(SteamApp app, SteamAppRunType runType = DefaultSteamAppRunType);

        void RunOrStopSteamAppProcess(SteamApp app, SteamAppRunType runType = DefaultSteamAppRunType);
    }

    public Process? StartSteamAppProcess(SteamAppRunType runType = DefaultSteamAppRunType)
    {
        var service = IMobiusClientAppRunService.Instance;
        if (service != null)
        {
            var proc = service.StartSteamAppProcess(this, runType);
            return Process = proc;
        }
        else
        {
            var proc = DefaultStartSteamAppProcess(runType);
            return Process = proc;
        }
    }

    Process? DefaultStartSteamAppProcess(SteamAppRunType runType)
    {
#if IOS || TVOS || MACCATALYST
        return null;
#else
        var arg = runType switch
        {
            SteamAppRunType.UnlockAchievement => "-achievement",
            SteamAppRunType.CloudManager => "-cloudmanager",
            _ => "-silence",
        };
        string arguments = $"-clt app {arg} -id {AppId}";
        var processPath = Environment.ProcessPath;
        processPath.ThrowIsNull();
#if !WINDOWS
        if (OperatingSystem.IsWindows())
#endif
        {
            var proc = Process2.Start(processPath, arguments);
            return proc;
        }
        {
#if !(WINDOWS || MACOS || ANDROID)
#if !LINUX
            if (OperatingSystem.IsLinux())
#endif
            {
                var psi = new ProcessStartInfo
                {
                    Arguments = arguments,
                    FileName = Path.Combine(AppContext.BaseDirectory, "Steam++.sh"),
                    UseShellExecute = true,
                };
                psi.Environment.Add("SteamAppId", AppId.ToString());
                var proc = Process.Start(psi);
                return proc;
            }
#endif
#if !LINUX
            {
                var proc = Process2.Start(
                    processPath,
                    arguments,
                    environment: new Dictionary<string, string>() {
                        {
                            "SteamAppId",
                            AppId.ToString()
                        }
               });
                return proc;
            }
#endif
        }
#endif
    }

    public void RunOrStopSteamAppProcess(SteamAppRunType runType)
    {
        var service = IMobiusClientAppRunService.Instance;
        if (service != null)
        {
            service.RunOrStopSteamAppProcess(this, runType);
            return;
        }
        else
        {
#if IOS || TVOS || MACCATALYST
            return;
#else
            if (Process != null && !Process.HasExited)
            {
                Process.KillEntireProcessTree();
                Process = null;
            }
            else
            {
                StartSteamAppProcess(runType);
            }
#endif
        }
    }

    public void RunOrStopSteamAppProcess() => RunOrStopSteamAppProcess(DefaultSteamAppRunType);
}
