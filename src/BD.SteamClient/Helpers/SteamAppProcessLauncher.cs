#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BD.SteamClient.Helpers;

/// <summary>Resolves the current app host without routing child processes through a desktop opener.</summary>
internal static class SteamAppProcessLauncher
{
    const int ErrorElevationRequired = 740;

    [UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "Only the dotnet muxer needs the entry DLL; apphost, single-file and NativeAOT launches use the executable directly.")]
    internal static Process? Start(uint appId, SteamAppRunType runType)
    {
        var processPath = Environment.ProcessPath;
        processPath.ThrowIsNull();
        var launcherPath = OperatingSystem.IsLinux() && RuntimeFeature.IsDynamicCodeSupported
            ? FindLinuxLauncher(AppContext.BaseDirectory)
            : null;
        var entryAssemblyPath = launcherPath == null && IsDotNetHost(processPath)
            ? Assembly.GetEntryAssembly()?.Location
            : null;
        var startInfo = CreateStartInfo(appId, runType, processPath, entryAssemblyPath, AppContext.BaseDirectory, launcherPath);
        try
        {
            return Process.Start(startInfo);
        }
        catch (Win32Exception ex) when (OperatingSystem.IsWindows() && ex.NativeErrorCode == ErrorElevationRequired)
        {
            // Release apphosts may require elevation. ShellExecute cannot override the child's
            // environment; the app command also sets SteamAppId from its explicit -id argument.
            var elevatedStartInfo = new ProcessStartInfo(startInfo.FileName)
            {
                WorkingDirectory = startInfo.WorkingDirectory,
                UseShellExecute = true,
                Verb = "runas",
            };
            foreach (var argument in startInfo.ArgumentList)
                elevatedStartInfo.ArgumentList.Add(argument);
            return Process.Start(elevatedStartInfo);
        }
    }

    internal static ProcessStartInfo CreateStartInfo(uint appId, SteamAppRunType runType, string processPath, string? entryAssemblyPath, string baseDirectory, string? linuxLauncherPath = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = linuxLauncherPath == null ? processPath : "/bin/bash",
            WorkingDirectory = Path.GetDirectoryName(linuxLauncherPath ?? processPath) ?? baseDirectory,
            UseShellExecute = false,
        };
        if (linuxLauncherPath != null)
        {
            // Run the existing managed-layout script as a real, trackable child and preserve
            // its bundled-runtime setup even when the script has no executable permission.
            startInfo.ArgumentList.Add(linuxLauncherPath);
        }
        else if (IsDotNetHost(processPath))
        {
            if (string.IsNullOrEmpty(entryAssemblyPath))
                throw new InvalidOperationException("The dotnet host requires the managed entry assembly path.");
            startInfo.ArgumentList.Add(entryAssemblyPath);
            startInfo.WorkingDirectory = Path.GetDirectoryName(entryAssemblyPath) ?? baseDirectory;
        }

        var appIdString = appId.ToString(CultureInfo.InvariantCulture);
        startInfo.ArgumentList.Add("-clt");
        startInfo.ArgumentList.Add("app");
        startInfo.ArgumentList.Add(runType switch
        {
            SteamAppRunType.UnlockAchievement => "-achievement",
            SteamAppRunType.CloudManager => "-cloudmanager",
            _ => "-silence",
        });
        startInfo.ArgumentList.Add("-id");
        startInfo.ArgumentList.Add(appIdString);
        startInfo.Environment["SteamAppId"] = appIdString;
        return startInfo;
    }

    internal static string? FindLinuxLauncher(string baseDirectory)
    {
        var launcherPath = Path.Combine(baseDirectory, "Steam++.sh");
        if (File.Exists(launcherPath))
            return launcherPath;

        // Managed Linux packages put the entry DLL in assemblies/ and the script one level up.
        var directory = new DirectoryInfo(baseDirectory);
        if (directory.Name == "assemblies" && directory.Parent != null)
        {
            launcherPath = Path.Combine(directory.Parent.FullName, "Steam++.sh");
            if (File.Exists(launcherPath))
                return launcherPath;
        }
        return null;
    }

    static bool IsDotNetHost(string processPath)
    {
        var fileName = Path.GetFileName(processPath);
        return fileName.Equals("dotnet", StringComparison.OrdinalIgnoreCase)
            || fileName.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase);
    }
}
#endif
