namespace Concertable.E2ETests.Ui.PageObjects;

public class FindPage
{
    private readonly IPage page;
    private readonly string url;

    public FindPage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.url = $"{spaBaseUrl}/find";
    }

    private ILocator FilterOpenButton  => page.GetByTestId("filter-open");
    private ILocator FilterPanel       => page.GetByTestId("filter-panel");
    private ILocator HeaderTypeSelect  => page.GetByTestId("filter-header-type");
    private ILocator ApplyButton       => page.GetByTestId("filter-apply");
    private ILocator SearchResults     => page.GetByTestId("search-results");
    private ILocator HeaderCards       => page.GetByTestId("header-card");
    private ILocator QueryInput        => page.GetByTestId("search-query");

    public Task GotoAsync() => page.GotoAsync(url);

    public async Task OpenFilterPanelAsync()
    {
        await FilterOpenButton.ClickAsync();
        await FilterPanel.WaitForAsync();
    }

    public async Task SelectHeaderTypeAsync(string type)
    {
        await HeaderTypeSelect.ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = type }).ClickAsync();
    }

    public async Task ApplyFiltersAsync()
    {
        await ApplyButton.ClickAsync();
        await FilterPanel.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    public Task WaitForResultsAsync() =>
        Assertions.Expect(SearchResults).ToBeVisibleAsync();

    public Task ClickFirstResultAsync() =>
        HeaderCards.First.ClickAsync();

    public Task TypeQueryAsync(string query) =>
        QueryInput.FillAsync(query);
}
