using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.PageObjects;

public class ApplicationCheckoutPage
{
    private readonly IPage page;
    private readonly IStripePayment payment;

    public ApplicationCheckoutPage(IPage page, IStripePayment payment)
    {
        this.page = page;
        this.payment = payment;
    }

    public Task PayWithSavedCardAsync() => payment.PayWithSavedCardAsync();
    public Task PayWithNewCardAsync(string cardNumber) => payment.PayWithNewCardAsync(cardNumber);

    public Task WaitForSuccessAsync() =>
        page.WaitForURLAsync("**/venue/my/concerts/concert/**");
}
