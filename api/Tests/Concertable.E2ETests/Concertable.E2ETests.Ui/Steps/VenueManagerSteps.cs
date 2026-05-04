using System.Text.Json;
using Concertable.E2ETests.Ui.PageObjects;
using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Steps;

[Binding]
public class VenueManagerSteps
{
    private readonly UiFixture fixture;
    private readonly Browser browser;
    private readonly WorkflowState state;
    private MyVenuePage myVenuePage = null!;
    private VenueApplicationCheckoutPage checkoutPage = null!;

    public VenueManagerSteps(UiFixture fixture, Browser browser, WorkflowState state)
    {
        this.fixture = fixture;
        this.browser = browser;
        this.state = state;
    }

    [When(@"the venue manager posts a flat fee opportunity for £(\d+)")]
    public async Task PostsFlatFeeOpportunity(decimal fee)
    {
        await browser.UseRoleAsync(Role.VenueManager);

        state.VenueId = fixture.App.SeedData.VenueManager1.VenueId;

        myVenuePage = new MyVenuePage(browser.Page, fixture.App.SpaBaseUrl);
        await myVenuePage.GotoAsync();
        await myVenuePage.PostFlatFeeOpportunityAsync(fee);
        await myVenuePage.WaitUntilSavedAsync();

        state.OpportunityId = await FetchNewestOpportunityIdAsync(state.VenueId);
    }

    [When(@"the venue manager accepts the application with a valid card")]
    public async Task AcceptsWithValidCard()
    {
        await browser.UseRoleAsync(Role.VenueManager);

        var applicationsPage = new VenueApplicationsPage(browser.Page, fixture.App.SpaBaseUrl);
        await applicationsPage.GotoAsync(state.OpportunityId);
        await applicationsPage.ClickAcceptAsync(state.ApplicationId);

        var acceptPage = new VenueAcceptApplicationPage(browser.Page);
        await acceptPage.ClickConfirmAsync();

        checkoutPage = new VenueApplicationCheckoutPage(browser.Page);
        await checkoutPage.PayWithCardAsync(StripeCards.Success);
    }

    [Then(@"a draft concert is created")]
    public Task DraftConcertCreated() => checkoutPage.WaitForSuccessAsync();

    private async Task<int> FetchNewestOpportunityIdAsync(int venueId)
    {
        var seed = fixture.App.SeedData;
        using var client = fixture.App.CreateAuthenticatedClient(seed.VenueManager1.Id, Role.VenueManager);
        var json = await client.GetStringAsync($"/api/Venue/{venueId}/opportunities");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(o => o.GetProperty("id").GetInt32())
            .Max();
    }
}
