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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<bool>();
        Assert.True(result);
    }

    #endregion
}
