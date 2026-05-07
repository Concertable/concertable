using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.PageObjects;

public class ApplyCheckoutPage
{
    private readonly IPage page;
    private readonly StripePayment payment;

    public ApplyCheckoutPage(IPage page)
    {
        this.page = page;
        payment = new StripePayment(page);
    }

    public async Task PayWithNewCardAsync(string cardNumber)
    {
        await page.WaitForURLAsync("**/artist/opportunity/checkout/**");
        await payment.PayWithNewCardAsync(cardNumber);
    }

    public async Task PayWithSavedCardAsync()
    {
        await page.WaitForURLAsync("**/artist/opportunity/checkout/**");
        await payment.PayWithSavedCardAsync();
    }
}
