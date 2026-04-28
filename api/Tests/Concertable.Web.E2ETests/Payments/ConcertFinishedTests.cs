using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Api.Responses;
using Concertable.Web.E2ETests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Web.E2ETests.Payments;

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
        customerClient = await fixture.CreateAuthenticatedClientAsync(fixture.SeedData.Customer.Email, fixture.SeedData.TestPassword);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCompleteApplication_WhenFlatFeeConcertFinishes()
    {
        await fixture.Client.PostAsSuccessAsync($"/e2e/finish/{fixture.SeedData.PostedFlatFeeApp.ConcertId!.Value}");

        var application = await fixture.Client.GetAsync<OpportunityApplicationResponse>(
            $"/api/OpportunityApplication/{fixture.SeedData.PostedFlatFeeApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }

    [Fact]
    public async Task ShouldCompleteApplication_WhenVenueHireConcertFinishes()
    {
        await fixture.Client.PostAsSuccessAsync($"/e2e/finish/{fixture.SeedData.PostedVenueHireApp.ConcertId!.Value}");

        var application = await fixture.Client.GetAsync<OpportunityApplicationResponse>(
            $"/api/OpportunityApplication/{fixture.SeedData.PostedVenueHireApp.ApplicationId}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
    }

    [Fact]
    public async Task ShouldCompleteApplicationAndPayArtist_WhenDoorSplitConcertFinishes()
    {
        // Op 50: DoorSplit 70% — 1 ticket pre-seeded at £20 = £20 revenue → artist share = £14 (1400 pence)
        var finishBody = await fixture.Client.PostAsSuccessAsync($"/e2e/finish/{fixture.SeedData.FinishedDoorSplitApp.ConcertId!.Value}")
            .ContinueWith(t => t.Result.Content.ReadAsStringAsync()).Unwrap();

        var intent = await fixture.StripePaymentIntents.GetAsync(finishBody.Trim('"'));
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(1400L, intent.Amount);

        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationResponse>(
                $"/api/OpportunityApplication/{fixture.SeedData.FinishedDoorSplitApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCompleteApplicationAndPayArtist_WhenVersusConcertFinishes()
    {
        // Op 51: Versus — guarantee £100 + 70% of door — 1 ticket pre-seeded at £20 → artist share = £114 (11400 pence)
        var finishBody = await fixture.Client.PostAsSuccessAsync($"/e2e/finish/{fixture.SeedData.FinishedVersusApp.ConcertId!.Value}")
            .ContinueWith(t => t.Result.Content.ReadAsStringAsync()).Unwrap();

        var intent = await fixture.StripePaymentIntents.GetAsync(finishBody.Trim('"'));
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(11400L, intent.Amount);

        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationResponse>(
                $"/api/OpportunityApplication/{fixture.SeedData.FinishedVersusApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }
}
