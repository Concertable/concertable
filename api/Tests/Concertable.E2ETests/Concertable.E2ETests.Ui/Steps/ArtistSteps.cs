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
    private ArtistVenueDetailsPage venuePage = null!;

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

        venuePage = new ArtistVenueDetailsPage(browser.Page, fixture.App.SpaBaseUrl);
        await venuePage.GotoAsync(state.VenueId);
        await venuePage.ApplyAsync(state.OpportunityId);
        await venuePage.WaitUntilAppliedAsync(state.OpportunityId);

        state.ApplicationId = await FetchNewestApplicationIdAsync(state.OpportunityId);
    }

    [When(@"the artist applies to the venue hire opportunity with a valid card")]
    public async Task ArtistAppliesToVenueHireWithValidCard()
    {
        await browser.UseRoleAsync(Role.ArtistManager);

        venuePage = new ArtistVenueDetailsPage(browser.Page, fixture.App.SpaBaseUrl);
        await venuePage.GotoAsync(state.VenueId);
        await venuePage.ApplyAsync(state.OpportunityId);

        var applyCheckoutPage = new ApplyCheckoutPage(browser.Page);
        var applied = browser.Page.WaitForResponseAsync($"**/application/{state.OpportunityId}");
        await applyCheckoutPage.PayWithSavedCardAsync();
        await applied;

        state.ApplicationId = await FetchNewestApplicationIdAsync(state.OpportunityId);
    }

    [Given(@"a venue hire opportunity is open for application")]
    public async Task AVenueHireOpportunityIsOpen()
    {
        await browser.UseRoleAsync(Role.ArtistManager);

        var opp = fixture.App.SeedData.FreshVenueHireOpportunity;
        state.VenueId = opp.VenueId;
        state.OpportunityId = opp.OpportunityId;

        await browser.Page.GotoAsync($"{fixture.App.SpaBaseUrl}/artist/opportunity/checkout/{state.OpportunityId}");
    }

    [When(@"the artist pays the venue hire fee with a new card")]
    public async Task PaysVenueHireFeeWithNewCard()
    {
        var applyCheckoutPage = new ApplyCheckoutPage(browser.Page);
        var applied = browser.Page.WaitForResponseAsync($"**/application/{state.OpportunityId}");
        await applyCheckoutPage.PayWithNewCardAsync(StripeCards.Success);
        await applied;

        state.ApplicationId = await FetchNewestApplicationIdAsync(state.OpportunityId);
    }

    [Then(@"the application is created")]
    public Task ApplicationIsCreated() => venuePage.WaitUntilAppliedAsync(state.OpportunityId);

    private async Task<int> FetchNewestApplicationIdAsync(int opportunityId)
    {
        var seed = fixture.App.SeedData;
        using var client = await fixture.App.CreateAuthenticatedClientAsync(seed.VenueManager1.Email);
        var json = await client.GetStringAsync($"/api/application/opportunity/{opportunityId}");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(a => a.GetProperty("id").GetInt32())
            .Max();
    }
}
