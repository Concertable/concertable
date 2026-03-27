using System.Net;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.ConcertApplication;

[Collection("Integration")]
public class ConcertApplicationApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertApplicationApiTests(ApiFixture fixture)
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
        var client = fixture.CreateClient(TestConstants.ArtistManager);

        // Act
        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenCalledByDifferentVenueManager()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager2);

        // Act
        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
