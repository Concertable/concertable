using System.Net;
using Concertable.IntegrationTests.Common;
using Xunit;

namespace Concertable.Concert.IntegrationTests.Application;

[Collection("Integration")]

public class ApplicationApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ApplicationApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region Accept

    [Fact]
    public async Task Accept_ShouldReturn403_WhenNotVenueManager()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);

        // Act
        var response = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenCalledByDifferentVenueManager()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager2);

        // Act
        var response = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
