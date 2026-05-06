using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.PageObjects;

public class ApplyCheckoutPage
{
    private readonly IPage page;
    private readonly StripeIframe stripe;

    public ApplyCheckoutPage(IPage page)
    {
        this.page = page;
        stripe = new StripeIframe(page);
    }

    private ILocator ConfirmButton => page.GetByTestId("confirm");

    public async Task PayWithNewCardAsync(string cardNumber)
    {
        await page.WaitForURLAsync("**/artist/opportunity/checkout/**");
        await stripe.FillCardAsync(cardNumber);
        await ConfirmButton.ClickAsync();
    }

    public async Task PayWithSavedCardAsync()
    {
        await page.WaitForURLAsync("**/artist/opportunity/checkout/**");
        await ConfirmButton.ClickAsync();
    }
}
