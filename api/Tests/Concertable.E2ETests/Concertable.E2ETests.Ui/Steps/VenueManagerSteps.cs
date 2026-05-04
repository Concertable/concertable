using Concertable.E2ETests.Ui.Support;
using Concertable.E2ETests.Ui.PageObjects;

namespace Concertable.E2ETests.Ui.Steps;

[Binding]
public class VenueManagerSteps
{
    private readonly UiFixture fixture;
    private readonly Browser browser;
    private MyVenuePage myVenuePage = null!;

    public VenueManagerSteps(UiFixture fixture, Browser browser)
    {
        this.fixture = fixture;
        this.browser = browser;
    }

    [When(@"the venue manager posts a flat fee opportunity for £(\d+)")]
    public async Task PostsFlatFeeOpportunity(decimal fee)
    {
        myVenuePage = new MyVenuePage(browser.Page, fixture.App.SpaBaseUrl);
        await myVenuePage.GotoAsync();
        await myVenuePage.PostFlatFeeOpportunityAsync(fee);
    }

    [Then(@"the opportunity is saved")]
    public Task OpportunityIsSaved() => myVenuePage.WaitUntilSavedAsync();
}
