using System.Net;
using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Core.Enums;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.OpportunityApplication;

[Collection("Integration")]
public class OpportunityApplicationDoorSplitApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public OpportunityApplicationDoorSplitApiTests(ApiFixture fixture)
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
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.DoorSplit.ApplicationId}", (object?)null);
        var response = await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.DoorSplit.ApplicationId}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldCreateDraftConcertAndNotifyArtist()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.DoorSplit.ApplicationId}", (object?)null);

        // Assert
        var application = await client.GetAsync<OpportunityApplicationDto>($"/api/OpportunityApplication/{TestConstants.DoorSplit.ApplicationId}");
        Assert.Equal(ApplicationStatus.AwaitingPayment, application!.Status);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/application/{TestConstants.DoorSplit.ApplicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }
}
