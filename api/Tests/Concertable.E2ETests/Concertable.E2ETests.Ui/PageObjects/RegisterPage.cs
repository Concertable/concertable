namespace Concertable.E2ETests.Ui.PageObjects;

public class RegisterPage
{
    private readonly IPage page;
    private readonly string authBaseUrl;

    public RegisterPage(IPage page, string authBaseUrl)
    {
        this.page = page;
        this.authBaseUrl = authBaseUrl;
    }

    private ILocator EmailInput => page.GetByTestId("email");
    private ILocator PasswordInput => page.GetByTestId("password");
    private ILocator SubmitButton => page.GetByTestId("submit");
    private ILocator SuccessMessage => page.GetByTestId("success");
    private ILocator SignInLink => page.GetByTestId("signin-link");
    private ILocator ErrorMessage => page.GetByTestId("error");
    private ILocator RoleRadio(Role role) => page.GetByTestId($"role-{role}");

    public Task WaitForLoadAsync() =>
        page.WaitForURLAsync($"{authBaseUrl}/Account/Register**", new() { Timeout = 15_000 });

    public async Task RegisterAsync(string email, string password, Role? role = null)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        if (role is not null && await RoleRadio(role.Value).CountAsync() > 0)
            await RoleRadio(role.Value).CheckAsync();
        await SubmitButton.ClickAsync();
        await Assertions.Expect(SuccessMessage).ToBeVisibleAsync(new() { Timeout = 15_000 });
    }

    public Task ClickSignInAsync() => SignInLink.ClickAsync();
}
