using Concertable.Concert.Application.DTOs;
using Concertable.Web.E2ETests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Web.E2ETests.Payments;

[Collection("E2E")]

public class TicketPurchaseTests : IAsyncLifetime
{
    private readonly AppFixture fixture;
    private readonly ITestOutputHelper output;

    public TicketPurchaseTests(AppFixture fixture, ITestOutputHelper output)
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
    public async Task ShouldPayVenueManager_WhenFlatFeeContract()
    {
        // Act
        var response = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.FlatFeeUpcomingConcert.Id, Quantity = 1 });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");
        var purchase = await response.Content.ReadAsync<TicketPaymentResponse>();
        Assert.Empty(purchase!.TicketIds);

        var tickets = await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.FlatFeeUpcomingConcert.Id),
            timeout: TimeSpan.FromSeconds(15));

        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedData.FlatFeeUpcomingConcert.Id);

        var intent = await fixture.StripePaymentIntents.GetAsync(purchase.TransactionId);
        Assert.Equal(fixture.SeedData.VenueManager1.StripeAccountId, intent.TransferData.DestinationId);
    }

    [Fact]
    public async Task ShouldPayVenueManager_WhenDoorSplitContract()
    {
        var response = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.DoorSplitUpcomingConcert.Id, Quantity = 1 });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");
        var purchase = await response.Content.ReadAsync<TicketPaymentResponse>();
        Assert.Empty(purchase!.TicketIds);

        var tickets = await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.DoorSplitUpcomingConcert.Id),
            timeout: TimeSpan.FromSeconds(15));

        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedData.DoorSplitUpcomingConcert.Id);

        var intent = await fixture.StripePaymentIntents.GetAsync(purchase.TransactionId);
        Assert.Equal(fixture.SeedData.VenueManager1.StripeAccountId, intent.TransferData.DestinationId);
    }

    [Fact]
    public async Task ShouldPayVenueManager_WhenVersusContract()
    {
        var response = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.VersusUpcomingConcert.Id, Quantity = 1 });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");
        var purchase = await response.Content.ReadAsync<TicketPaymentResponse>();
        Assert.Empty(purchase!.TicketIds);

        var tickets = await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.VersusUpcomingConcert.Id),
            timeout: TimeSpan.FromSeconds(15));

        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedData.VersusUpcomingConcert.Id);

        var intent = await fixture.StripePaymentIntents.GetAsync(purchase.TransactionId);
        Assert.Equal(fixture.SeedData.VenueManager1.StripeAccountId, intent.TransferData.DestinationId);
    }

    [Fact]
    public async Task ShouldPayArtistManager_WhenVenueHireContract()
    {
        var response = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = fixture.SeedData.VenueHireUpcomingConcert.Id, Quantity = 1 });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");
        var purchase = await response.Content.ReadAsync<TicketPaymentResponse>();
        Assert.Empty(purchase!.TicketIds);

        var tickets = await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == fixture.SeedData.VenueHireUpcomingConcert.Id),
            timeout: TimeSpan.FromSeconds(15));

        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedData.VenueHireUpcomingConcert.Id);

        // VenueHire: artist hired the venue, so ticket revenue routes to the artist manager
        var intent = await fixture.StripePaymentIntents.GetAsync(purchase.TransactionId);
        Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData.DestinationId);
    }
}
