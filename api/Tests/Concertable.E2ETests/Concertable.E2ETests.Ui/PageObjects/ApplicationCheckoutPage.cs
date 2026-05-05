namespace Concertable.E2ETests.Ui.PageObjects;

public class ApplicationCheckoutPage
{
    private readonly IPage page;

    public ApplicationCheckoutPage(IPage page) => this.page = page;

    private ILocator ConfirmButton => page.GetByTestId("confirm");

    public Task PayWithSavedCardAsync() => ConfirmButton.ClickAsync();

    public Task WaitForSuccessAsync() =>
        page.WaitForURLAsync("**/venue/my/concerts/concert/**");
}
