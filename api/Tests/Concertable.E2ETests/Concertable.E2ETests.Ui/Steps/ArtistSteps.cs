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

        var applied = browser.Page.WaitForResponseAsync($"**/api/application/{state.OpportunityId}");
        await venuePage.ApplyAsync(state.OpportunityId);
        state.ApplicationId = await ReadApplicationIdAsync(await applied);

        await venuePage.WaitUntilAppliedAsync(state.OpportunityId);
    }

    [When(@"the artist applies to the venue hire opportunity with a valid card")]
    public async Task ArtistAppliesToVenueHireWithValidCard()
    {
        await browser.UseRoleAsync(Role.ArtistManager);

        venuePage = new ArtistVenueDetailsPage(browser.Page, fixture.App.SpaBaseUrl);
        await venuePage.GotoAsync(state.VenueId);
        await venuePage.ApplyAsync(state.OpportunityId);

        var applyCheckoutPage = new ApplyCheckoutPage(browser.Page);
        var applied = browser.Page.WaitForResponseAsync($"**/api/application/{state.OpportunityId}");
        await applyCheckoutPage.PayWithSavedCardAsync();
        state.ApplicationId = await ReadApplicationIdAsync(await applied);
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
        var applied = browser.Page.WaitForResponseAsync($"**/api/application/{state.OpportunityId}");
        await applyCheckoutPage.PayWithNewCardAsync(StripeCards.Success);
        state.ApplicationId = await ReadApplicationIdAsync(await applied);
    }

    [Then(@"the application is created")]
    public Task ApplicationIsCreated() => venuePage.WaitUntilAppliedAsync(state.OpportunityId);

    private static async Task<int> ReadApplicationIdAsync(IResponse response)
    {
        var json = await response.JsonAsync();
        return json!.Value.GetProperty("id").GetInt32();
    }
}
