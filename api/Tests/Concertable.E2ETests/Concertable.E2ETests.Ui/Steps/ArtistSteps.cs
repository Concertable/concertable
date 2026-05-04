using System.Text.Json;
using Concertable.E2ETests.Ui.PageObjects;
using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Steps;

[Binding]
public class ArtistSteps
{
    private readonly UiFixture fixture;
    private readonly Browser browser;
    private readonly WorkflowState state;
    private ArtistFindVenuePage venuePage = null!;

    public ArtistSteps(UiFixture fixture, Browser browser, WorkflowState state)
    {
        this.fixture = fixture;
        this.browser = browser;
        this.state = state;
    }

    [When(@"the artist applies to the opportunity")]
    public async Task ArtistApplies()
    {
        await browser.UseRoleAsync(Role.ArtistManager);

        venuePage = new ArtistFindVenuePage(browser.Page, fixture.App.SpaBaseUrl);
        await venuePage.GotoAsync(state.VenueId);
        await venuePage.ApplyAsync(state.OpportunityId);
        await venuePage.WaitUntilAppliedAsync(state.OpportunityId);

        state.ApplicationId = await FetchNewestApplicationIdAsync(state.OpportunityId);
    }

    [Then(@"the application is created")]
    public Task ApplicationIsCreated() => venuePage.WaitUntilAppliedAsync(state.OpportunityId);

    private async Task<int> FetchNewestApplicationIdAsync(int opportunityId)
    {
        var seed = fixture.App.SeedData;
        using var client = fixture.App.CreateAuthenticatedClient(seed.VenueManager1.Id, Role.VenueManager);
        var json = await client.GetStringAsync($"/api/application/opportunity/{opportunityId}");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(a => a.GetProperty("id").GetInt32())
            .Max();
    }
}
