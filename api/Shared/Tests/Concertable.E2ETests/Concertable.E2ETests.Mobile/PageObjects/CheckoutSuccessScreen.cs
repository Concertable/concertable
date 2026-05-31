using Concertable.E2ETests.Mobile.Support;

namespace Concertable.E2ETests.Mobile.PageObjects;

public class CheckoutSuccessScreen
{
    private readonly MobileApp app;

    public CheckoutSuccessScreen(MobileApp app) => this.app = app;

    private AppiumElement SuccessScreen   => app.Driver.GetByTestId("checkout-success", TimeSpan.FromSeconds(45));
    private AppiumElement ViewTicketsButton => app.Driver.GetByTestId("view-tickets");

    public void WaitUntilVisible() => _ = SuccessScreen;

    public void ClickViewTickets() => ViewTicketsButton.Click();
}
