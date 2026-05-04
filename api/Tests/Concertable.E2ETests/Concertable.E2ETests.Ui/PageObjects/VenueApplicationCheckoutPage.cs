using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.PageObjects;

public class VenueApplicationCheckoutPage
{
    private readonly IPage page;
    private readonly StripeIframe stripe;

    public VenueApplicationCheckoutPage(IPage page)
    {
        this.page = page;
        stripe = new StripeIframe(page);
    }

    private ILocator ConfirmButton => page.GetByTestId("confirm");

    public async Task PayWithCardAsync(string cardNumber)
    {
        await stripe.FillCardAsync(cardNumber);
        await ConfirmButton.ClickAsync();
    }

    public Task WaitForSuccessAsync() =>
        Assertions.Expect(page.GetByText("Application Accepted")).ToBeVisibleAsync(
            new() { Timeout = 30000 });
}
