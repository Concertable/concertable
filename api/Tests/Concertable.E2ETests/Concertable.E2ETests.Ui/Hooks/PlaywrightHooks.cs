using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

[Binding]
public class PlaywrightHooks
{
    private static readonly string[] KnownRoles = ["VenueManager", "ArtistManager", "Customer"];

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

        var role = scenarioContext.ScenarioInfo.Tags.FirstOrDefault(KnownRoles.Contains);
        var storageState = role is null ? null : LoginCaptureHooks.GetStorageState(role);

        await browser.InitializeAsync(fixture.Browser, storageState);
    }

    [AfterScenario]
    public Task AfterScenario() => browser.DisposeAsync().AsTask();
}
