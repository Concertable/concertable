using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

[Binding]
public class StripeHooks(UiFixture fixture)
{
    [BeforeTestRun(Order = 2)]
    public static async Task DetachAllCardsBeforeTestRun()
    {
        await PlaywrightHooks.Fixture.App.ResetAsync();
        await DetachSeededCustomerCardsAsync(PlaywrightHooks.Fixture.App);
    }

    [BeforeScenario("ResetsStripe", Order = 2)]
    public async Task DetachSavedPaymentMethodsAsync() =>
        await DetachSeededCustomerCardsAsync(fixture.App);

    private static async Task DetachSeededCustomerCardsAsync(AppFixture app)
    {
        var seedData = app.SeedData;
        var customerIds = new[]
        {
            seedData.VenueManager1.StripeCustomerId,
            seedData.ArtistManager1.StripeCustomerId,
            seedData.Customer.StripeCustomerId,
        };

        foreach (var id in customerIds)
        {
            await app.Stripe.DetachAllCardsAsync(id);
            await app.Stripe.AttachTestCardAsync(id);
        }
    }
}
