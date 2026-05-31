using Concertable.E2ETests.Mobile.Support;

namespace Concertable.E2ETests.Mobile.PageObjects;

public class ConcertDetailScreen
{
    private readonly MobileApp app;

    public ConcertDetailScreen(MobileApp app) => this.app = app;

    private AppiumElement BuyTicketsButton => app.Driver.GetByTestId("buy-tickets");

    public void WaitUntilLoaded() => _ = BuyTicketsButton;

    public void ClickBuyTickets() => BuyTicketsButton.Click();
}
