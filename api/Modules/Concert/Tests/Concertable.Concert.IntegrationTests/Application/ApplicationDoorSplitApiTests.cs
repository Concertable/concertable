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

public class ApplicationDoorSplitApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ApplicationDoorSplitApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AcceptCheckout_ShouldReturnDeferredDoorSharePaymentSession()
    {
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        var response = await client.PostAsync($"/api/Application/{fixture.SeedData.DoorSplitApp.Id}/checkout");

        await response.ShouldBe(HttpStatusCode.OK);
        var checkout = await response.Content.ReadAsync<Checkout>();
        Assert.NotNull(checkout);
        Assert.Equal(PaymentTiming.Deferred, checkout!.Timing);
        Assert.IsType<DoorSharePayment>(checkout.Amount);
        Assert.NotEmpty(checkout.Session.ClientSecret);
    }

    [Fact]
    public async Task ApplyCheckout_ShouldReturn400_WhenContractDoesNotSupportApplyTimeCheckout()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);

        var response = await client.PostAsync($"/api/Application/opportunity/{fixture.SeedData.DoorSplitApp.OpportunityId}/checkout");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldReturn400_WhenAlreadyAccepted()
    {
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        await client.PostAsync(
            $"/api/Application/{fixture.SeedData.DoorSplitApp.Id}/accept", new { paymentMethodId = "pm_test" });

        var response = await client.PostAsync(
            $"/api/Application/{fixture.SeedData.DoorSplitApp.Id}/accept", new { paymentMethodId = "pm_test" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accept_ShouldFail_WhenCardWillDecline()
    {
        // Arrange — verify-and-void is stubbed to decline
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1, o => o.UseDeclineAtVerify());

        // Act
        var response = await client.PostAsync(
            $"/api/Application/{fixture.SeedData.DoorSplitApp.Id}/accept", new { paymentMethodId = "pm_test" });

        // Assert — 400 returned, no DeferredBooking or concert draft created
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var concert = await fixture.ReadDbContext.Concerts
            .FirstOrDefaultAsync(c => c.Booking.ApplicationId == fixture.SeedData.DoorSplitApp.Id);
        Assert.Null(concert);
    }

    [Fact]
    public async Task Accept_ShouldCreateDraftConcertAndNotifyArtistAndVenue()
    {
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        var acceptResponse = await client.PostAsync(
            $"/api/Application/{fixture.SeedData.DoorSplitApp.Id}/accept", new { paymentMethodId = "pm_test" });
        await acceptResponse.ShouldBe(HttpStatusCode.OK);

        var application = await client.GetAsync<ApplicationResponse>(
            $"/api/Application/{fixture.SeedData.DoorSplitApp.Id}");

        Assert.Equal(ApplicationStatus.Accepted, application!.Status);

        var concert = await client.GetAssertAsync<ConcertDetailsResponse>(
            $"/api/Concert/application/{fixture.SeedData.DoorSplitApp.Id}");

        Assert.NotNull(concert);
        Assert.Null(concert.DatePosted);

        Assert.Equal(2, fixture.NotificationService.DraftCreated.Count);
        var notifiedUserIds = fixture.NotificationService.DraftCreated.Select(n => n.UserId).ToList();
        Assert.Contains(fixture.SeedData.ArtistManager.Id.ToString(), notifiedUserIds);
        Assert.Contains(fixture.SeedData.VenueManager1.Id.ToString(), notifiedUserIds);
        Assert.All(fixture.NotificationService.DraftCreated, n => Assert.NotNull(n.Payload));
    }
}