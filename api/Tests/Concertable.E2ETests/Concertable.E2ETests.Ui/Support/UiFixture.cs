using Xunit.Abstractions;

namespace Concertable.E2ETests.Ui.Support;

public class UiFixture : IAsyncLifetime
{
    private IPlaywright playwright = null!;

    public AppFixture App { get; }
    public IBrowser Browser { get; private set; } = null!;

    public UiFixture() => App = new AppFixture();

    public UiFixture(IMessageSink messageSink) => App = new AppFixture(messageSink);

    public async Task InitializeAsync()
    {
        await App.InitializeAsync();
        playwright = await Playwright.CreateAsync();
        Browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = Environment.GetEnvironmentVariable("CI") == "true",
            SlowMo = Environment.GetEnvironmentVariable("CI") == "true" ? 0 : 250
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        playwright.Dispose();
        await App.DisposeAsync();
    }
}
