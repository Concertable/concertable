using System.Net;
using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;

using Concertable.Concert.Api.Responses;
using Concertable.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Concertable.Concert.IntegrationTests.Application;

[Collection("Integration")]

public class ApplicationFlatFeeApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ApplicationFlatFeeApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AcceptCheckout_ShouldReturnImmediateFlatPaymentSession()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        var response = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/checkout");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var checkout = await response.Content.ReadAsync<Checkout>();
        Assert.NotNull(checkout);
        Assert.Equal(PaymentTiming.Immediate, checkout!.Timing);
        Assert.IsType<FlatPayment>(checkout.Amount);
        Assert.NotEmpty(checkout.Session.ClientSecret);
    }

    [Fact]
    public async Task ApplyCheckout_ShouldReturn400_WhenContractDoesNotSupportApplyTimeCheckout()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);

        // Act
        var response = await client.PostAsync($"/api/Application/opportunity/{fixture.SeedData.FlatFeeApp.OpportunityId}/checkout");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        var firstResponse = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", new { paymentMethodId = "pm_card_visa" });
        await firstResponse.ShouldBe(HttpStatusCode.OK);
        await fixture.StripeClient.SendWebhookAsync();
        var response = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", new { paymentMethodId = "pm_card_visa" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldConfirmBookingAndCreateDraftConcertAndNotifyArtistAndVenue()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        var acceptResponse = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", new { paymentMethodId = "pm_card_visa" });
        await acceptResponse.ShouldBe(HttpStatusCode.OK);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<ApplicationResponse>($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}");
        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
        var concert = await client.GetAssertAsync<ConcertDetailsResponse>($"/api/Concert/application/{fixture.SeedData.FlatFeeApp.Id}");
        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);
        Assert.Equal(2, fixture.NotificationService.DraftCreated.Count);
        var notifiedUserIds = fixture.NotificationService.DraftCreated.Select(n => n.UserId).ToList();
        Assert.Contains(fixture.SeedData.ArtistManager.Id.ToString(), notifiedUserIds);
        Assert.Contains(fixture.SeedData.VenueManager1.Id.ToString(), notifiedUserIds);
        Assert.All(fixture.NotificationService.DraftCreated, n => Assert.NotNull(n.Payload));
    }

    [Fact]
    public async Task Accept_ShouldIgnoreDuplicateWebhookEvent()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        var acceptResponse = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", new { paymentMethodId = "pm_card_visa" });
        await acceptResponse.ShouldBe(HttpStatusCode.OK);
        await fixture.StripeClient.SendWebhookAsync();
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        Assert.Equal(2, fixture.NotificationService.DraftCreated.Count);
    }

    [Fact]
    public async Task Accept_ShouldNotConfirmBooking_WhenWebhookFails()
    {
        // Arrange
        fixture.CreateClient(fixture.SeedData.VenueManager1, o => o.UseFailingStripe());
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        var acceptResponse = await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", new { paymentMethodId = "pm_card_visa" });
        await acceptResponse.ShouldBe(HttpStatusCode.OK);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<ApplicationResponse>($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}");
        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
        Assert.Empty(fixture.NotificationService.DraftCreated);
    }

    [Fact]
    public async Task Accept_ShouldNotCreateDraft_WhenPaymentFails()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1, o => o.UseFailingPayment());

        // Act
        await client.PostAsync($"/api/Application/{fixture.SeedData.FlatFeeApp.Id}/accept", new { paymentMethodId = "pm_card_visa" });

        // Assert
        var draft = await fixture.ReadDbContext.Concerts.FirstOrDefaultAsync(c => c.Booking.ApplicationId == fixture.SeedData.FlatFeeApp.Id);
        Assert.Null(draft);
    }
}
