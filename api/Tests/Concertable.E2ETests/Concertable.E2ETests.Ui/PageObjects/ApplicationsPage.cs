namespace Concertable.E2ETests.Ui.PageObjects;

public class ApplicationsPage
{
    private readonly IPage page;
    private readonly string url;

    public ApplicationsPage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.url = $"{spaBaseUrl}/venue/my/applications";
    }

    private ILocator Application(int id) => page.GetByTestId($"application-{id}");
    private ILocator AcceptButton(int id) => Application(id).GetByTestId("accept");

    public Task GotoAsync(int opportunityId) => page.GotoAsync($"{url}/{opportunityId}");

    public Task ClickAcceptAsync(int applicationId) => AcceptButton(applicationId).ClickAsync();
}
