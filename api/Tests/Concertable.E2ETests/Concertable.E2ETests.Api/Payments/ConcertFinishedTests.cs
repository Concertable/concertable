using System.Net;
using Concertable.Concert.Api.Responses;
using Concertable.Tests.Common;
using Xunit;
namespace Concertable.E2ETests.Api.Payments;

[Collection("E2E")]
public class ConcertFinishedTests(AppFixture fixture) : IAsyncLifetime
{

    public async Task InitializeAsync() => await fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCompleteBooking_WhenFlatFeeConcertFinishes()
    {
        // Act
        await TriggerConcertFinishedFunctionAsync();

        // Assert
        await fixture.Polling.UntilAsync(
            () => fixture.Db.Booking.GetStatusByApplicationIdAsync(fixture.SeedData.PastFlatFeeApp.ApplicationId),
            status => status == (int)BookingStatus.Complete,
            timeout: TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task ShouldCompleteBooking_WhenVenueHireConcertFinishes()
    {
        // Act
        await TriggerConcertFinishedFunctionAsync();

        // Assert
        await fixture.Polling.UntilAsync(
            () => fixture.Db.Booking.GetStatusByApplicationIdAsync(fixture.SeedData.PastVenueHireApp.ApplicationId),
            status => status == (int)BookingStatus.Complete,
            timeout: TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task ShouldCompleteBookingAndPayArtist_WhenDoorSplitConcertFinishes()
    {
        // PastDoorSplit: DoorSplit 70% — 1 ticket pre-seeded at £20 → artist share = £14 (1400 pence)

        // Act
        await TriggerConcertFinishedFunctionAsync();

        // Assert
        var paymentIntentId = await fixture.Polling.UntilAsync(
            () => fixture.Sql.Connection.GetLatestSettlementPaymentIntentIdByApplicationIdAsync(fixture.SeedData.PastDoorSplitApp.ApplicationId),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(30));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId);
        Assert.Equal(fixture.SeedData.ArtistManager1.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(1400L, intent.Amount);
    }

    [Fact]
    public async Task ShouldCompleteBookingAndPayArtist_WhenVersusConcertFinishes()
    {
        // PastVersus: Versus £100 + 70% door — 1 ticket pre-seeded at £20 → artist share = £114 (11400 pence)

        // Act
        await TriggerConcertFinishedFunctionAsync();

        // Assert
        var paymentIntentId = await fixture.Polling.UntilAsync(
            () => fixture.Sql.Connection.GetLatestSettlementPaymentIntentIdByApplicationIdAsync(fixture.SeedData.PastVersusApp.ApplicationId),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(30));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId);
        Assert.Equal(fixture.SeedData.ArtistManager1.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(11400L, intent.Amount);
    }

    private async Task TriggerConcertFinishedFunctionAsync()
    {
        var response = await fixture.Client.PostAsync("/e2e/run-completion", content: null);
        await response.ShouldBe(HttpStatusCode.OK);
    }
}
