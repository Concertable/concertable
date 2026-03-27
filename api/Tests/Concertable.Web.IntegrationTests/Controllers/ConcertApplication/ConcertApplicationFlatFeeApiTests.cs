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
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var application = await client.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{TestConstants.FlatFee.ApplicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.AwaitingPayment, application.Status);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);
        await AcceptApplicationAsync();
        await fixture.StripeClient.SendWebhookAsync();

        // Act
        var response = await client.PostAsync($"/api/ConcertApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Settle_ShouldTransitionToSettled_AfterWebhookProcessed()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);
        await AcceptApplicationAsync();

        // Act
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{TestConstants.FlatFee.ApplicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.Settled, application.Status);
    }

    [Fact]
    public async Task Settle_ShouldCreateDraftConcert_AfterWebhookProcessed()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);
        await AcceptApplicationAsync();

        // Act
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var concert = await client.GetAsync<ConcertDto>($"/api/Concert/application/{TestConstants.FlatFee.ApplicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
    }

    [Fact]
    public async Task Settle_ShouldNotifyArtist_AfterWebhookProcessed()
    {
        // Arrange
        await AcceptApplicationAsync();

        // Act
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Settle_ShouldIgnoreDuplicateWebhookEvent()
    {
        // Arrange
        await AcceptApplicationAsync();

        // Act
        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        Assert.Single(fixture.NotificationService.DraftCreated);
    }
}
