using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Requests;
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
        var (_, applicationId) = await CreatePendingApplicationAsync();
        var artistClient = fixture.CreateClient(TestConstants.ArtistManager);

        var response = await artistClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenCalledByDifferentVenueManager()
    {
        var (_, applicationId) = await CreatePendingApplicationAsync();
        var otherClient = fixture.CreateClient(TestConstants.VenueManager2);

        var response = await otherClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldSetStatusToAwaitingPayment()
    {
        var (venueClient, applicationId) = await CreatePendingApplicationAsync();

        var response = await venueClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var application = await venueClient.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{applicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.AwaitingPayment, application.Status);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        var (venueClient, applicationId) = await CreatePendingApplicationAsync();

        await venueClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);
        await TriggerWebhookAsync(venueClient);

        var response = await venueClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #region Webhook Settlement

    [Fact]
    public async Task Accept_ShouldTransitionToSettled_AfterWebhookProcessed()
    {
        var (venueClient, applicationId) = await CreatePendingApplicationAsync();

        await venueClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);
        await TriggerWebhookAsync(venueClient);

        var application = await venueClient.GetAsync<ConcertApplicationDto>($"/api/ConcertApplication/{applicationId}");
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.Settled, application.Status);
    }

    [Fact]
    public async Task Accept_ShouldCreateDraftConcert_AfterWebhookProcessed()
    {
        var (venueClient, applicationId) = await CreatePendingApplicationAsync();

        await venueClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);
        await TriggerWebhookAsync(venueClient);

        var concert = await venueClient.GetAsync<ConcertDto>($"/api/Concert/application/{applicationId}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
    }

    [Fact]
    public async Task Accept_ShouldSendDraftCreatedNotification_AfterWebhookProcessed()
    {
        fixture.NotificationService.Reset();
        var (venueClient, applicationId) = await CreatePendingApplicationAsync();

        await venueClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);
        await TriggerWebhookAsync(venueClient);

        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(TestConstants.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Accept_ShouldIgnoreDuplicateWebhookEvent()
    {
        fixture.NotificationService.Reset();
        var (venueClient, applicationId) = await CreatePendingApplicationAsync();

        await venueClient.PostAsync($"/api/ConcertApplication/accept/{applicationId}", (object?)null);
        await TriggerWebhookAsync(venueClient);
        await TriggerWebhookAsync(venueClient);

        Assert.Single(fixture.NotificationService.DraftCreated);
    }

    #endregion

    #endregion

    #region Helpers

    private async Task<(HttpClient venueClient, int applicationId)> CreatePendingApplicationAsync()
    {
        var venueClient = fixture.CreateClient(TestConstants.VenueManager);
        var artistClient = fixture.CreateClient(TestConstants.ArtistManager);

        var opportunity = await venueClient.PostAsync<ConcertOpportunityRequest, ConcertOpportunityDto>(
            "/api/ConcertOpportunity",
            new ConcertOpportunityRequest
            {
                StartDate = DateTime.UtcNow.AddMonths(2),
                EndDate = DateTime.UtcNow.AddMonths(2).AddHours(3),
                GenreIds = [TestConstants.RockGenreId],
                Contract = new FlatFeeContractDto { PaymentMethod = PaymentMethod.Cash, Fee = 500 }
            });

        await artistClient.PostAsync($"/api/ConcertApplication/{opportunity!.Id}", (object?)null);

        var applications = await venueClient.GetAsync<IEnumerable<ConcertApplicationDto>>(
            $"/api/ConcertApplication/all/{opportunity.Id}");

        var application = Assert.Single(applications!);
        return (venueClient, application.Id);
    }

    private Task TriggerWebhookAsync(HttpClient client)
    {
        var metadata = fixture.StripeClient.LastMetadata;
        var metadataJson = string.Join(",\n", metadata.Select(kv => $"\"{kv.Key}\": \"{kv.Value}\""));

        var json = $$"""
        {
            "id": "evt_test_{{Guid.NewGuid():N}}",
            "object": "event",
            "type": "payment_intent.succeeded",
            "data": {
                "object": {
                    "id": "{{fixture.StripeClient.LastPaymentIntentId}}",
                    "object": "payment_intent",
                    "status": "succeeded",
                    "metadata": { {{metadataJson}} }
                }
            }
        }
        """;

        return client.PostWebhookAsync(json);
    }

    #endregion
}
