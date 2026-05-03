using System.Net;
using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Api.Responses;
using Concertable.E2ETests.Common;
using Concertable.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.E2ETests.Api.Payments;

[Collection("E2E")]

public class ConcertFinishedTests : IAsyncLifetime
{
    private readonly AppFixture fixture;
    private readonly ITestOutputHelper output;

    public ConcertFinishedTests(AppFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        this.output = output;
    }

    private HttpClient customerClient = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
        customerClient = fixture.CreateAuthenticatedClient(fixture.SeedData.Customer.Id, Role.Customer);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCompleteApplication_WhenFlatFeeConcertFinishes()
    {
        await fixture.Client.PostAsSuccessAsync($"/e2e/finish/{fixture.SeedData.PostedFlatFeeApp.ConcertId!.Value}");

        var application = await fixture.Client.GetAsync<ApplicationResponse>(
            $"/api/Application/{fixture.SeedData.PostedFlatFeeApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }

    [Fact]
    public async Task ShouldCompleteApplication_WhenVenueHireConcertFinishes()
    {
        await fixture.Client.PostAsSuccessAsync($"/e2e/finish/{fixture.SeedData.PostedVenueHireApp.ConcertId!.Value}");

        var application = await fixture.Client.GetAsync<ApplicationResponse>(
            $"/api/Application/{fixture.SeedData.PostedVenueHireApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }

    [Fact]
    public async Task ShouldCompleteApplicationAndPayArtist_WhenDoorSplitConcertFinishes()
    {
        // Op 50: DoorSplit 70% â€” 1 ticket pre-seeded at Â£20 = Â£20 revenue â†’ artist share = Â£14 (1400 pence)
        var response = await fixture.Client.PostAsync($"/e2e/finish/{fixture.SeedData.FinishedDoorSplitApp.ConcertId!.Value}");
        await response.ShouldBe(HttpStatusCode.OK);

        var paymentIntentId = await fixture.Polling.UntilAsync(
            () => fixture.Sql.Connection.GetLatestSettlementPaymentIntentIdByApplicationIdAsync(fixture.SeedData.FinishedDoorSplitApp.ApplicationId),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId);
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(1400L, intent.Amount);

        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<ApplicationResponse>(
                $"/api/Application/{fixture.SeedData.FinishedDoorSplitApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCompleteApplicationAndPayArtist_WhenVersusConcertFinishes()
    {
        // Op 51: Versus â€” guarantee Â£100 + 70% of door â€” 1 ticket pre-seeded at Â£20 â†’ artist share = Â£114 (11400 pence)
        var response = await fixture.Client.PostAsync($"/e2e/finish/{fixture.SeedData.FinishedVersusApp.ConcertId!.Value}");
        await response.ShouldBe(HttpStatusCode.OK);

        var paymentIntentId = await fixture.Polling.UntilAsync(
            () => fixture.Sql.Connection.GetLatestSettlementPaymentIntentIdByApplicationIdAsync(fixture.SeedData.FinishedVersusApp.ApplicationId),
            id => id is not null,
            timeout: TimeSpan.FromSeconds(15));

        var intent = await fixture.StripePaymentIntents.GetAsync(paymentIntentId);
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(11400L, intent.Amount);

        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<ApplicationResponse>(
                $"/api/Application/{fixture.SeedData.FinishedVersusApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }
}
