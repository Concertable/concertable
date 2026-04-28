using System.Net;
using Concertable.Concert.Application.DTOs;

using Concertable.Concert.Api.Responses;
using Concertable.Web.IntegrationTests.Infrastructure;
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
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        await client.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.DoorSplitApp.Id}",
            (object?)null);

        var response = await client.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.DoorSplitApp.Id}",
            (object?)null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldCreateDraftConcertAndNotifyArtist()
    {
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        var acceptResponse = await client.PostAsync(
            $"/api/OpportunityApplication/accept/{fixture.SeedData.DoorSplitApp.Id}",
            (object?)null);
        await acceptResponse.ShouldBe(HttpStatusCode.OK);

        var application = await client.GetAsync<OpportunityApplicationResponse>(
            $"/api/OpportunityApplication/{fixture.SeedData.DoorSplitApp.Id}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);

        var concert = await client.GetAssertAsync<ConcertDetailsResponse>(
            $"/api/Concert/application/{fixture.SeedData.DoorSplitApp.Id}");

        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);

        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(fixture.SeedData.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }
}