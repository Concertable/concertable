using System.Net;
using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Requests;
using Concertable.Concert.Application.Responses;
using Concertable.Concert.Api.Responses;
using Concertable.Concert.Domain;
using Concertable.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Concertable.Concert.IntegrationTests.Application;

[Collection("Integration")]

public class ApplicationVenueHireApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ApplicationVenueHireApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ApplyCheckout_ShouldReturnAuthorizeFlatPaymentSession()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);

        // Act
        var response = await client.PostAsync($"/api/Application/opportunity/{fixture.SeedData.VenueHireApp.OpportunityId}/checkout");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var checkout = await response.Content.ReadAsync<Checkout>();
        Assert.NotNull(checkout);
        Assert.Equal(PaymentTiming.Authorize, checkout!.Timing);
        Assert.IsType<FlatPayment>(checkout.Amount);
        Assert.NotEmpty(checkout.Session.ClientSecret);
    }

    [Fact]
    public async Task AcceptCheckout_ShouldReturn400_WhenContractDoesNotSupportAcceptTimeCheckout()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        var response = await client.PostAsync($"/api/Application/{fixture.SeedData.VenueHireApp.Id}/checkout");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ApplyCheckoutThenApply_ShouldStorePaymentMethodOnPrepaidApplication()
    {
        // Arrange — venue manager creates a fresh VenueHire opportunity
        var venueClient = fixture.CreateClient(fixture.SeedData.VenueManager1);
        var oppRequest = new OpportunityRequest
        {
            StartDate = DateTime.UtcNow.AddMonths(13),
            EndDate = DateTime.UtcNow.AddMonths(13).AddHours(3),
            GenreIds = [fixture.SeedData.Rock.Id],
            Contract = new VenueHireContract { PaymentMethod = PaymentMethod.Cash, HireFee = 250m }
        };
        var oppResponse = await venueClient.PostAsync("/api/Opportunity", oppRequest);
        var opportunity = await oppResponse.Content.ReadAsync<OpportunityResponse>();

        // Act — artist runs apply-checkout, then applies with the PM
        var artistClient = fixture.CreateClient(fixture.SeedData.ArtistManager);
        var checkoutResponse = await artistClient.PostAsync($"/api/Application/opportunity/{opportunity!.Id}/checkout");
        await checkoutResponse.ShouldBe(HttpStatusCode.OK);

        var applyResponse = await artistClient.PostAsync($"/api/Application/{opportunity.Id}", new { paymentMethodId = "pm_card_visa" });
        await applyResponse.ShouldBe(HttpStatusCode.Created);

        // Assert — a PrepaidApplication was created with the supplied PM
        var prepaid = await fixture.ReadDbContext.Applications
            .OfType<PrepaidApplication>()
            .FirstOrDefaultAsync(a => a.OpportunityId == opportunity.Id);
        Assert.NotNull(prepaid);
        Assert.Equal("pm_card_visa", prepaid!.PaymentMethodId);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        await client.PostAsync($"/api/Application/{fixture.SeedData.VenueHireApp.Id}/accept", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();
        var response = await client.PostAsync($"/api/Application/{fixture.SeedData.VenueHireApp.Id}/accept", (object?)null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldConfirmBookingAndCreateDraftConcertAndNotifyArtistAndVenue()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        // Act
        await client.PostAsync($"/api/Application/{fixture.SeedData.VenueHireApp.Id}/accept", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<ApplicationResponse>($"/api/Application/{fixture.SeedData.VenueHireApp.Id}");
        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
        var concert = await client.GetAsync<ConcertDetailsResponse>($"/api/Concert/application/{fixture.SeedData.VenueHireApp.Id}");
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
        await client.PostAsync($"/api/Application/{fixture.SeedData.VenueHireApp.Id}/accept", (object?)null);
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
        await client.PostAsync($"/api/Application/{fixture.SeedData.VenueHireApp.Id}/accept", (object?)null);
        await fixture.StripeClient.SendWebhookAsync();

        // Assert
        var application = await client.GetAsync<ApplicationResponse>($"/api/Application/{fixture.SeedData.VenueHireApp.Id}");
        Assert.Equal(ApplicationStatus.Accepted, application!.Status);
        Assert.Empty(fixture.NotificationService.DraftCreated);
    }

    [Fact]
    public async Task Accept_ShouldNotCreateDraft_WhenPaymentFails()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1, o => o.UseFailingPayment());

        // Act
        await client.PostAsync($"/api/Application/{fixture.SeedData.VenueHireApp.Id}/accept", (object?)null);

        // Assert
        var draft = await fixture.ReadDbContext.Concerts.FirstOrDefaultAsync(c => c.Booking.ApplicationId == fixture.SeedData.VenueHireApp.Id);
        Assert.Null(draft);
    }

    [Fact]
    public async Task Apply_ShouldFail_WhenCardWillDecline()
    {
        // Arrange — venue manager creates a fresh VenueHire opportunity
        var venueClient = fixture.CreateClient(fixture.SeedData.VenueManager1);
        var oppRequest = new OpportunityRequest
        {
            StartDate = DateTime.UtcNow.AddMonths(13),
            EndDate = DateTime.UtcNow.AddMonths(13).AddHours(3),
            GenreIds = [fixture.SeedData.Rock.Id],
            Contract = new VenueHireContract { PaymentMethod = PaymentMethod.Cash, HireFee = 250m }
        };
        var oppResponse = await venueClient.PostAsync("/api/Opportunity", oppRequest);
        var opportunity = await oppResponse.Content.ReadAsync<OpportunityResponse>();

        // Act — artist applies; verify-and-void is stubbed to decline
        var artistClient = fixture.CreateClient(fixture.SeedData.ArtistManager, o => o.UseDeclineAtVerify());
        var applyResponse = await artistClient.PostAsync($"/api/Application/{opportunity!.Id}", new { paymentMethodId = "pm_card_visa" });

        // Assert — 400 returned, no PrepaidApplication row created
        Assert.Equal(HttpStatusCode.BadRequest, applyResponse.StatusCode);
        var prepaid = await fixture.ReadDbContext.Applications
            .OfType<PrepaidApplication>()
            .FirstOrDefaultAsync(a => a.OpportunityId == opportunity.Id);
        Assert.Null(prepaid);
    }
}
