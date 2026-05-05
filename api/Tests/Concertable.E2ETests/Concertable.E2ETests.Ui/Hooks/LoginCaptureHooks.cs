using Concertable.E2ETests.Ui.PageObjects;
using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

public static class LoginCaptureHooks
{
    private static readonly Dictionary<Role, string> storageStateByRole = [];

    public static string GetStorageState(Role role) =>
        storageStateByRole.TryGetValue(role, out var state)
            ? state
            : throw new InvalidOperationException($"No cached storage state for role '{role}'.");

    public static async Task CaptureAllAsync(UiFixture fixture)
    {
        var seed = fixture.App.SeedData;
        await CaptureAsync(fixture, Role.VenueManager, seed.VenueManager1.Email, seed.TestPassword);
        await CaptureAsync(fixture, Role.ArtistManager, seed.ArtistManager.Email, seed.TestPassword);
        await CaptureAsync(fixture, Role.Customer, seed.Customer.Email, seed.TestPassword);
    }

    private static async Task CaptureAsync(UiFixture fixture, Role role, string email, string password)
    {
        await using var context = await fixture.Browser.NewContextAsync(new() { IgnoreHTTPSErrors = true });
        var page = await context.NewPageAsync();
        var home = new HomePage(page, fixture.App.SpaBaseUrl);
        var login = new LoginPage(page, fixture.App.SpaBaseUrl);

        await home.GotoAsync();
        await home.ClickSignInAsync();
        await login.SignInAsync(email, password);
        await home.WaitUntilLoadedAsync();

        storageStateByRole[role] = await context.StorageStateAsync();
    }
}
