using Concertable.Application.DTOs;
using Concertable.Core.Enums;
using Concertable.Web.E2ETests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Web.E2ETests.Payments;

[Collection("E2E")]
public class ConcertSettlementTests : IAsyncLifetime
{
    private readonly AppFixture fixture;
    private readonly ITestOutputHelper output;

    public ConcertSettlementTests(AppFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        this.output = output;
    }

    private HttpClient venueManagerClient = null!;
    private HttpClient customerClient = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
        venueManagerClient = await fixture.CreateAuthenticatedClientAsync(fixture.SeedData.VenueManager1.Email, fixture.SeedData.TestPassword);
        customerClient = await fixture.CreateAuthenticatedClientAsync(fixture.SeedData.Customer.Email, fixture.SeedData.TestPassword);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCreateConcert_WhenFlatFeeContractSettles()
    {
        // Accept triggers InitiateAsync (payment) → webhook fires → SettleAsync (creates concert)
        var acceptResponse = await venueManagerClient.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}");

        var acceptBody = await acceptResponse.Content.ReadAsStringAsync();
        Assert.True(acceptResponse.IsSuccessStatusCode, $"{(int)acceptResponse.StatusCode} {acceptResponse.StatusCode}: {acceptBody}");

        // Poll until SettleAsync has run and created the concert
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationDto>(
                $"/api/OpportunityApplication/{fixture.SeedData.PendingFlatFeeApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCreateConcert_WhenVenueHireContractSettles()
    {
        // Accept triggers InitiateAsync (payment) → webhook fires → SettleAsync (creates concert)
        var acceptResponse = await venueManagerClient.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.PendingVenueHireApp.ApplicationId}");

        var acceptBody = await acceptResponse.Content.ReadAsStringAsync();
        Assert.True(acceptResponse.IsSuccessStatusCode, $"{(int)acceptResponse.StatusCode} {acceptResponse.StatusCode}: {acceptBody}");

        // Poll until SettleAsync has run and created the concert
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationDto>(
                $"/api/OpportunityApplication/{fixture.SeedData.PendingVenueHireApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Accepted,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCompleteApplication_WhenDoorSplitContractSettles()
    {
        // Buy a ticket so there's door revenue, then trigger FinishedAsync which pays artist
        // The payment webhook then fires SettleAsync which marks the application Complete
        var purchaseResponse = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.FinishedDoorSplitApp.ConcertId!.Value, Quantity = 1 });

        var purchaseBody = await purchaseResponse.Content.ReadAsStringAsync();
        Assert.True(purchaseResponse.IsSuccessStatusCode, $"{(int)purchaseResponse.StatusCode} {purchaseResponse.StatusCode}: {purchaseBody}");

        await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.FinishedDoorSplitApp.ConcertId!.Value),
            timeout: TimeSpan.FromSeconds(15));

        var finishResponse = await fixture.Client.PostAsync($"/e2e/finish/{fixture.SeedData.FinishedDoorSplitApp.ConcertId!.Value}");
        var finishBody = await finishResponse.Content.ReadAsStringAsync();
        Assert.True(finishResponse.IsSuccessStatusCode, $"{(int)finishResponse.StatusCode} {finishResponse.StatusCode}: {finishBody}");

        // Settlement webhook fires after the PaymentIntent is confirmed → marks application Complete
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationDto>(
                $"/api/OpportunityApplication/{fixture.SeedData.FinishedDoorSplitApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Complete,
            timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ShouldCompleteApplication_WhenVersusContractSettles()
    {
        var purchaseResponse = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.FinishedVersusApp.ConcertId!.Value, Quantity = 1 });

        var purchaseBody = await purchaseResponse.Content.ReadAsStringAsync();
        Assert.True(purchaseResponse.IsSuccessStatusCode, $"{(int)purchaseResponse.StatusCode} {purchaseResponse.StatusCode}: {purchaseBody}");

        await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.FinishedVersusApp.ConcertId!.Value),
            timeout: TimeSpan.FromSeconds(15));

        var finishResponse = await fixture.Client.PostAsync($"/e2e/finish/{fixture.SeedData.FinishedVersusApp.ConcertId!.Value}");
        var finishBody = await finishResponse.Content.ReadAsStringAsync();
        Assert.True(finishResponse.IsSuccessStatusCode, $"{(int)finishResponse.StatusCode} {finishResponse.StatusCode}: {finishBody}");

        // Settlement webhook fires after the PaymentIntent is confirmed → marks application Complete
        await fixture.Polling.UntilAsync(
            async () => await fixture.Client.GetAsync<OpportunityApplicationDto>(
                $"/api/OpportunityApplication/{fixture.SeedData.FinishedVersusApp.ApplicationId}"),
            app => app?.Status == ApplicationStatus.Complete,
            timeout: TimeSpan.FromSeconds(15));
    }
}
