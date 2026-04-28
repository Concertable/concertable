using System.Net;
using Concertable.Concert.Application.DTOs;
using Concertable.User.Application.Requests;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.User;

[Collection("Integration")]
public class UserApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public UserApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region UpdateLocation

    [Fact]
    public async Task UpdateLocation_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PutAsync("/api/Users/location", new UpdateLocationRequest(51.5, -0.1));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLocation_ShouldReturn200_WhenAuthenticated()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        // Act
        var response = await client.PutAsync("/api/Users/location", new UpdateLocationRequest(51.5, -0.1));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadAsync<CustomerDto>();
        Assert.NotNull(user);
        Assert.Equal(fixture.SeedData.Customer.Id, user.Id);
    }

    [Fact]
    public async Task UpdateLocation_ShouldPersistCoordinates()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);
        const double latitude = 53.4808;
        const double longitude = -2.2426;

        // Act
        var response = await client.PutAsync("/api/Users/location", new UpdateLocationRequest(latitude, longitude));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadAsync<CustomerDto>();
        Assert.NotNull(user);
        Assert.NotNull(user.Latitude);
        Assert.NotNull(user.Longitude);
    }

    #endregion
}
