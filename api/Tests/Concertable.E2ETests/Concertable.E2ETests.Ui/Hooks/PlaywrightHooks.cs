using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

[Binding]
public class PlaywrightHooks
{
    public static UiFixture Fixture { get; private set; } = null!;
    private readonly Browser browser;

    public PlaywrightHooks(Browser browser) => this.browser = browser;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        Fixture = new UiFixture();
        await Fixture.InitializeAsync();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (Fixture is not null)
            await Fixture.DisposeAsync();
    }

    [BeforeScenario]
    public async Task BeforeScenario(ScenarioContext scenarioContext)
    {
        await Fixture.App.ResetAsync();
        await LoginCaptureHooks.CaptureAllAsync(Fixture);

        var role = scenarioContext.ScenarioInfo.Tags
            .Select(tag => Enum.TryParse<Role>(tag, out var r) ? (Role?)r : null)
            .FirstOrDefault(r => r is not null);

        await browser.InitializeAsync(Fixture.Browser, role);
    }

    [AfterScenario]
    public Task AfterScenario() => browser.DisposeAsync().AsTask();
}
