using System.Net;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.Ticket.TicketRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.Ticket;

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
        var request = BuildPurchaseRequest(TestConstants.PostedFlatFee.ConcertId);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Purchase_ShouldReturn403_WhenNotCustomer()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildPurchaseRequest(TestConstants.PostedFlatFee.ConcertId);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Purchase_ShouldReturn400_WhenConcertNotPosted()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Customer);
        var request = BuildPurchaseRequest(TestConstants.Settled.ConcertId);

        // Act
        var response = await client.PostAsync("/api/Ticket/purchase", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region CanPurchase

    [Fact]
    public async Task CanPurchase_ShouldReturn400_WhenConcertNotPosted()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Customer);

        // Act
        var response = await client.GetAsync($"/api/Ticket/can-purchase/{TestConstants.Settled.ConcertId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CanPurchase_ShouldReturn200_WhenValid()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Customer);

        // Act
        var response = await client.GetAsync($"/api/Ticket/can-purchase/{TestConstants.PostedFlatFee.ConcertId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<bool>();
        Assert.True(result);
    }

    #endregion
}
