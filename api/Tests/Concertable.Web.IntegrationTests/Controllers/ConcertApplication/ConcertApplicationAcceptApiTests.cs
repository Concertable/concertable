using System.Net;
using Application.DTOs;
using Concertable.Web.IntegrationTests.Infrastructure;
using Core.Enums;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.ConcertApplication;

[Collection("Integration")]
public class ConcertApplicationAcceptFlatFeeTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertApplicationAcceptFlatFeeTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region Accept

    [Fact]
    public async Task Accept_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.ArtistManager);

        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenCalledByDifferentVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2);

        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldSetStatusToAwaitingPayment()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var application = await client.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{TestConstants.PendingApplicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.AwaitingPayment, application.Status);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        await client.PostAsJsonEnsureSuccessAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}");
        await fixture.StripeClient.SendWebhookAsync();

        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #region Webhook Settlement

    [Fact]
    public async Task Accept_ShouldTransitionToSettled_AfterWebhookProcessed()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        await client.PostAsJsonEnsureSuccessAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}");
        await fixture.StripeClient.SendWebhookAsync();

        var application = await client.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{TestConstants.PendingApplicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.Settled, application.Status);
    }

    [Fact]
    public async Task Accept_ShouldCreateDraftConcert_AndNotifyArtist_AfterWebhookProcessed()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        await client.PostAsJsonEnsureSuccessAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}");
        await fixture.StripeClient.SendWebhookAsync();

        var concert = await client.GetAsync<ConcertDto>($"/api/Concert/application/{TestConstants.PendingApplicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);

        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Accept_ShouldIgnoreDuplicateWebhookEvent()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        await client.PostAsJsonEnsureSuccessAsync($"/api/ConcertApplication/accept/{TestConstants.PendingApplicationId}");
        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        Assert.Single(fixture.NotificationService.DraftCreated);
    }

    #endregion

    #endregion
}
