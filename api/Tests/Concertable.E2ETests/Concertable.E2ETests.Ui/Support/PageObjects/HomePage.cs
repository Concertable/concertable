namespace Concertable.E2ETests.Ui.Support.PageObjects;

public class HomePage
{
    private const string SignInLink = "header-login";

    private readonly IPage page;
    private readonly string spaBaseUrl;

    public HomePage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.spaBaseUrl = spaBaseUrl;
    }

    public Task GotoAsync() => page.GotoAsync($"{spaBaseUrl}/");

    public Task ClickSignInAsync() => page.GetByTestId(SignInLink).ClickAsync();

    public Task WaitUntilLoadedAsync(float timeoutMs = 15000) =>
        page.WaitForURLAsync($"{spaBaseUrl}/", new() { Timeout = timeoutMs });
}
