using Concertable.E2ETests.Mobile.Support;

namespace Concertable.E2ETests.Mobile.PageObjects;

public class HomeScreen
{
    private readonly MobileApp app;

    public HomeScreen(MobileApp app) => this.app = app;

    private AppiumElement FirstConcertCard => app.Driver.GetByTestId("concert-card", TimeSpan.FromSeconds(45));

    public void WaitUntilLoaded() => _ = FirstConcertCard;

    public void OpenFirstConcert() => FirstConcertCard.Click();
}
