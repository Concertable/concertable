using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Concertable.E2ETests.Mobile.Support;

public class MobileFixture : IAsyncLifetime
{
    public AppFixture App { get; }
    public Uri AppiumServerUri { get; private set; } = null!;
    public string ApkPath { get; private set; } = null!;
    public string AppPackage { get; private set; } = null!;
    public string AppActivity { get; private set; } = null!;
    public string AvdName { get; private set; } = null!;

    public MobileFixture() => App = new AppFixture();
    public MobileFixture(IMessageSink messageSink) => App = new AppFixture(messageSink);

    public async Task InitializeAsync()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.E2E.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        AppiumServerUri = new Uri(config["Mobile:AppiumServer"]   ?? "http://127.0.0.1:4723/");
        AppPackage      = config["Mobile:AppPackage"]              ?? "com.concertable.app";
        AppActivity     = config["Mobile:AppActivity"]             ?? ".MainActivity";
        AvdName         = config["Mobile:AvdName"]                 ?? "ConcertableTest";

        var apkRelative = config["Mobile:ApkPath"] ?? "TestAssets/concertable-debug.apk";
        ApkPath = Path.IsPathRooted(apkRelative)
            ? apkRelative
            : Path.Combine(AppContext.BaseDirectory, apkRelative);

        if (!File.Exists(ApkPath))
            throw new FileNotFoundException(
                $"APK not found at {ApkPath}. Build it via 'cd app/mobile && npx expo prebuild --platform android && cd android && ./gradlew assembleDebug', " +
                $"then copy app/build/outputs/apk/debug/app-debug.apk to TestAssets/concertable-debug.apk. See Concertable.E2ETests.Mobile/SETUP.md.");

        await App.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await App.DisposeAsync();
    }
}
