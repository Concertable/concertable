using System.Net;
using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Application.Responses;
using Concertable.Concert.Api.Responses;
using Concertable.Application.Interfaces.Payment;
using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Core.Enums;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.Ticket.TicketRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.Ticket;

[Collection("Integration")]
public class TicketVenueHireApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public TicketVenueHireApiTests(ApiFixture fixture)
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
        var request = BuildPurchaseRequest(fixture.SeedData.PostedVenueHireBooking.Concert!.Id);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);
        var result = await response.Content.ReadAsync<TicketPaymentResponse>();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.NotNull(result!.TransactionId);
        var tickets = await client.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user");
        Assert.Single(tickets!);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/{fixture.SeedData.PostedVenueHireBooking.Concert!.Id}");
        Assert.Equal(99, concert!.AvailableTickets);
        var (userId, _) = Assert.Single(fixture.NotificationService.TicketPurchased);
        Assert.Equal(fixture.SeedData.Customer.Id.ToString(), userId);
        Assert.Equal(fixture.SeedData.Customer.Id.ToString(), fixture.StripePaymentClient.LastMetadata["fromUserId"]);
        Assert.Equal(fixture.SeedData.ArtistManager.Id.ToString(), fixture.StripePaymentClient.LastMetadata["toUserId"]);
        var transactions = await client.GetAsync<Pagination<ITransaction>>("/api/Transaction");
        var transaction = Assert.Single(transactions!.Data);
        Assert.Equal(TransactionStatus.Complete, transaction.Status);
        Assert.Equal(fixture.SeedData.Customer.Id, transaction.FromUserId);
        Assert.Equal(fixture.SeedData.ArtistManager.Id, transaction.ToUserId);
    }

    [Fact]
    public async Task Purchase_ShouldIgnoreDuplicateWebhookEvent()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);
        var request = BuildPurchaseRequest(fixture.SeedData.PostedVenueHireBooking.Concert!.Id);

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
    public async Task Purchase_ShouldReturnError_WhenPaymentFails()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer, o => o.UseFailingPayment());
        var request = BuildPurchaseRequest(fixture.SeedData.PostedVenueHireBooking.Concert!.Id);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Empty(fixture.NotificationService.TicketPurchased);
        var tickets = await client.GetAsync<IEnumerable<TicketDto>>("/api/Ticket/upcoming/user");
        Assert.Empty(tickets!);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/{fixture.SeedData.PostedVenueHireBooking.Concert!.Id}");
        Assert.Equal(100, concert!.AvailableTickets);
        var transactions = await client.GetAsync<Pagination<ITransaction>>("/api/Transaction");
        Assert.Empty(transactions!.Data);
    }
}
