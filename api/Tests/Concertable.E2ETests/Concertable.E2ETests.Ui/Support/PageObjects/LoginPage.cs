namespace Concertable.E2ETests.Ui.Support.PageObjects;

public class LoginPage
{
    private const string EmailInput = "login-email";
    private const string PasswordInput = "login-password";
    private const string SubmitButton = "login-submit";

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
        await page.GetByTestId(EmailInput).FillAsync(email);
        await page.GetByTestId(PasswordInput).FillAsync(password);
        await page.GetByTestId(SubmitButton).ClickAsync();
    }

    public Task WaitForUrlAsync(string url, float timeoutMs = 15000) =>
        page.WaitForURLAsync(url, new() { Timeout = timeoutMs });
}
