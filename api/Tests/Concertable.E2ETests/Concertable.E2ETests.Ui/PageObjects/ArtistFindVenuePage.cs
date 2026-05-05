namespace Concertable.E2ETests.Ui.PageObjects;

public class ArtistFindVenuePage
{
    private readonly IPage page;
    private readonly string spaBaseUrl;

    public ArtistFindVenuePage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.spaBaseUrl = spaBaseUrl;
    }

    private ILocator Opportunity(int id) => page.GetByTestId($"opportunity-{id}");
    private ILocator ApplyButton(int id) => Opportunity(id).GetByTestId("apply");

    public Task GotoAsync(int venueId) => page.GotoAsync($"{spaBaseUrl}/artist/find/venue/{venueId}");

    public Task ApplyAsync(int opportunityId) => ApplyButton(opportunityId).ClickAsync();

    public Task WaitUntilAppliedAsync(int opportunityId) =>
        Assertions.Expect(page.GetByText("Application submitted!")).ToBeVisibleAsync();
}
