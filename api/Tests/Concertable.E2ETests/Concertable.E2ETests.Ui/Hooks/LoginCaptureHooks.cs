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
        var (email, password) = role switch
        {
            Role.Customer      => (seed.Customer.Email,       seed.TestPassword),
            Role.VenueManager  => (seed.VenueManager1.Email,  seed.TestPassword),
            Role.ArtistManager => (seed.ArtistManager1.Email, seed.TestPassword),
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };

        await CaptureAsync(fixture, role, email, password);
        return storageStateByRole[role];
    }

    private static async Task CaptureAsync(UiFixture fixture, Role role, string email, string password)
    {
        await using var context = await fixture.Browser.NewContextAsync(new() { IgnoreHTTPSErrors = true });
        var page = await context.NewPageAsync();
        var spaUrl = fixture.App.GetSpaUrlForRole(role);
        var home = new HomePage(page, spaUrl);
        var login = new LoginPage(page, spaUrl);

        await home.GotoAsync();
        await home.ClickSignInAsync();
        await login.SignInAsync(email, password);
        await home.WaitUntilLoadedAsync();

        storageStateByRole[role] = await context.StorageStateAsync();
    }
}
