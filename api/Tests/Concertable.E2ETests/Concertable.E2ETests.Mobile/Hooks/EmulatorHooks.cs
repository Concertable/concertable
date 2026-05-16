using System.Diagnostics;
using Concertable.E2ETests.Mobile.Support;

namespace Concertable.E2ETests.Mobile.Hooks;

[Binding]
public class EmulatorHooks
{
    public static MobileFixture Fixture { get; private set; } = null!;
    private static readonly SemaphoreSlim InitLock = new(1, 1);
    private static Process? emulatorProcess;
    private static Process? appiumProcess;
    private static bool weStartedEmulator;
    private static bool weStartedAppium;

    [BeforeTestRun(Order = 1)]
    public static async Task BeforeTestRun()
    {
        await InitLock.WaitAsync();
        try
        {
            if (Fixture is not null) return;
            Fixture = new MobileFixture();

            await EnsureEmulatorRunningAsync(Fixture.AvdName);
            await EnsureAppiumServerAsync(Fixture.AppiumServerUri);

            await Fixture.InitializeAsync();

            InstallApk(Fixture.ApkPath);
        }
        finally
        {
            InitLock.Release();
        }
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (Fixture is not null)
            await Fixture.DisposeAsync();

        if (weStartedAppium && appiumProcess is { HasExited: false })
        {
            try { appiumProcess.Kill(entireProcessTree: true); }
            catch { /* best-effort */ }
            appiumProcess = null;
        }

        if (weStartedEmulator)
        {
            try { RunAdb("emu kill"); }
            catch { /* best-effort */ }
            emulatorProcess = null;
        }
    }

    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario() =>
        await Fixture.App.ResetAsync();

    private static async Task EnsureEmulatorRunningAsync(string avdName)
    {
        if (HasRunningEmulator()) return;

        var emulatorBin = ResolveEmulatorBinary();
        emulatorProcess = Process.Start(new ProcessStartInfo
        {
            FileName = emulatorBin,
            Arguments = $"-avd {avdName} -no-snapshot-load -no-boot-anim",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
        weStartedEmulator = true;

        var deadline = DateTime.UtcNow.AddMinutes(3);
        while (DateTime.UtcNow < deadline)
        {
            if (HasRunningEmulator() && BootCompleted()) return;
            await Task.Delay(2_000);
        }
        throw new TimeoutException($"Emulator {avdName} did not become ready within 3 minutes.");
    }

    private static async Task EnsureAppiumServerAsync(Uri serverUri)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        try
        {
            var status = await http.GetAsync(new Uri(serverUri, "status"));
            if (status.IsSuccessStatusCode) return;
        }
        catch { /* not running yet */ }

        var appiumCmd = OperatingSystem.IsWindows() ? "appium.cmd" : "appium";
        appiumProcess = Process.Start(new ProcessStartInfo
        {
            FileName = appiumCmd,
            Arguments = $"--port {serverUri.Port} --base-path {serverUri.AbsolutePath.TrimEnd('/')}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
        weStartedAppium = true;

        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var status = await http.GetAsync(new Uri(serverUri, "status"));
                if (status.IsSuccessStatusCode) return;
            }
            catch { }
            await Task.Delay(1_000);
        }
        throw new TimeoutException($"Appium server at {serverUri} did not become ready within 60 seconds.");
    }

    private static void InstallApk(string apkPath) =>
        RunAdb($"install -r \"{apkPath}\"");

    private static bool HasRunningEmulator()
    {
        var output = RunAdb("devices");
        return output.Split('\n').Any(line => line.StartsWith("emulator-") && line.Contains("device"));
    }

    private static bool BootCompleted()
    {
        try
        {
            var prop = RunAdb("shell getprop sys.boot_completed").Trim();
            return prop == "1";
        }
        catch
        {
            return false;
        }
    }

    private static string RunAdb(string args)
    {
        var adb = ResolveAdbBinary();
        var psi = new ProcessStartInfo
        {
            FileName = adb,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEnd();
        p.WaitForExit(30_000);
        return stdout;
    }

    private static string ResolveAdbBinary() =>
        ResolveAndroidBinary("platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");

    private static string ResolveEmulatorBinary() =>
        ResolveAndroidBinary("emulator", OperatingSystem.IsWindows() ? "emulator.exe" : "emulator");

    private static string ResolveAndroidBinary(string folder, string binary)
    {
        var sdkRoot = Environment.GetEnvironmentVariable("ANDROID_HOME")
                   ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        if (sdkRoot is not null)
        {
            var candidate = Path.Combine(sdkRoot, folder, binary);
            if (File.Exists(candidate)) return candidate;
        }
        return binary;
    }
}
