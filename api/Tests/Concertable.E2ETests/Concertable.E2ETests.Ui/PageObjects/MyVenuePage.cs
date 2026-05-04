namespace Concertable.E2ETests.Ui.PageObjects;

public class MyVenuePage
{
    private const string EditButton = "edit";
    private const string SaveButton = "save";
    private const string AddOpportunity = "opportunity-add";
    private const string FlatFeeFee = "contract-flatfee-fee";

    private readonly IPage page;
    private readonly string spaBaseUrl;

    public MyVenuePage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.spaBaseUrl = spaBaseUrl;
    }

    public Task GotoAsync() => page.GotoAsync($"{spaBaseUrl}/venue/my");

    public async Task PostFlatFeeOpportunityAsync(decimal fee)
    {
        await page.GetByTestId(EditButton).ClickAsync();
        await page.GetByTestId(AddOpportunity).ClickAsync();
        await page.GetByTestId(FlatFeeFee).FillAsync(fee.ToString());
        await page.GetByTestId(SaveButton).ClickAsync();
    }

    public Task WaitUntilSavedAsync() =>
        Assertions.Expect(page.GetByTestId(SaveButton)).ToBeDisabledAsync();
}
