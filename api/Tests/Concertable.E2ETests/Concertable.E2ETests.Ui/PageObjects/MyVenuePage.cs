namespace Concertable.E2ETests.Ui.PageObjects;

public class MyVenuePage
{
    private readonly IPage page;
    private readonly string spaBaseUrl;

    public MyVenuePage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.spaBaseUrl = spaBaseUrl;
    }

    private ILocator EditButton => page.GetByTestId("edit");
    private ILocator SaveButton => page.GetByTestId("save");
    private ILocator AddOpportunityButton => page.GetByTestId("opportunity-add");
    private ILocator FlatFeeFeeInput => page.GetByTestId("opportunity-card-edit").Last.GetByTestId("contract-flatfee-fee");

    public Task GotoAsync() => page.GotoAsync($"{spaBaseUrl}/venue/my");

    public async Task PostFlatFeeOpportunityAsync(decimal fee)
    {
        await EditButton.ClickAsync();
        await AddOpportunityButton.ClickAsync();
        await FlatFeeFeeInput.FillAsync(fee.ToString());
        await SaveButton.ClickAsync();
    }

    public Task WaitUntilSavedAsync() =>
        Assertions.Expect(SaveButton).ToBeDisabledAsync();
}
