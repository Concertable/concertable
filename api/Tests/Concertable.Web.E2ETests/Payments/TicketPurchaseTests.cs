using Concertable.Application.DTOs;
using Concertable.Application.Responses;
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
        customerClient = await fixture.CreateAuthenticatedClientAsync(E2ETestConstants.Customer1.Email, E2ETestConstants.TestPassword);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldPayVenueManager_WhenFlatFeeContract()
    {
        // Act
        var response = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = E2ETestConstants.PostedConcert.ConcertId, Quantity = 1 });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");
        var purchase = await response.Content.ReadAsync<TicketPaymentResponse>();
        Assert.Empty(purchase!.TicketIds);

        // Wait for webhook to fire and complete the ticket
        var tickets = await fixture.Polling.UntilAsync(
            async () => await customerClient.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user"),
            t => t.Any(ticket => ticket.Concert.Id == E2ETestConstants.PostedConcert.ConcertId),
            timeout: TimeSpan.FromSeconds(15));

        // Assert ticket issued
        Assert.Contains(tickets, t => t.Concert.Id == E2ETestConstants.PostedConcert.ConcertId);

        // Assert payment routed to venue manager
        var intent = await fixture.StripePaymentIntents.GetAsync(purchase.TransactionId);
        Assert.Equal(E2ETestConstants.VenueManager1.StripeAccountId, intent.TransferData.DestinationId);
    }
}
