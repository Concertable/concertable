using System.Net;
using Application.DTOs;
using Concertable.Web.IntegrationTests.Infrastructure;
using Core.Enums;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.ConcertApplication;

[Collection("Integration")]
public class ConcertApplicationDoorSplitApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertApplicationDoorSplitApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.DoorSplit.ApplicationId}", (object?)null);
        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.DoorSplit.ApplicationId}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldCreateDraftConcertAndNotifyArtist()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.DoorSplit.ApplicationId}", (object?)null);

        // Assert
        var application = await client.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{TestConstants.DoorSplit.ApplicationId}");
        Assert.Equal(ApplicationStatus.AwaitingPayment, application!.Status);
        var concert = await client.GetAsync<ConcertDto>($"/api/Concert/application/{TestConstants.DoorSplit.ApplicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }
}
