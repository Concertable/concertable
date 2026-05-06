using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Steps;

[Binding]
public class PaymentSteps
{
    private readonly UiFixture fixture;

    public PaymentSteps(UiFixture fixture) => this.fixture = fixture;

    [Then(@"a payment hold of £(\d+) is captured from the artist")]
    public async Task PaymentHoldCaptured(decimal amount)
    {
        var hold = await fixture.App.Stripe.FindCapturedHoldAsync(
            fixture.App.SeedData.ArtistManager1.StripeCustomerId, amount);
        Assert.NotNull(hold);
    }

    [Then(@"a Stripe transfer of £(\d+) is made to the venue manager")]
    public async Task StripeTransferMade(decimal amount)
    {
        var transfer = await fixture.App.Stripe.FindTransferAsync(
            fixture.App.SeedData.VenueManager1.StripeAccountId, amount);
        Assert.NotNull(transfer);
    }
}
