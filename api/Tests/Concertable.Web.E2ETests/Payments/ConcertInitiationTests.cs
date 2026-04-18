using Concertable.Application.DTOs;
using Concertable.Core.Enums;
using Concertable.Web.E2ETests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Web.E2ETests.Payments;

[Collection("E2E")]
public class ConcertInitiationTests : IAsyncLifetime
{
    private readonly AppFixture fixture;
    private readonly ITestOutputHelper output;

    public ConcertInitiationTests(AppFixture fixture, ITestOutputHelper output)
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
    public async Task ShouldPayArtistManager_WhenFlatFeeContractInitiated()
    {
        var response = await venueManagerClient.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");

        // Initiation creates a settlement PaymentIntent immediately (venue pays artist flat fee)
        var paymentIntentId = await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<string>($"/e2e/payment-intent/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}"),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId!.Trim('"'));
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
    }

    [Fact]
    public async Task ShouldPayVenueManager_WhenVenueHireContractInitiated()
    {
        var response = await venueManagerClient.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingVenueHireApp.ApplicationId}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");

        // Initiation creates a settlement PaymentIntent immediately (artist pays venue hire fee)
        var paymentIntentId = await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<string>($"/e2e/payment-intent/{fixture.SeedData.PendingVenueHireApp.ApplicationId}"),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId!.Trim('"'));
        Assert.Equal(fixture.SeedData.VenueManager1.StripeAccountId, intent.TransferData.DestinationId);
    }

    [Fact]
    public async Task ShouldSetApplicationAwaitingPayment_WhenDoorSplitContractInitiated()
    {
        var response = await venueManagerClient.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingDoorSplitApp.ApplicationId}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");

        var application = await fixture.Client.GetAsync<OpportunityApplicationDto>(
            $"/api/OpportunityApplication/{fixture.SeedData.PendingDoorSplitApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }

    [Fact]
    public async Task ShouldSetApplicationAwaitingPayment_WhenVersusContractInitiated()
    {
        var response = await venueManagerClient.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingVersusApp.ApplicationId}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");

        var application = await fixture.Client.GetAsync<OpportunityApplicationDto>(
            $"/api/OpportunityApplication/{fixture.SeedData.PendingVersusApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }
}
