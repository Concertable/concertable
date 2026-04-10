using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.OpportunityApplication;

[Collection("Integration")]
public class OpportunityApplicationVersusApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public OpportunityApplicationVersusApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Accept_ShouldCreateDraftConcertAndNotifyArtist()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.Versus.ApplicationId}", (object?)null);

        // Assert
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/application/{TestConstants.Versus.ApplicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }
}
