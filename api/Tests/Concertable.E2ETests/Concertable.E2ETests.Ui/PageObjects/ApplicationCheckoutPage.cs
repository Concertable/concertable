using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.PageObjects;

public class ApplicationCheckoutPage
{
    private readonly IPage page;
    private readonly StripePayment payment;

    public ApplicationCheckoutPage(IPage page)
    {
        this.page = page;
        payment = new StripePayment(page);
    }

    public Task PayWithSavedCardAsync() => payment.PayWithSavedCardAsync();
    public Task PayWithNewCardAsync(string cardNumber) => payment.PayWithNewCardAsync(cardNumber);

    public Task WaitForSuccessAsync() =>
        page.WaitForURLAsync("**/venue/my/concerts/concert/**");
}
