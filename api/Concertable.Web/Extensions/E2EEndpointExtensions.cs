using Concertable.Application.Interfaces;
using Concertable.Concert.Application.Interfaces;
using Concertable.Data.Application;
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
                VenueManager1 = new SeededVenueManager { Email = seedData.VenueManager1.Email, StripeAccountId = seedData.VenueManager1StripeAccountId },
                ArtistManager = new SeededArtistManager { Email = seedData.ArtistManager.Email, StripeAccountId = seedData.ArtistManagerStripeAccountId },
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
                UpcomingFlatFeeApp = new SeededOpportunityApplication { ApplicationId = seedData.UpcomingFlatFeeApp.Id, ConcertId = seedData.UpcomingFlatFeeApp.Booking!.Concert!.Id },
                UpcomingVenueHireApp = new SeededOpportunityApplication { ApplicationId = seedData.UpcomingVenueHireApp.Id, ConcertId = seedData.UpcomingVenueHireApp.Booking!.Concert!.Id },
                FlatFeeUpcomingConcert = new SeededConcert { Id = seedData.UpcomingFlatFeeBooking.Concert!.Id },
                VenueHireUpcomingConcert = new SeededConcert { Id = seedData.UpcomingVenueHireBooking.Concert!.Id },
                DoorSplitUpcomingConcert = new SeededConcert { Id = seedData.PostedDoorSplitBooking.Concert!.Id },
                VersusUpcomingConcert = new SeededConcert { Id = seedData.PostedVersusBooking.Concert!.Id },
            });
        });

        app.MapPost("/e2e/finish/{concertId:int}", async (int concertId, ICompletionDispatcher completionDispatcher, IReadDbContext readDb) =>
        {
            var result = await completionDispatcher.FinishAsync(concertId);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors.Select(e => e.Message));

            var bookingId = await readDb.ConcertBookings
                .Where(b => b.Concert!.Id == concertId)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            var paymentIntentId = await readDb.SettlementTransactions
                .Where(t => t.BookingId == bookingId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.PaymentIntentId)
                .FirstOrDefaultAsync();

            return paymentIntentId is not null
                ? Results.Ok(paymentIntentId)
                : Results.Ok();
        });

        app.MapGet("/e2e/payment-intent/{applicationId:int}", async (int applicationId, IReadDbContext readDb) =>
        {
            var bookingId = await readDb.ConcertBookings
                .Where(b => b.ApplicationId == applicationId)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            var paymentIntentId = await readDb.SettlementTransactions
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
