using Concertable.Application.DTOs;
using Concertable.Web.E2ETests.Infrastructure;
using Xunit;

namespace Concertable.Web.E2ETests.Payments;

[Collection("E2E")]
public class TicketPurchaseTests : IAsyncLifetime
{
    private readonly AppFixture fixture;

    public TicketPurchaseTests(AppFixture fixture)
    {
        this.fixture = fixture;
    }

    private HttpClient customerClient = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
        customerClient = await fixture.CreateAuthenticatedClientAsync(E2ETestConstants.Customer1.Email, E2ETestConstants.TestPassword);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Purchase_ShouldIssueTicket_WhenPaymentSucceeds()
    {
        // Act
        var response = await customerClient.PostAsync(
            "/api/Ticket/purchase",
            new { ConcertId = E2ETestConstants.PostedConcert.ConcertId, Quantity = 1 });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");
        var purchase = await response.Content.ReadAsync<TicketPurchaseDto>();
        Assert.True(purchase!.Success, $"Purchase failed: {body}");
        Assert.Empty(purchase.TicketIds);

        // Wait for webhook to fire and complete the ticket
        var tickets = await customerClient.PollUntilAsync<IEnumerable<TicketDto>>(
            "/api/Ticket/upcoming/user",
            t => t.Any(ticket => ticket.Concert.Id == E2ETestConstants.PostedConcert.ConcertId),
            timeout: TimeSpan.FromSeconds(15));

        // Assert
        Assert.Contains(tickets, t => t.Concert.Id == E2ETestConstants.PostedConcert.ConcertId);
    }
}
