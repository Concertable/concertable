using Concertable.Application.Interfaces;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Factories;
using Concertable.Seeding.Fakers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Data.Seeders;

internal class ConcertTestSeeder : ITestSeeder
{
    public int Order => 3;

    private readonly ConcertDbContext context;
    private readonly SeedData seed;
    private readonly TimeProvider timeProvider;

    public ConcertTestSeeder(ConcertDbContext context, SeedData seed, TimeProvider timeProvider)
    {
        this.context = context;
        this.seed = seed;
        this.timeProvider = timeProvider;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        await context.Opportunities.SeedIfEmptyAsync(async () =>
        {
            // TODO Step 10: real Contract seeding moves to ContractTestSeeder.
            // For now, opportunities seed with placeholder contractIds (1..10) — runtime FK is meaningless until Step 10.
            seed.Opportunities =
            [
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(2), now.AddMonths(2).AddHours(3)), contractId: 1, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(3), now.AddMonths(3).AddHours(3)), contractId: 2, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(4), now.AddMonths(4).AddHours(3)), contractId: 3, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(5), now.AddMonths(5).AddHours(3)), contractId: 4, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(6), now.AddMonths(6).AddHours(3)), contractId: 5, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(7), now.AddMonths(7).AddHours(3)), contractId: 6, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(8), now.AddMonths(8).AddHours(3)), contractId: 7, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(9), now.AddMonths(9).AddHours(3)), contractId: 8, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(10), now.AddMonths(10).AddHours(3)), contractId: 9, [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(11), now.AddMonths(11).AddHours(3)), contractId: 10, [seed.Rock.Id]),
            ];

            context.Opportunities.AddRange(seed.Opportunities);
            await context.SaveChangesAsync(ct);
        });

        await context.OpportunityApplications.SeedIfEmptyAsync(async () =>
        {
            var opps = seed.Opportunities;

            seed.FlatFeeApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[0].Id);

            seed.ConfirmedBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Draft Concert", 0m, 100, 100, seed.Artist.Id, seed.Venue.Id, opps[1].Period.Start, opps[1].Period.End).Generate());
            seed.ConfirmedApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[1].Id, seed.ConfirmedBooking);

            seed.AwaitingPaymentBooking = ConcertBookingFactory.AwaitingPayment(ConcertFaker.GetFaker("Unsettled Concert", 0m, 100, 100, seed.Artist.Id, seed.Venue.Id, opps[2].Period.Start, opps[2].Period.End).Generate());
            seed.AwaitingPaymentApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[2].Id, seed.AwaitingPaymentBooking);

            seed.VersusApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[3].Id);
            seed.DoorSplitApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[4].Id);
            seed.VenueHireApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[5].Id);

            seed.PostedFlatFeeBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted FlatFee Concert", 10.00m, 100, 100, seed.Artist.Id, seed.Venue.Id, opps[6].Period.Start, opps[6].Period.End, now).Generate());
            seed.PostedFlatFeeBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedFlatFeeApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[6].Id, seed.PostedFlatFeeBooking);

            seed.PostedDoorSplitBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted DoorSplit Concert", 10.00m, 100, 100, seed.Artist.Id, seed.Venue.Id, opps[7].Period.Start, opps[7].Period.End, now).Generate());
            seed.PostedDoorSplitBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedDoorSplitApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[7].Id, seed.PostedDoorSplitBooking);

            seed.PostedVersusBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted Versus Concert", 10.00m, 100, 100, seed.Artist.Id, seed.Venue.Id, opps[8].Period.Start, opps[8].Period.End, now).Generate());
            seed.PostedVersusBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedVersusApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[8].Id, seed.PostedVersusBooking);

            seed.PostedVenueHireBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted VenueHire Concert", 10.00m, 100, 100, seed.Artist.Id, seed.Venue.Id, opps[9].Period.Start, opps[9].Period.End, now).Generate());
            seed.PostedVenueHireBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedVenueHireApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[9].Id, seed.PostedVenueHireBooking);

            context.OpportunityApplications.AddRange(
                seed.FlatFeeApp,
                seed.ConfirmedApp,
                seed.AwaitingPaymentApp,
                seed.VersusApp,
                seed.DoorSplitApp,
                seed.VenueHireApp,
                seed.PostedFlatFeeApp,
                seed.PostedDoorSplitApp,
                seed.PostedVersusApp,
                seed.PostedVenueHireApp);

            await context.SaveChangesAsync(ct);
        });
    }
}
