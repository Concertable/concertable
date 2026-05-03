using Concertable.Application.Interfaces;
using Concertable.Concert.Application.Interfaces;
using Concertable.Seeding;
using Concertable.Shared;

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
                Customer = new SeededUser { Id = seedData.Customer.Id, Email = seedData.Customer.Email },
                VenueManager1 = new SeededVenueManager { Id = seedData.VenueManager1.Id, Email = seedData.VenueManager1.Email, StripeAccountId = seedData.VenueManager1StripeAccountId },
                ArtistManager = new SeededArtistManager { Id = seedData.ArtistManager.Id, Email = seedData.ArtistManager.Email, StripeAccountId = seedData.ArtistManagerStripeAccountId },
                PendingFlatFeeApp = new SeededApplication { ApplicationId = seedData.FlatFeeApp.Id },
                PendingVenueHireApp = new SeededApplication { ApplicationId = seedData.VenueHireApp.Id },
                PendingDoorSplitApp = new SeededApplication { ApplicationId = seedData.DoorSplitApp.Id },
                PendingVersusApp = new SeededApplication { ApplicationId = seedData.VersusApp.Id },
                PostedFlatFeeApp = new SeededApplication { ApplicationId = seedData.PostedFlatFeeApp.Id, ConcertId = seedData.PostedFlatFeeApp.Booking!.Concert!.Id },
                PostedDoorSplitApp = new SeededApplication { ApplicationId = seedData.PostedDoorSplitApp.Id, ConcertId = seedData.PostedDoorSplitApp.Booking!.Concert!.Id },
                PostedVersusApp = new SeededApplication { ApplicationId = seedData.PostedVersusApp.Id, ConcertId = seedData.PostedVersusApp.Booking!.Concert!.Id },
                PostedVenueHireApp = new SeededApplication { ApplicationId = seedData.PostedVenueHireApp.Id, ConcertId = seedData.PostedVenueHireApp.Booking!.Concert!.Id },
                FinishedDoorSplitApp = new SeededApplication { ApplicationId = seedData.FinishedDoorSplitApp.Id, ConcertId = seedData.FinishedDoorSplitApp.Booking!.Concert!.Id },
                FinishedVersusApp = new SeededApplication { ApplicationId = seedData.FinishedVersusApp.Id, ConcertId = seedData.FinishedVersusApp.Booking!.Concert!.Id },
                UpcomingFlatFeeApp = new SeededApplication { ApplicationId = seedData.UpcomingFlatFeeApp.Id, ConcertId = seedData.UpcomingFlatFeeApp.Booking!.Concert!.Id },
                UpcomingVenueHireApp = new SeededApplication { ApplicationId = seedData.UpcomingVenueHireApp.Id, ConcertId = seedData.UpcomingVenueHireApp.Booking!.Concert!.Id },
                FlatFeeUpcomingConcert = new SeededConcert { Id = seedData.UpcomingFlatFeeBooking.Concert!.Id },
                VenueHireUpcomingConcert = new SeededConcert { Id = seedData.UpcomingVenueHireBooking.Concert!.Id },
                DoorSplitUpcomingConcert = new SeededConcert { Id = seedData.PostedDoorSplitBooking.Concert!.Id },
                VersusUpcomingConcert = new SeededConcert { Id = seedData.PostedVersusBooking.Concert!.Id },
            });
        });

        app.MapPost("/e2e/finish/{concertId:int}", async (int concertId, ICompletionDispatcher CompletionDispatcher) =>
        {
            var result = await CompletionDispatcher.FinishAsync(concertId);
            return result.IsFailed
                ? Results.BadRequest(result.Errors.SelectMessages())
                : Results.Ok();
        });
    }
}
