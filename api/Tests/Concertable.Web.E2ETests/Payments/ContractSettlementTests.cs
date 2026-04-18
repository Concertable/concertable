using Concertable.Application.DTOs;
using Concertable.Web.E2ETests.Infrastructure;
using Stripe;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Web.E2ETests.Payments;

[Collection("E2E")]
public class ContractSettlementTests : IAsyncLifetime
{
    private readonly AppFixture fixture;
    private readonly ITestOutputHelper output;

    public ContractSettlementTests(AppFixture fixture, ITestOutputHelper output)
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
    public async Task ShouldPayArtistManager_WhenDoorSplitConcertFinishes()
    {
        // Arrange: purchase 1 ticket at £20 to create door revenue
        var purchaseResponse = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.FinishedDoorSplitApp.Concert.Id, Quantity = 1 });

        var purchaseBody = await purchaseResponse.Content.ReadAsStringAsync();
        Assert.True(purchaseResponse.IsSuccessStatusCode, $"{(int)purchaseResponse.StatusCode} {purchaseResponse.StatusCode}: {purchaseBody}");

        await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.FinishedDoorSplitApp.Concert.Id),
            timeout: TimeSpan.FromSeconds(15));

        // Act: trigger settlement for the finished concert
        var finishResponse = await fixture.Client.PostAsync($"/e2e/finish/{fixture.SeedData.FinishedDoorSplitApp.Concert.Id}");
        var finishBody = await finishResponse.Content.ReadAsStringAsync();
        Assert.True(finishResponse.IsSuccessStatusCode, $"{(int)finishResponse.StatusCode} {finishResponse.StatusCode}: {finishBody}");

        // Assert: poll Stripe for the settlement PaymentIntent
        // Op 50: DoorSplit 70% — 1 ticket at £20 = £20 revenue → artist share = £14 (1400 pence)
        var searchOptions = new PaymentIntentSearchOptions
        {
            Query = $"metadata['applicationId']:'{fixture.SeedData.FinishedDoorSplitApp.ApplicationId}' AND metadata['type']:'settlement'"
        };

        var intents = await fixture.Polling.UntilAsync(
            async () => (await fixture.StripePaymentIntents.SearchAsync(searchOptions)).Data,
            data => data.Any(),
            timeout: TimeSpan.FromSeconds(15));

        var intent = intents.First();
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(1400L, intent.Amount); // £20 * 70% = £14 → 1400 pence
    }

    [Fact]
    public async Task ShouldPayArtistManager_WhenVersusConcertFinishes()
    {
        // Arrange: purchase 1 ticket at £20 to create door revenue
        var purchaseResponse = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.FinishedVersusApp.Concert.Id, Quantity = 1 });

        var purchaseBody = await purchaseResponse.Content.ReadAsStringAsync();
        Assert.True(purchaseResponse.IsSuccessStatusCode, $"{(int)purchaseResponse.StatusCode} {purchaseResponse.StatusCode}: {purchaseBody}");

        await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.FinishedVersusApp.Concert.Id),
            timeout: TimeSpan.FromSeconds(15));

        // Act: trigger settlement for the finished concert
        var finishResponse = await fixture.Client.PostAsync($"/e2e/finish/{fixture.SeedData.FinishedVersusApp.Concert.Id}");
        var finishBody = await finishResponse.Content.ReadAsStringAsync();
        Assert.True(finishResponse.IsSuccessStatusCode, $"{(int)finishResponse.StatusCode} {finishResponse.StatusCode}: {finishBody}");

        // Assert: poll Stripe for the settlement PaymentIntent
        // Op 51: Versus — guarantee £100 + 70% of door — 1 ticket at £20 = £20 revenue → artist share = £100 + £14 = £114 (11400 pence)
        var searchOptions = new PaymentIntentSearchOptions
        {
            Query = $"metadata['applicationId']:'{fixture.SeedData.FinishedVersusApp.ApplicationId}' AND metadata['type']:'settlement'"
        };

        var intents = await fixture.Polling.UntilAsync(
            async () => (await fixture.StripePaymentIntents.SearchAsync(searchOptions)).Data,
            data => data.Any(),
            timeout: TimeSpan.FromSeconds(15));

        var intent = intents.First();
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
        Assert.Equal(11400L, intent.Amount); // £100 guarantee + (£20 * 70%) = £114 → 11400 pence
    }
}
