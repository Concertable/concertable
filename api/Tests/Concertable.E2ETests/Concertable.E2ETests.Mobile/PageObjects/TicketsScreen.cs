using Concertable.E2ETests.Mobile.Support;

namespace Concertable.E2ETests.Mobile.PageObjects;

public class TicketsScreen
{
    private readonly MobileApp app;

    public TicketsScreen(MobileApp app) => this.app = app;

    private AppiumElement FirstTicketCard => app.Driver.GetByTestId("ticket-card", TimeSpan.FromSeconds(30));

    public void WaitUntilLoaded() => _ = FirstTicketCard;

    public void OpenFirstTicket() => FirstTicketCard.Click();
}
