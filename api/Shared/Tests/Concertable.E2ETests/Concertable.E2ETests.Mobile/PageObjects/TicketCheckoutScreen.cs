using Concertable.E2ETests.Mobile.Support;

namespace Concertable.E2ETests.Mobile.PageObjects;

public class TicketCheckoutScreen
{
    private readonly MobileApp app;
    private readonly StripePaymentSheet paymentSheet;

    public TicketCheckoutScreen(MobileApp app, StripePaymentSheet paymentSheet)
    {
        this.app = app;
        this.paymentSheet = paymentSheet;
    }

    private AppiumElement PayButton => app.Driver.GetByTestId("checkout-pay", TimeSpan.FromSeconds(30));

    public void WaitUntilLoaded() => _ = PayButton;

    public Task PayWithNewCardAsync(string cardNumber)
    {
        PayButton.Click();
        return paymentSheet.PayWithNewCardAsync(cardNumber);
    }

    public Task PayWithSavedCardAsync()
    {
        PayButton.Click();
        return paymentSheet.PayWithSavedCardAsync();
    }
}
