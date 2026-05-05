using System.Net;
using Concertable.IntegrationTests.Common;
using Xunit;
using static Concertable.Concert.IntegrationTests.Ticket.TicketRequestBuilders;

namespace Concertable.Concert.IntegrationTests.Ticket;

[Collection("Integration")]

public class TicketApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public TicketApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region Purchase

    [Fact]
    public async Task Purchase_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();
        var request = BuildPurchaseRequest(fixture.SeedData.PostedFlatFeeBooking.Concert!.Id);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Purchase_ShouldReturn403_WhenNotCustomer()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);
        var request = BuildPurchaseRequest(fixture.SeedData.PostedFlatFeeBooking.Concert!.Id);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Purchase_ShouldReturn400_WhenConcertNotPosted()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);
        var request = BuildPurchaseRequest(fixture.SeedData.ConfirmedBooking.Concert!.Id);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        await response.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region CanPurchase

    [Fact]
    public async Task CanPurchase_ShouldReturnFalse_WhenConcertNotPosted()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        // Act
        var response = await client.GetAsync($"/api/Ticket/concert/{fixture.SeedData.ConfirmedBooking.Concert!.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadAsync<bool>();
        Assert.False(result);
    }

    [Fact]
    public async Task CanPurchase_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        // Act
        var response = await client.GetAsync($"/api/Ticket/concert/{fixture.SeedData.PostedFlatFeeBooking.Concert!.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadAsync<bool>();
        Assert.True(result);
    }

    #endregion
}
