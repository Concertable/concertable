using Concertable.E2ETests.Ui.PageObjects;
using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

public static class LoginCaptureHooks
{
    private static readonly Dictionary<Role, string> storageStateByRole = [];

    public static void Reset() => storageStateByRole.Clear();

    public static async Task<string> GetOrCaptureAsync(UiFixture fixture, Role role)
    {
        if (storageStateByRole.TryGetValue(role, out var state))
            return state;

        var seed = fixture.App.SeedData;
        var (email, password, spaUrl) = role switch
        {
            Role.Customer      => (seed.Customer.Email,       seed.TestPassword, fixture.App.CustomerSpaUrl),
            Role.VenueManager  => (seed.VenueManager1.Email,  seed.TestPassword, fixture.App.VenueSpaUrl),
            Role.ArtistManager => (seed.ArtistManager1.Email, seed.TestPassword, fixture.App.ArtistSpaUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };

        await CaptureAsync(fixture, role, email, password, spaUrl);
        return storageStateByRole[role];
    }

    private static async Task CaptureAsync(UiFixture fixture, Role role, string email, string password, string spaUrl)
    {
        await using var context = await fixture.Browser.NewContextAsync(new() { IgnoreHTTPSErrors = true });
        var page = await context.NewPageAsync();
        var home = new HomePage(page, spaUrl);
        var login = new LoginPage(page, spaUrl);

        await home.GotoAsync();
        await home.ClickSignInAsync();
        await login.SignInAsync(email, password);
        await home.WaitUntilLoadedAsync();

        storageStateByRole[role] = await context.StorageStateAsync();
    }
}
