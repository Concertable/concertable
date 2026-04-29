using System.Net;
using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Api.Responses;
using Concertable.IntegrationTests.Common;
using Xunit;

namespace Concertable.Concert.IntegrationTests.Application;

[Collection("Integration")]
public class ApplicationVersusApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ApplicationVersusApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Accept_ShouldCreateDraftConcertAndNotifyArtist()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        var acceptResponse = await client.PostAsync($"/api/Application/accept/{fixture.SeedData.VersusApp.Id}", (object?)null);
        await acceptResponse.ShouldBe(HttpStatusCode.OK);

        // Assert
        var concert = await client.GetAssertAsync<ConcertDetailsResponse>($"/api/Concert/application/{fixture.SeedData.VersusApp.Id}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(fixture.SeedData.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }
}
