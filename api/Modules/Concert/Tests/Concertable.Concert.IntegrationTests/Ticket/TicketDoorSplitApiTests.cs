using System.Net;
using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Api.Responses;
using Concertable.Payment.Application.DTOs;
using Concertable.IntegrationTests.Common;
using Xunit;
using static Concertable.Concert.IntegrationTests.Ticket.TicketRequestBuilders;

namespace Concertable.Concert.IntegrationTests.Ticket;

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
        var client = fixture.CreateClient(fixture.SeedData.Customer);
        var request = BuildPurchaseRequest(fixture.SeedData.PostedDoorSplitBooking.Concert!.Id);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);
        var result = await response.Content.ReadAsync<TicketPaymentResponse>();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.NotNull(result!.TransactionId);
        var tickets = await client.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user");
        Assert.Single(tickets!);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/{fixture.SeedData.PostedDoorSplitBooking.Concert!.Id}");
        Assert.Equal(99, concert!.AvailableTickets);
        var (userId, _) = Assert.Single(fixture.NotificationService.TicketPurchased);
        Assert.Equal(fixture.SeedData.Customer.Id.ToString(), userId);
        Assert.Equal(fixture.SeedData.Customer.Id.ToString(), fixture.StripeApiClient.LastMetadata["fromUserId"]);
        Assert.Equal(fixture.SeedData.VenueManager1.Id.ToString(), fixture.StripeApiClient.LastMetadata["toUserId"]);
        var transactions = await client.GetAsync<Pagination<TicketTransactionDto>>("/api/Transaction");
        var transaction = Assert.Single(transactions!.Data);
        Assert.Equal(TransactionStatus.Complete, transaction.Status);
        Assert.Equal(fixture.SeedData.Customer.Id, transaction.FromUserId);
        Assert.Equal(fixture.SeedData.VenueManager1.Id, transaction.ToUserId);
    }

    [Fact]
    public async Task Purchase_ShouldIgnoreDuplicateWebhookEvent()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);
        var request = BuildPurchaseRequest(fixture.SeedData.PostedDoorSplitBooking.Concert!.Id);

        // Act
        await client.PostAsync("/api/Ticket/purchase", request);
        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        Assert.Single(fixture.NotificationService.TicketPurchased);
        var transactions = await client.GetAsync<Pagination<TicketTransactionDto>>("/api/Transaction");
        Assert.Single(transactions!.Data);
    }

    [Fact]
    public async Task Purchase_ShouldReturnError_WhenPaymentFails()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer, o => o.UseFailingPayment());
        var request = BuildPurchaseRequest(fixture.SeedData.PostedDoorSplitBooking.Concert!.Id);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        await response.ShouldBe(HttpStatusCode.BadRequest);
        Assert.Empty(fixture.NotificationService.TicketPurchased);
        var tickets = await client.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user");
        Assert.Empty(tickets!);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/{fixture.SeedData.PostedDoorSplitBooking.Concert!.Id}");
        Assert.Equal(100, concert!.AvailableTickets);
        var transactions = await client.GetAsync<Pagination<TicketTransactionDto>>("/api/Transaction");
        Assert.Empty(transactions!.Data);
    }
}
