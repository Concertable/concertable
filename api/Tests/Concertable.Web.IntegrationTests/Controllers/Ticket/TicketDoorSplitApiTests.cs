using Concertable.Application.DTOs;
using Concertable.Application.Results;
using Concertable.Web.Responses;
using Concertable.Application.Interfaces.Payment;
using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Core.Enums;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.Ticket.TicketRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.Ticket;

[Collection("Integration")]
public class TicketDoorSplitApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public TicketDoorSplitApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Purchase_ShouldCreateTicketAndReduceAvailabilityAfterWebhook()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Customer);
        var request = BuildPurchaseRequest(TestConstants.PostedDoorSplit.ConcertId);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);
        var result = await response.Content.ReadAsync<TicketPurchaseDto>();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        Assert.True(result!.Success);
        Assert.NotNull(result.TransactionId);
        var tickets = await client.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user");
        Assert.Single(tickets!);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/{TestConstants.PostedDoorSplit.ConcertId}");
        Assert.Equal(99, concert!.AvailableTickets);
        var (userId, _) = Assert.Single(fixture.NotificationService.TicketPurchased);
        Assert.Equal(TestConstants.Customer.Id.ToString(), userId);
        Assert.Equal(TestConstants.Customer.Id.ToString(), fixture.StripePaymentClient.LastMetadata["fromUserId"]);
        Assert.Equal(TestConstants.VenueManager.Id.ToString(), fixture.StripePaymentClient.LastMetadata["toUserId"]);
        var transactions = await client.GetAsync<Pagination<ITransaction>>("/api/Transaction");
        var transaction = Assert.Single(transactions!.Data);
        Assert.Equal(TransactionStatus.Complete, transaction.Status);
        Assert.Equal(TestConstants.Customer.Id, transaction.FromUserId);
        Assert.Equal(TestConstants.VenueManager.Id, transaction.ToUserId);
    }

    [Fact]
    public async Task Purchase_ShouldIgnoreDuplicateWebhookEvent()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Customer);
        var request = BuildPurchaseRequest(TestConstants.PostedDoorSplit.ConcertId);

        // Act
        await client.PostAsync("/api/Ticket/purchase", request);
        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        Assert.Single(fixture.NotificationService.TicketPurchased);
        var transactions = await client.GetAsync<Pagination<ITransaction>>("/api/Transaction");
        Assert.Single(transactions!.Data);
    }

    [Fact]
    public async Task Purchase_ShouldReturn200_WithFailureResponse_WhenPaymentFails()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Customer, o => o.UseFailingPayment());
        var request = BuildPurchaseRequest(TestConstants.PostedDoorSplit.ConcertId);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        var result = await response.Content.ReadAsync<TicketPurchaseDto>();
        Assert.False(result!.Success);
        Assert.Empty(fixture.NotificationService.TicketPurchased);
        var tickets = await client.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user");
        Assert.Empty(tickets!);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/{TestConstants.PostedDoorSplit.ConcertId}");
        Assert.Equal(100, concert!.AvailableTickets);
        var transactions = await client.GetAsync<Pagination<ITransaction>>("/api/Transaction");
        Assert.Empty(transactions!.Data);
    }
}
