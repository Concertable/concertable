namespace Concertable.E2ETests.Ui.PageObjects;

public class HomePage
{
    private readonly IPage page;
    private readonly string url;

    public HomePage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.url = spaBaseUrl;
    }

    private ILocator SignInLink => page.GetByTestId("header-login");

    public Task GotoAsync() => page.GotoAsync(url);

    public Task ClickSignInAsync() => SignInLink.ClickAsync();

    public Task WaitUntilLoadedAsync(float timeoutMs = 15000) =>
        page.WaitForURLAsync(url, new() { Timeout = timeoutMs });
}
