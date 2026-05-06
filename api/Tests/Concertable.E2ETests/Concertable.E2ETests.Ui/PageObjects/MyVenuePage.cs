namespace Concertable.E2ETests.Ui.PageObjects;

public class MyVenuePage
{
    private readonly IPage page;
    private readonly string url;

    public MyVenuePage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.url = $"{spaBaseUrl}/venue/my";
    }

    private ILocator EditButton => page.GetByTestId("edit");
    private ILocator SaveButton => page.GetByTestId("save");
    private ILocator AddOpportunityButton => page.GetByTestId("opportunity-add");
    private ILocator LastCardEdit => page.GetByTestId("opportunity-card-edit").Last;
    private ILocator FlatFeeFeeInput => LastCardEdit.GetByTestId("contract-flatfee-fee");
    private ILocator ContractTypeSelect => LastCardEdit.GetByTestId("opportunity-contract-type");
    private ILocator VenueHireFeeInput => LastCardEdit.GetByTestId("contract-venuehire-fee");

    public Task GotoAsync() => page.GotoAsync(url);

    public async Task PostFlatFeeOpportunityAsync(decimal fee)
    {
        await EditButton.ClickAsync();
        await Assertions.Expect(EditButton).ToHaveTextAsync("Editing");
        await AddOpportunityButton.ClickAsync();
        await FlatFeeFeeInput.FillAsync(fee.ToString());
        await SaveButton.ClickAsync();
    }

    public async Task PostVenueHireOpportunityAsync(decimal fee)
    {
        await EditButton.ClickAsync();
        await Assertions.Expect(EditButton).ToHaveTextAsync("Editing");
        await AddOpportunityButton.ClickAsync();
        await ContractTypeSelect.ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Venue Hire" }).ClickAsync();
        await VenueHireFeeInput.FillAsync(fee.ToString());
        await SaveButton.ClickAsync();
    }

    public Task WaitUntilSavedAsync() =>
        Assertions.Expect(EditButton).ToHaveTextAsync("Edit");
}
