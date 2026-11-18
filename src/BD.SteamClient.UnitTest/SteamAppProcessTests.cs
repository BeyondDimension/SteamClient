using BD.SteamClient.Helpers;

namespace BD.SteamClient.UnitTest;

/// <summary>Offline coverage that never starts Steam or the main application.</summary>
public sealed class SteamAppProcessTests
{
    [TestCase("Steam++.exe")]
    [TestCase("Steam++")]
    [TestCase("Watt Toolkit")]
    public void NativeAndCustomHostsLaunchDirectly(string executableName)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Watt Toolkit package");
        var executable = Path.Combine(directory, executableName);
        var startInfo = SteamAppProcessLauncher.CreateStartInfo(480, SteamAppRunType.Idle, executable, null, directory);

        Assert.That(startInfo.FileName, Is.EqualTo(executable));
        Assert.That(startInfo.ArgumentList, Is.EqualTo(new[] { "-clt", "app", "-silence", "-id", "480" }));
        Assert.That(startInfo.WorkingDirectory, Is.EqualTo(directory));
        Assert.That(startInfo.UseShellExecute, Is.False);
        Assert.That(startInfo.Environment["SteamAppId"], Is.EqualTo("480"));
    }

    [TestCase("dotnet")]
    [TestCase("dotnet.exe")]
    public void DotnetHostIncludesTheEntryDllAsOneArgument(string executableName)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Watt Toolkit development");
        var executable = Path.Combine(directory, "runtime", executableName);
        var entryDll = Path.Combine(directory, "assemblies", "Steam++.dll");
        var startInfo = SteamAppProcessLauncher.CreateStartInfo(480, SteamAppRunType.Idle, executable, entryDll, directory);

        Assert.That(startInfo.FileName, Is.EqualTo(executable));
        Assert.That(startInfo.ArgumentList, Is.EqualTo(new[] { entryDll, "-clt", "app", "-silence", "-id", "480" }));
        Assert.That(startInfo.WorkingDirectory, Is.EqualTo(Path.GetDirectoryName(entryDll)));
    }

    [TestCase(null)]
    [TestCase("")]
    public void DotnetHostWithoutEntryDllCannotStartAnUnrelatedCommand(string? entryDll)
    {
        Assert.Throws<InvalidOperationException>(() => SteamAppProcessLauncher.CreateStartInfo(480, SteamAppRunType.Idle, "dotnet", entryDll, Path.GetTempPath()));
    }

    [TestCase(SteamAppRunType.Idle, "-silence")]
    [TestCase(SteamAppRunType.UnlockAchievement, "-achievement")]
    [TestCase(SteamAppRunType.CloudManager, "-cloudmanager")]
    public void AllModesPreserveTheirFlagsAndInvariantSteamAppId(SteamAppRunType runType, string modeArgument)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var startInfo = SteamAppProcessLauncher.CreateStartInfo(uint.MaxValue, runType, "Steam++", null, Path.GetTempPath());

            Assert.That(startInfo.ArgumentList, Is.EqualTo(new[] { "-clt", "app", modeArgument, "-id", "4294967295" }));
            Assert.That(startInfo.Environment["SteamAppId"], Is.EqualTo("4294967295"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Test]
    public void ManagedLinuxScriptRunsAsTrackableBashChildWithSteamEnvironment()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Watt Toolkit package");
        var script = Path.Combine(directory, "Steam++.sh");
        var startInfo = SteamAppProcessLauncher.CreateStartInfo(480, SteamAppRunType.CloudManager, "/usr/bin/dotnet", null, directory, script);

        Assert.That(startInfo.FileName, Is.EqualTo("/bin/bash"));
        Assert.That(startInfo.ArgumentList, Is.EqualTo(new[] { script, "-clt", "app", "-cloudmanager", "-id", "480" }));
        Assert.That(startInfo.WorkingDirectory, Is.EqualTo(directory));
        Assert.That(startInfo.UseShellExecute, Is.False);
        Assert.That(startInfo.Environment["SteamAppId"], Is.EqualTo("480"));
    }

    [Test]
    public void LinuxLauncherIsOptionalAndFoundInExistingManagedLayouts()
    {
        var directory = Path.Combine(Path.GetTempPath(), "wtts-launcher-" + Guid.NewGuid().ToString("N"));
        var assembliesDirectory = Path.Combine(directory, "assemblies");
        Directory.CreateDirectory(assembliesDirectory);
        try
        {
            Assert.That(SteamAppProcessLauncher.FindLinuxLauncher(directory), Is.Null);
            Assert.That(SteamAppProcessLauncher.FindLinuxLauncher(assembliesDirectory), Is.Null);
            var rootScript = Path.Combine(directory, "Steam++.sh");
            File.WriteAllText(rootScript, "#!/bin/bash\n");
            Assert.That(SteamAppProcessLauncher.FindLinuxLauncher(directory), Is.EqualTo(rootScript));
            Assert.That(SteamAppProcessLauncher.FindLinuxLauncher(assembliesDirectory), Is.EqualTo(rootScript));

            var localScript = Path.Combine(assembliesDirectory, "Steam++.sh");
            File.WriteAllText(localScript, "#!/bin/bash\n");
            Assert.That(SteamAppProcessLauncher.FindLinuxLauncher(assembliesDirectory), Is.EqualTo(localScript));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public async Task ConcurrentIdleStartsReturnTheExistingProcessWithoutLaunchingChildren()
    {
        using var process = Process.GetCurrentProcess();
        var app = new SteamApp(480) { Process = process };
        try
        {
            var results = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() => app.StartSteamAppProcess())));

            foreach (var result in results)
                Assert.That(result, Is.SameAs(process));
            Assert.That(app.Process, Is.SameAs(process));
        }
        finally
        {
            // This process is borrowed only for the liveness test; never invoke stop/toggle on it.
            app.Process = null;
        }
    }

    [Test]
    public void RepeatedStopWithoutOwnedProcessIsANoOp()
    {
        var app = new SteamApp(480);
        app.StopSteamAppProcess();
        app.StopSteamAppProcess();
        Assert.That(app.Process, Is.Null);
    }

    [Test]
    public async Task ConcurrentStopsTerminateOnlyTheOwnedChildAndReleaseItsHandle()
    {
        var process = StartTestChild(waitForInput: true);
        using var observer = Process.GetProcessById(process.Id);
        var app = new SteamApp(480) { Process = process };
        try
        {
            await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(app.StopSteamAppProcess)));

            Assert.That(app.Process, Is.Null);
            Assert.That(observer.WaitForExit(5000), Is.True);
            Assert.Throws<InvalidOperationException>(() => _ = process.HasExited);
        }
        finally
        {
            if (!observer.HasExited)
            {
                observer.Kill(entireProcessTree: true);
                observer.WaitForExit();
            }
            process.Dispose();
        }
    }

    [Test]
    public void StopReleasesAnAlreadyExitedChild()
    {
        using var process = StartTestChild(waitForInput: false);
        Assert.That(process.WaitForExit(5000), Is.True);
        var app = new SteamApp(480) { Process = process };

        app.StopSteamAppProcess();

        Assert.That(app.Process, Is.Null);
        Assert.Throws<InvalidOperationException>(() => _ = process.HasExited);
    }

    static Process StartTestChild(bool waitForInput)
    {
        var startInfo = new ProcessStartInfo(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh")
        {
            UseShellExecute = false,
            RedirectStandardInput = waitForInput,
            CreateNoWindow = true,
        };
        if (OperatingSystem.IsWindows())
        {
            startInfo.ArgumentList.Add("/d");
            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add(waitForInput ? "set /p WTTS_TEST_WAIT=" : "exit /b 0");
        }
        else
        {
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add(waitForInput ? "read WTTS_TEST_WAIT" : "exit 0");
        }
        return Process.Start(startInfo) ?? throw new InvalidOperationException("The isolated test child could not be started.");
    }
}
