namespace Concertable.E2ETests.Ui.PageObjects;

public class VenueAcceptApplicationPage
{
    private readonly IPage page;

    public VenueAcceptApplicationPage(IPage page) => this.page = page;

    private ILocator ConfirmButton => page.GetByTestId("confirm");

    public Task ClickConfirmAsync() => ConfirmButton.ClickAsync();
}
