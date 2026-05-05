using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

[Binding]
public class PlaywrightHooks
{
    private readonly UiFixture fixture;
    private readonly Browser browser;

    public PlaywrightHooks(UiFixture fixture, Browser browser)
    {
        this.fixture = fixture;
        this.browser = browser;
    }

    [BeforeScenario]
    public async Task BeforeScenario(ScenarioContext scenarioContext)
    {
        await fixture.App.ResetAsync();
        await LoginCaptureHooks.CaptureAllAsync(fixture);

        var role = scenarioContext.ScenarioInfo.Tags
            .Select(tag => Enum.TryParse<Role>(tag, out var r) ? (Role?)r : null)
            .FirstOrDefault(r => r is not null);

        await browser.InitializeAsync(fixture.Browser, role);
    }

    [AfterScenario]
    public Task AfterScenario() => browser.DisposeAsync().AsTask();
}
