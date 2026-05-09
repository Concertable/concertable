using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

[Binding]
public class PlaywrightHooks
{
    public static UiFixture Fixture { get; private set; } = null!;
    private static int _runnerCount;
    private readonly Browser browser;

    public PlaywrightHooks(Browser browser) => this.browser = browser;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        if (Interlocked.Increment(ref _runnerCount) == 1)
        {
            Fixture = new UiFixture();
            await Fixture.InitializeAsync();
        }
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (Interlocked.Decrement(ref _runnerCount) == 0 && Fixture is not null)
            await Fixture.DisposeAsync();
    }

    [BeforeScenario]
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
