using System.Net;
using Concertable.Application.DTOs;
using Concertable.Concert.Api.Responses;
using Concertable.Web.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.OpportunityApplication;

[Collection("Integration")]
public class OpportunityApplicationVenueHireApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public OpportunityApplicationVenueHireApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{fixture.SeedData.VenueHireApp.Id}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();
        var response = await client.PostAsync($"/api/OpportunityApplication/accept/{fixture.SeedData.VenueHireApp.Id}", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldConfirmBookingAndCreateDraftConcertAndNotifyArtist()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{fixture.SeedData.VenueHireApp.Id}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<OpportunityApplicationDto>($"/api/OpportunityApplication/{fixture.SeedData.VenueHireApp.Id}");
        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/application/{fixture.SeedData.VenueHireApp.Id}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
        var (userId, payload) = Assert.Single(fixture.NotificationService.DraftCreated);
        Assert.Equal(fixture.SeedData.ArtistManager.Id.ToString(), userId);
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Accept_ShouldIgnoreDuplicateWebhookEvent()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{fixture.SeedData.VenueHireApp.Id}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        Assert.Single(fixture.NotificationService.DraftCreated);
    }

    [Fact]
    public async Task Accept_ShouldNotConfirmBooking_WhenWebhookFails()
    {
        // Arrange
        fixture.CreateClient(fixture.SeedData.VenueManager1, o => o.UseFailingStripe());
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{fixture.SeedData.VenueHireApp.Id}", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<OpportunityApplicationDto>($"/api/OpportunityApplication/{fixture.SeedData.VenueHireApp.Id}");
        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
        Assert.Empty(fixture.NotificationService.DraftCreated);
    }

    [Fact]
    public async Task Accept_ShouldNotCreateDraft_WhenPaymentFails()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1, o => o.UseFailingPayment());

        // Act
        await client.PostAsync($"/api/OpportunityApplication/accept/{fixture.SeedData.VenueHireApp.Id}", (object?)null);

        // Assert
        var draft = await fixture.ReadDbContext.Concerts.FirstOrDefaultAsync(c => c.Booking.ApplicationId == fixture.SeedData.VenueHireApp.Id);
        Assert.Null(draft);
    }
}
