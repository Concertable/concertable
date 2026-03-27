using System.Net;
using Application.DTOs;
using Concertable.Web.IntegrationTests.Infrastructure;
using Core.Enums;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.ConcertApplication;

[Collection("Integration")]
public class ConcertApplicationFlatFeeApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertApplicationFlatFeeApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task AcceptApplicationAsync()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        await client.PostAsJsonEnsureSuccessAsync($"/api/ConcertApplication/accept/{TestConstants.FlatFee.ApplicationId}");
    }

    [Fact]
    public async Task Accept_ShouldSetStatusToAwaitingPayment()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var application = await client.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{TestConstants.FlatFee.ApplicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.AwaitingPayment, application.Status);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        await AcceptApplicationAsync();
        await fixture.StripeClient.SendWebhookAsync();

        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Settle_ShouldTransitionToSettled_AfterWebhookProcessed()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        await AcceptApplicationAsync();

        await fixture.StripeClient.SendWebhookAsync();

        var application = await client.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{TestConstants.FlatFee.ApplicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.Settled, application.Status);
    }

    [Fact]
    public async Task Settle_ShouldCreateDraftConcert_AfterWebhookProcessed()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        await AcceptApplicationAsync();

        await fixture.StripeClient.SendWebhookAsync();

        var concert = await client.GetAsync<ConcertDto>($"/api/Concert/application/{TestConstants.FlatFee.ApplicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
    }

    [Fact]
    public async Task Settle_ShouldNotifyArtist_AfterWebhookProcessed()
    {
        await AcceptApplicationAsync();

        await fixture.StripeClient.SendWebhookAsync();

        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Settle_ShouldIgnoreDuplicateWebhookEvent()
    {
        await AcceptApplicationAsync();

        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        Assert.Single(fixture.NotificationService.DraftCreated);
    }
}
