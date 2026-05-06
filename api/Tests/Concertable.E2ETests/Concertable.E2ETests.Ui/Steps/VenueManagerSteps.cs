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

        var applicationsPage = new ApplicationsPage(browser.Page, fixture.App.SpaBaseUrl);
        await applicationsPage.GotoAsync(state.OpportunityId);
        await applicationsPage.ClickAcceptAsync(state.ApplicationId);

        var acceptPage = new AcceptApplicationPage(browser.Page);
        await acceptPage.ClickConfirmAsync();

        var checkoutPage = new ApplicationCheckoutPage(browser.Page);
        await checkoutPage.PayWithSavedCardAsync();
    }

    [When(@"the venue manager posts a venue hire opportunity for £(\d+)")]
    public async Task PostsVenueHireOpportunity(decimal fee)
    {
        await browser.UseRoleAsync(Role.VenueManager);

        state.VenueId = fixture.App.SeedData.VenueManager1.VenueId;

        myVenuePage = new MyVenuePage(browser.Page, fixture.App.SpaBaseUrl);
        await myVenuePage.GotoAsync();
        await myVenuePage.PostVenueHireOpportunityAsync(fee);
        await myVenuePage.WaitUntilSavedAsync();

        state.OpportunityId = await FetchNewestOpportunityIdAsync(state.VenueId);
    }

    [When(@"the venue manager posts a door split opportunity for (\d+)% door")]
    public async Task PostsDoorSplitOpportunity(int doorPercent)
    {
        await browser.UseRoleAsync(Role.VenueManager);

        state.VenueId = fixture.App.SeedData.VenueManager1.VenueId;

        myVenuePage = new MyVenuePage(browser.Page, fixture.App.SpaBaseUrl);
        await myVenuePage.GotoAsync();
        await myVenuePage.PostDoorSplitOpportunityAsync(doorPercent);
        await myVenuePage.WaitUntilSavedAsync();

        state.OpportunityId = await FetchNewestOpportunityIdAsync(state.VenueId);
    }

    [When(@"the venue manager posts a versus opportunity for £(\d+) guarantee and (\d+)% door")]
    public async Task PostsVersusOpportunity(int guarantee, int doorPercent)
    {
        await browser.UseRoleAsync(Role.VenueManager);

        state.VenueId = fixture.App.SeedData.VenueManager1.VenueId;

        myVenuePage = new MyVenuePage(browser.Page, fixture.App.SpaBaseUrl);
        await myVenuePage.GotoAsync();
        await myVenuePage.PostVersusOpportunityAsync(guarantee, doorPercent);
        await myVenuePage.WaitUntilSavedAsync();

        state.OpportunityId = await FetchNewestOpportunityIdAsync(state.VenueId);
    }

    [When(@"the venue manager accepts the application")]
    public async Task AcceptsApplication()
    {
        await browser.UseRoleAsync(Role.VenueManager);

        var applicationsPage = new ApplicationsPage(browser.Page, fixture.App.SpaBaseUrl);
        await applicationsPage.GotoAsync(state.OpportunityId);
        await applicationsPage.ClickAcceptAsync(state.ApplicationId);

        var acceptPage = new AcceptApplicationPage(browser.Page);
        await acceptPage.ClickConfirmAsync();
    }

    [Then(@"a draft concert is created")]
    public Task DraftConcertCreated() =>
        browser.Page.WaitForURLAsync("**/venue/my/concerts/concert/**");

    private Task<int> FetchNewestOpportunityIdAsync(int venueId) =>
        fixture.App.Db.Opportunity.GetNewestAsync(venueId);
}
