using Concertable.Application.DTOs;
using Concertable.Web.E2ETests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Web.E2ETests.Payments;

[Collection("E2E")]
public class ConcertDraftTests : IAsyncLifetime
{
    private readonly AppFixture fixture;
    private readonly ITestOutputHelper output;

    public ConcertDraftTests(AppFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        this.output = output;
    }

    private HttpClient venueManagerClient = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
        venueManagerClient = await fixture.CreateAuthenticatedClientAsync(fixture.SeedData.VenueManager1.Email, fixture.SeedData.TestPassword);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCreateDraftAndPayArtist_WhenFlatFeeApplicationAccepted()
    {
        await venueManagerClient.PostAsSuccessAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}");

        // Venue pays artist flat fee upfront — assert correct destination
        var paymentIntentId = await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<string>($"/e2e/payment-intent/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}"),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId!.Trim('"'));
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);

        // Webhook fires SettleAsync → draft concert created
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationDto>(
                $"/api/OpportunityApplication/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCreateDraftAndPayVenue_WhenVenueHireApplicationAccepted()
    {
        await venueManagerClient.PostAsSuccessAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingVenueHireApp.ApplicationId}");

        // Artist pays venue hire fee upfront — assert correct destination
        var paymentIntentId = await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<string>($"/e2e/payment-intent/{fixture.SeedData.PendingVenueHireApp.ApplicationId}"),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId!.Trim('"'));
        Assert.Equal(fixture.SeedData.VenueManager1.StripeAccountId, intent.TransferData.DestinationId);

        // Webhook fires SettleAsync → draft concert created
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationDto>(
                $"/api/OpportunityApplication/{fixture.SeedData.PendingVenueHireApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCreateDraft_WhenDoorSplitApplicationAccepted()
    {
        await venueManagerClient.PostAsSuccessAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingDoorSplitApp.ApplicationId}");

        // No upfront payment — draft created immediately
        var application = await fixture.Client.GetAsync<OpportunityApplicationDto>(
            $"/api/OpportunityApplication/{fixture.SeedData.PendingDoorSplitApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }

    [Fact]
    public async Task ShouldCreateDraft_WhenVersusApplicationAccepted()
    {
        await venueManagerClient.PostAsSuccessAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingVersusApp.ApplicationId}");

        // No upfront payment — draft created immediately
        var application = await fixture.Client.GetAsync<OpportunityApplicationDto>(
            $"/api/OpportunityApplication/{fixture.SeedData.PendingVersusApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }
}
