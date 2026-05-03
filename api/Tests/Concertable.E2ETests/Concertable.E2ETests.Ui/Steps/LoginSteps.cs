using Concertable.E2ETests.Ui.Hooks;
using Concertable.E2ETests.Ui.Support.PageObjects;

namespace Concertable.E2ETests.Ui.Steps;

[Binding]
public class LoginSteps
{
    private IBrowserContext context = null!;
    private IPage page = null!;
    private LoginPage loginPage = null!;

    [Given(@"a fresh browser context")]
    public async Task GivenAFreshBrowserContext()
    {
        context = await TestRunHooks.Fixture.Browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true
        });
        page = await context.NewPageAsync();
        loginPage = new LoginPage(page, TestRunHooks.Fixture.App.SpaBaseUrl);
    }

    [When(@"the customer signs in with seeded credentials")]
    public async Task WhenCustomerSignsIn()
    {
        var seed = TestRunHooks.Fixture.App.SeedData;
        await loginPage.GotoAsync();
        await loginPage.SignInAsync(seed.Customer.Email, seed.TestPassword);
    }

    [Then(@"the SPA lands on the home page")]
    public async Task ThenSpaLandsOnHome()
    {
        await loginPage.WaitForUrlAsync($"{TestRunHooks.Fixture.App.SpaBaseUrl}/");
    }

    [AfterScenario]
    public async Task AfterScenarioCleanup()
    {
        if (context is not null)
            await context.DisposeAsync();
    }
}
