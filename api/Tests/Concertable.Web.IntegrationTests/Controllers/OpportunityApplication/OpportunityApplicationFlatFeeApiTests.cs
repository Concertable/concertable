using System.Net;
using Concertable.Application.DTOs;
using Concertable.Web.Responses;
using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Core.Enums;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.OpportunityApplication;

[Collection("Integration")]
public class OpportunityApplicationFlatFeeApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public OpportunityApplicationFlatFeeApiTests(ApiFixture fixture)
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
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();
        var response = await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldSettleAndCreateDraftConcertAndNotifyArtist()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<OpportunityApplicationDto>($"/api/OpportunityApplication/{TestConstants.FlatFee.ApplicationId}");
        Assert.Equal(ApplicationStatus.Settled, application!.Status);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/application/{TestConstants.FlatFee.ApplicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Accept_ShouldIgnoreDuplicateWebhookEvent()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        Assert.Single(fixture.NotificationService.DraftCreated);
    }

    [Fact]
    public async Task Accept_ShouldNotSettle_WhenWebhookFails()
    {
        // Arrange
        fixture.CreateClient(TestConstants.VenueManager, o => o.UseFailingStripe());
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<OpportunityApplicationDto>($"/api/OpportunityApplication/{TestConstants.FlatFee.ApplicationId}");
        Assert.Equal(ApplicationStatus.AwaitingPayment, application!.Status);
        Assert.Empty(fixture.NotificationService.DraftCreated);
    }

    [Fact]
    public async Task Accept_ShouldNotSettle_WhenPaymentFails()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager, o => o.UseFailingPayment());

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{TestConstants.FlatFee.ApplicationId}", (object?)null);

        // Assert
        var application = await fixture.CreateClient(TestConstants.VenueManager)
            .GetAsync<OpportunityApplicationDto>($"/api/OpportunityApplication/{TestConstants.FlatFee.ApplicationId}");
        Assert.Equal(ApplicationStatus.AwaitingPayment, application!.Status);
        Assert.Empty(fixture.NotificationService.DraftCreated);
    }
}
