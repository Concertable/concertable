namespace Concertable.E2ETests.Ui.PageObjects;

public class VenueApplicationsPage
{
    private readonly IPage page;
    private readonly string spaBaseUrl;

    public VenueApplicationsPage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.spaBaseUrl = spaBaseUrl;
    }

    private ILocator Application(int id) => page.GetByTestId($"application-{id}");
    private ILocator AcceptButton(int id) => Application(id).GetByTestId("accept");

    public Task GotoAsync(int opportunityId) =>
        page.GotoAsync($"{spaBaseUrl}/venue/my/applications/{opportunityId}");

    public Task ClickAcceptAsync(int applicationId) => AcceptButton(applicationId).ClickAsync();
}
