using Concertable.E2ETests.Ui.PageObjects;
using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Steps;

[Binding]
public class VenueManagerSteps
{
    private readonly UiFixture fixture;
    private readonly Browser browser;
    private readonly WorkflowState state;
    private readonly IStripePayment payment;
    private MyVenuePage myVenuePage = null!;

    public VenueManagerSteps(
        UiFixture fixture,
        Browser browser,
        WorkflowState state,
        IStripePayment payment)
    {
        this.fixture = fixture;
        this.browser = browser;
        this.state = state;
        this.payment = payment;
    }

    [When(@"the venue manager posts a flat fee opportunity for £(\d+)")]
    public async Task PostsFlatFeeOpportunity(decimal fee)
    {
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

        var checkoutPage = new ApplicationCheckoutPage(browser.Page, payment);
        await checkoutPage.PayWithSavedCardAsync();
    }

    [When(@"the venue manager posts a venue hire opportunity for £(\d+)")]
    public async Task PostsVenueHireOpportunity(decimal fee)
    {
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

    [Given(@"a flat fee opportunity has been applied to")]
    public async Task AFlatFeeOpportunityHasBeenAppliedTo()
    {
        state.ApplicationId = fixture.App.SeedData.PendingFlatFeeApp.ApplicationId;
        await browser.Page.GotoAsync($"{fixture.App.SpaBaseUrl}/venue/application/checkout/{state.ApplicationId}");
    }

    [Given(@"a door split opportunity has been applied to")]
    public async Task ADoorSplitOpportunityHasBeenAppliedTo()
    {
        state.ApplicationId = fixture.App.SeedData.PendingDoorSplitApp.ApplicationId;
        await browser.Page.GotoAsync($"{fixture.App.SpaBaseUrl}/venue/application/checkout/{state.ApplicationId}");
    }

    [Given(@"a versus opportunity has been applied to")]
    public async Task AVersusOpportunityHasBeenAppliedTo()
    {
        state.ApplicationId = fixture.App.SeedData.PendingVersusApp.ApplicationId;
        await browser.Page.GotoAsync($"{fixture.App.SpaBaseUrl}/venue/application/checkout/{state.ApplicationId}");
    }

    [When(@"the venue manager pays the flat fee with a new card")]
    public async Task PaysWithNewCard()
    {
        var checkoutPage = new ApplicationCheckoutPage(browser.Page, payment);
        await checkoutPage.PayWithNewCardAsync(StripeCards.Success);
    }

    [When(@"the venue manager pays the flat fee with a declined card")]
    public Task PaysFlatFeeWithDeclinedCard() =>
        new ApplicationCheckoutPage(browser.Page, payment).PayWithNewCardAsync(StripeCards.Decline);

    [When(@"the venue manager pays the flat fee with a 3DS card")]
    public async Task PaysFlatFeeWith3dsCard()
    {
        await new ApplicationCheckoutPage(browser.Page, payment).PayWithNewCardAsync(StripeCards.Requires3ds);
        await payment.CompleteChallengeAsync();
    }

    [When(@"the venue manager pays the flat fee with a 3DS-failing card")]
    public async Task PaysFlatFeeWith3dsFailingCard()
    {
        await new ApplicationCheckoutPage(browser.Page, payment).PayWithNewCardAsync(StripeCards.Insufficient3ds);
        await payment.CompleteChallengeAsync();
    }

    [When(@"the venue manager registers a card with a new card")]
    public Task RegistersCardWithNewCard() =>
        new ApplicationCheckoutPage(browser.Page, payment).PayWithNewCardAsync(StripeCards.Success);

    [When(@"the venue manager registers a card with a declined card")]
    public Task RegistersCardWithDeclinedCard() =>
        new ApplicationCheckoutPage(browser.Page, payment).PayWithNewCardAsync(StripeCards.Decline);

    [When(@"the venue manager registers a card with a 3DS card")]
    public async Task RegistersCardWith3dsCard()
    {
        await new ApplicationCheckoutPage(browser.Page, payment).PayWithNewCardAsync(StripeCards.Requires3ds);
        await payment.CompleteChallengeAsync();
    }

    [When(@"the venue manager registers a card with a 3DS-failing card")]
    public async Task RegistersCardWith3dsFailingCard()
    {
        await new ApplicationCheckoutPage(browser.Page, payment).PayWithNewCardAsync(StripeCards.Requires3ds);
        await payment.FailChallengeAsync();
    }

    [Then(@"a draft concert is created")]
    public Task DraftConcertCreated() =>
        browser.Page.WaitForURLAsync("**/venue/my/concerts/concert/**");

    private Task<int> FetchNewestOpportunityIdAsync(int venueId) =>
        fixture.App.Db.Opportunity.GetNewestAsync(venueId);
}
