using Concertable.E2ETests.Ui.Support;
using Concertable.E2ETests.Ui.Support.PageObjects;

namespace Concertable.E2ETests.Ui.Steps;

[Binding]
public class LoginSteps
{
    private readonly UiFixture fixture;
    private IBrowserContext context = null!;
    private IPage page = null!;
    private HomePage homePage = null!;
    private LoginPage loginPage = null!;

    public LoginSteps(UiFixture fixture) => this.fixture = fixture;

    [Given(@"a visitor is on the home page")]
    public async Task VisitorOnHomePage()
    {
        context = await fixture.Browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });
        page = await context.NewPageAsync();
        homePage = new HomePage(page, fixture.App.SpaBaseUrl);
        loginPage = new LoginPage(page, fixture.App.SpaBaseUrl);
        await homePage.GotoAsync();
    }

    [When(@"they click sign in")]
    public Task ClickSignIn() => homePage.ClickSignInAsync();

    [When(@"they submit seeded customer credentials")]
    public Task SubmitCustomerCredentials()
    {
        var seed = fixture.App.SeedData;
        return loginPage.SignInAsync(seed.Customer.Email, seed.TestPassword);
    }

    [Then(@"they are returned to the home page")]
    public Task ReturnedToHomePage() => homePage.WaitUntilLoadedAsync();

    [AfterScenario]
    public async Task AfterScenarioCleanup()
    {
        if (context is not null)
            await context.DisposeAsync();
    }
}
