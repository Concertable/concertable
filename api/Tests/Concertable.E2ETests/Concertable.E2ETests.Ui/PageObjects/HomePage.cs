namespace Concertable.E2ETests.Ui.PageObjects;

public class HomePage
{
    private readonly IPage page;
    private readonly string spaBaseUrl;

    public HomePage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.spaBaseUrl = spaBaseUrl;
    }

    private ILocator SignInLink => page.GetByTestId("header-login");

    public Task GotoAsync() => page.GotoAsync($"{spaBaseUrl}/");

    public Task ClickSignInAsync() => SignInLink.ClickAsync();

    public Task WaitUntilLoadedAsync(float timeoutMs = 15000) =>
        page.WaitForURLAsync($"{spaBaseUrl}/", new() { Timeout = timeoutMs });
}
