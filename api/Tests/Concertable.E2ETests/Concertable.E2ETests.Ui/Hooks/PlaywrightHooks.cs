using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

[Binding]
public class PlaywrightHooks
{
    public static UiFixture Fixture { get; private set; } = null!;
    private static readonly SemaphoreSlim InitLock = new(1, 1);
    private readonly Browser browser;

    public PlaywrightHooks(Browser browser) => this.browser = browser;

    [BeforeTestRun(Order = 1)]
    public static async Task BeforeTestRun()
    {
        await InitLock.WaitAsync();
        try
        {
            if (Fixture is not null) return;
            Fixture = new UiFixture();
            await Fixture.InitializeAsync();
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
    }

    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario(ScenarioContext scenarioContext)
    {
        await Fixture.App.ResetAsync();
        LoginCaptureHooks.Reset();

        var role = scenarioContext.ScenarioInfo.Tags
            .Select(tag => Enum.TryParse<Role>(tag, out var r) ? (Role?)r : null)
            .FirstOrDefault(r => r is not null);

        await browser.InitializeAsync(Fixture.Browser, role, Fixture);
    }

    [AfterScenario]
    public Task AfterScenario() => browser.DisposeAsync().AsTask();
}
