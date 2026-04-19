using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Infrastructure.Data;
using Concertable.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Web.Extensions;

public static class E2EEndpointExtensions
{
    public static void MapE2EEndpoints(this WebApplication app)
    {
        app.MapPost("/e2e/reseed", async (IDbInitializer dbInitializer, SeedData seedData) =>
        {
            await dbInitializer.InitializeAsync();
            return Results.Ok(new SeedDataResponse
            {
                TestPassword = SeedData.TestPassword,
                Customer = new SeededUser { Email = seedData.Customer.Email },
                VenueManager1 = new SeededVenueManager { Email = seedData.VenueManager1.Email, StripeAccountId = seedData.VenueManager1.StripeAccountId },
                ArtistManager = new SeededArtistManager { Email = seedData.ArtistManager.Email, StripeAccountId = seedData.ArtistManager.StripeAccountId },
                PendingFlatFeeApp = new SeededOpportunityApplication { ApplicationId = seedData.FlatFeeApp.Id },
                PendingVenueHireApp = new SeededOpportunityApplication { ApplicationId = seedData.VenueHireApp.Id },
                PendingDoorSplitApp = new SeededOpportunityApplication { ApplicationId = seedData.DoorSplitApp.Id },
                PendingVersusApp = new SeededOpportunityApplication { ApplicationId = seedData.VersusApp.Id },
                PostedFlatFeeApp = new SeededOpportunityApplication { ApplicationId = seedData.PostedFlatFeeApp.Id, ConcertId = seedData.PostedFlatFeeApp.Booking!.Concert!.Id },
                PostedDoorSplitApp = new SeededOpportunityApplication { ApplicationId = seedData.PostedDoorSplitApp.Id, ConcertId = seedData.PostedDoorSplitApp.Booking!.Concert!.Id },
                PostedVersusApp = new SeededOpportunityApplication { ApplicationId = seedData.PostedVersusApp.Id, ConcertId = seedData.PostedVersusApp.Booking!.Concert!.Id },
                PostedVenueHireApp = new SeededOpportunityApplication { ApplicationId = seedData.PostedVenueHireApp.Id, ConcertId = seedData.PostedVenueHireApp.Booking!.Concert!.Id },
                FinishedDoorSplitApp = new SeededOpportunityApplication { ApplicationId = seedData.FinishedDoorSplitApp.Id, ConcertId = seedData.FinishedDoorSplitApp.Booking!.Concert!.Id },
                FinishedVersusApp = new SeededOpportunityApplication { ApplicationId = seedData.FinishedVersusApp.Id, ConcertId = seedData.FinishedVersusApp.Booking!.Concert!.Id },
            });
        });

        app.MapPost("/e2e/finish/{concertId:int}", async (int concertId, IFinishedDispatcher finishedDispatcher, ApplicationDbContext db) =>
        {
            var result = await finishedDispatcher.FinishedAsync(concertId);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors.Select(e => e.Message));

            var bookingId = await db.ConcertBookings
                .Where(b => b.Concert!.Id == concertId)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            var paymentIntentId = await db.SettlementTransactions
                .Where(t => t.BookingId == bookingId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.PaymentIntentId)
                .FirstOrDefaultAsync();

            return paymentIntentId is not null
                ? Results.Ok(paymentIntentId)
                : Results.Ok();
        });

        app.MapGet("/e2e/payment-intent/{applicationId:int}", async (int applicationId, ApplicationDbContext db) =>
        {
            var bookingId = await db.ConcertBookings
                .Where(b => b.ApplicationId == applicationId)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            var paymentIntentId = await db.SettlementTransactions
                .Where(t => t.BookingId == bookingId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.PaymentIntentId)
                .FirstOrDefaultAsync();

            return paymentIntentId is not null
                ? Results.Ok(paymentIntentId)
                : Results.NotFound("No settlement transaction found for this application");
        });
    }
}
