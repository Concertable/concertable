using System.Net;
using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Api.Responses;
using Concertable.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.E2ETests.Api.Payments;

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
        venueManagerClient = fixture.CreateAuthenticatedClient(fixture.SeedData.VenueManager1.Id, Role.VenueManager);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCreateDraftAndPayArtist_WhenFlatFeeApplicationAccepted()
    {
        await venueManagerClient.PostAsSuccessAsync(
            $"/api/Application/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}/accept",
            new { PaymentMethodId = AppFixture.TestPaymentMethodId });

        // Venue pays artist flat fee upfront â€” assert correct destination
        var paymentIntentId = await fixture.Polling.UntilAsync(
            () => fixture.Sql.Connection.GetLatestSettlementPaymentIntentIdByApplicationIdAsync(fixture.SeedData.PendingFlatFeeApp.ApplicationId),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId);
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);

        // Webhook fires SettleAsync â†’ draft concert created
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<ApplicationResponse>(
                $"/api/Application/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCreateDraftAndPayVenue_WhenVenueHireApplicationAccepted()
    {
        var response = await venueManagerClient.PostAsync(
            $"/api/Application/{fixture.SeedData.PendingVenueHireApp.ApplicationId}/accept",
            (HttpContent?)null);
        await response.ShouldBe(HttpStatusCode.OK);

        // Artist pays venue hire fee upfront â€” assert correct destination
        var paymentIntentId = await fixture.Polling.UntilAsync(
            () => fixture.Sql.Connection.GetLatestSettlementPaymentIntentIdByApplicationIdAsync(fixture.SeedData.PendingVenueHireApp.ApplicationId),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId);
        Assert.Equal(fixture.SeedData.VenueManager1.StripeAccountId, intent.TransferData.DestinationId);

        // Webhook fires SettleAsync â†’ draft concert created
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<ApplicationResponse>(
                $"/api/Application/{fixture.SeedData.PendingVenueHireApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCreateDraft_WhenDoorSplitApplicationAccepted()
    {
        await venueManagerClient.PostAsSuccessAsync(
            $"/api/Application/{fixture.SeedData.PendingDoorSplitApp.ApplicationId}/accept",
            new { PaymentMethodId = AppFixture.TestPaymentMethodId });

        // No upfront payment â€” draft created immediately
        var application = await fixture.Client.GetAsync<ApplicationResponse>(
            $"/api/Application/{fixture.SeedData.PendingDoorSplitApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }

    [Fact]
    public async Task ShouldCreateDraft_WhenVersusApplicationAccepted()
    {
        await venueManagerClient.PostAsSuccessAsync(
            $"/api/Application/{fixture.SeedData.PendingVersusApp.ApplicationId}/accept",
            new { PaymentMethodId = AppFixture.TestPaymentMethodId });

        // No upfront payment â€” draft created immediately
        var application = await fixture.Client.GetAsync<ApplicationResponse>(
            $"/api/Application/{fixture.SeedData.PendingVersusApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }
}
