namespace Concertable.E2ETests.Ui.Support.PageObjects;

public class LoginPage
{
    private readonly IPage page;
    private readonly string spaBaseUrl;

    public LoginPage(IPage page, string spaBaseUrl)
    {
        this.page = page;
        this.spaBaseUrl = spaBaseUrl;
    }

    public Task GotoAsync() => page.GotoAsync($"{spaBaseUrl}/login");

    public async Task SignInAsync(string email, string password)
    {
        await page.Locator("input[name='Email']").FillAsync(email);
        await page.Locator("input[name='Password']").FillAsync(password);
        await page.Locator("button[type='submit']").ClickAsync();
    }

    public Task WaitForUrlAsync(string url, float timeoutMs = 15000) =>
        page.WaitForURLAsync(url, new() { Timeout = timeoutMs });
}
