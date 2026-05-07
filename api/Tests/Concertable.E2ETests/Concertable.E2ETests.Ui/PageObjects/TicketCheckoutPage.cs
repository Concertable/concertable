using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.PageObjects;

public class TicketCheckoutPage
{
    private readonly IPage page;
    private readonly StripePayment stripePayment;

    public TicketCheckoutPage(IPage page)
    {
        this.page = page;
        stripePayment = new StripePayment(page);
    }

    private ILocator ConfirmButton     => page.GetByTestId("confirm");
    private ILocator AwaitingScreen    => page.GetByTestId("checkout-awaiting");
    private ILocator SuccessScreen     => page.GetByTestId("checkout-success");
    private ILocator ViewTicketsButton => page.GetByTestId("view-tickets");
    private ILocator Toast(string text) =>
        page.Locator("[data-sonner-toast]").Filter(new() { HasText = text });

    public Task PayWithTestCardAsync() => ConfirmButton.ClickAsync();

    public Task PayWithNewCardAsync(string cardNumber) =>
        stripePayment.PayWithNewCardAsync(cardNumber);

    public Task WaitForAwaitingScreenAsync() =>
        Assertions.Expect(AwaitingScreen).ToBeVisibleAsync();

    public Task WaitForSuccessScreenAsync() =>
        Assertions.Expect(SuccessScreen).ToBeVisibleAsync(new() { Timeout = 30_000 });

    public Task WaitForTicketPurchasedToastAsync() =>
        Assertions.Expect(Toast("You purchased")).ToBeVisibleAsync(new() { Timeout = 30_000 });

    public Task ClickViewTicketsAsync() =>
        ViewTicketsButton.ClickAsync();
}
