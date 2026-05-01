using Concertable.Application.Interfaces;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Factories;
using Concertable.Seeding.Fakers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Data.Seeders;

internal class ConcertDevSeeder : IDevSeeder
{
    public int Order => 4;

    private readonly ConcertDbContext context;
    private readonly SeedData seed;
    private readonly TimeProvider timeProvider;

    public ConcertDevSeeder(ConcertDbContext context, SeedData seed, TimeProvider timeProvider)
    {
        this.context = context;
        this.seed = seed;
        this.timeProvider = timeProvider;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var customerIds = seed.CustomerIds;
        var artistManagerIds = seed.ArtistManagerIds;

        await context.Opportunities.SeedIfEmptyAsync(async () =>
        {
            var contracts = seed.Contracts;
            seed.Opportunities =
            [
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-60), now.AddDays(-60).AddHours(3)), contractId: contracts[0].Id),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(-55), now.AddDays(-55).AddHours(3)), contractId: contracts[1].Id),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(-50), now.AddDays(-50).AddHours(3)), contractId: contracts[2].Id),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(-45), now.AddDays(-45).AddHours(3)), contractId: contracts[3].Id),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(-40), now.AddDays(-40).AddHours(3)), contractId: contracts[4].Id),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(-35), now.AddDays(-35).AddHours(3)), contractId: contracts[5].Id),
                OpportunityFactory.Create(7, new DateRange(now.AddDays(-30), now.AddDays(-30).AddHours(3)), contractId: contracts[6].Id),
                OpportunityFactory.Create(8, new DateRange(now.AddDays(-25), now.AddDays(-25).AddHours(3)), contractId: contracts[7].Id),
                OpportunityFactory.Create(9, new DateRange(now.AddDays(-20), now.AddDays(-20).AddHours(3)), contractId: contracts[8].Id),
                OpportunityFactory.Create(10, new DateRange(now.AddDays(-15), now.AddDays(-15).AddHours(3)), contractId: contracts[9].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-10), now.AddDays(-10).AddHours(3)), contractId: contracts[10].Id),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(-5), now.AddDays(-5).AddHours(3)), contractId: contracts[11].Id),
                OpportunityFactory.Create(3, new DateRange(now, now.AddHours(3)), contractId: contracts[12].Id),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(5), now.AddDays(5).AddHours(3)), contractId: contracts[13].Id),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(10), now.AddDays(10).AddHours(3)), contractId: contracts[14].Id),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)), contractId: contracts[15].Id),
                OpportunityFactory.Create(7, new DateRange(now.AddDays(20), now.AddDays(20).AddHours(3)), contractId: contracts[16].Id),
                OpportunityFactory.Create(8, new DateRange(now.AddDays(25), now.AddDays(25).AddHours(3)), contractId: contracts[17].Id),
                OpportunityFactory.Create(9, new DateRange(now.AddDays(30), now.AddDays(30).AddHours(3)), contractId: contracts[18].Id),
                OpportunityFactory.Create(10, new DateRange(now.AddDays(35), now.AddDays(35).AddHours(3)), contractId: contracts[19].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-40), now.AddDays(-40).AddHours(3)), contractId: contracts[20].Id),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(45), now.AddDays(45).AddHours(3)), contractId: contracts[21].Id),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(50), now.AddDays(50).AddHours(3)), contractId: contracts[22].Id),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(55), now.AddDays(55).AddHours(3)), contractId: contracts[23].Id),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(60), now.AddDays(60).AddHours(3)), contractId: contracts[24].Id),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(65), now.AddDays(65).AddHours(3)), contractId: contracts[25].Id),
                OpportunityFactory.Create(7, new DateRange(now.AddDays(70), now.AddDays(70).AddHours(3)), contractId: contracts[26].Id),
                OpportunityFactory.Create(8, new DateRange(now.AddDays(75), now.AddDays(75).AddHours(3)), contractId: contracts[27].Id),
                OpportunityFactory.Create(9, new DateRange(now.AddDays(80), now.AddDays(80).AddHours(3)), contractId: contracts[28].Id),
                OpportunityFactory.Create(10, new DateRange(now.AddDays(85), now.AddDays(85).AddHours(3)), contractId: contracts[29].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-85), now.AddDays(-85).AddHours(3)), contractId: contracts[30].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(85), now.AddDays(85).AddHours(5)), contractId: contracts[31].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(2), now.AddDays(2).AddHours(3)), contractId: contracts[32].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(4), now.AddDays(4).AddHours(3)), contractId: contracts[33].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(6), now.AddDays(6).AddHours(3)), contractId: contracts[34].Id),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(8), now.AddDays(8).AddHours(3)), contractId: contracts[35].Id),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(10), now.AddDays(10).AddHours(3)), contractId: contracts[36].Id),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(12), now.AddDays(12).AddHours(3)), contractId: contracts[37].Id),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(14), now.AddDays(14).AddHours(3)), contractId: contracts[38].Id),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(16), now.AddDays(16).AddHours(3)), contractId: contracts[39].Id),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(18), now.AddDays(18).AddHours(3)), contractId: contracts[40].Id),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(22), now.AddDays(22).AddHours(3)), contractId: contracts[41].Id),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(24), now.AddDays(24).AddHours(3)), contractId: contracts[42].Id),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(26), now.AddDays(26).AddHours(3)), contractId: contracts[43].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(30), now.AddDays(30).AddHours(3)), contractId: contracts[44].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(32), now.AddDays(32).AddHours(3)), contractId: contracts[45].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(34), now.AddDays(34).AddHours(3)), contractId: contracts[46].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(36), now.AddDays(36).AddHours(3)), contractId: contracts[47].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(38), now.AddDays(38).AddHours(3)), contractId: contracts[48].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-60), now.AddDays(-60).AddHours(3)), contractId: contracts[49].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-90), now.AddDays(-90).AddHours(3)), contractId: contracts[50].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(120), now.AddDays(120).AddHours(3)), contractId: contracts[51].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(150), now.AddDays(150).AddHours(3)), contractId: contracts[52].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(180), now.AddDays(180).AddHours(3)), contractId: contracts[53].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(200), now.AddDays(200).AddHours(3)), contractId: contracts[54].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(210), now.AddDays(210).AddHours(3)), contractId: contracts[55].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(220), now.AddDays(220).AddHours(3)), contractId: contracts[56].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)), contractId: contracts[57].Id),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(20), now.AddDays(20).AddHours(3)), contractId: contracts[58].Id),
            ];

            context.Opportunities.AddRange(seed.Opportunities);
            await context.SaveChangesAsync(ct);
        });

        await context.OpportunityGenres.SeedIfEmptyAsync(async () =>
        {
            var opportunityGenres = new OpportunityGenreEntity[]
            {
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 2, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 3, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 4, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 7, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 10, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 12, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 13, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 14, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 15, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 17, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 18, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 19, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 20, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 21, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 23, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 24, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 25, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 27, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 28, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 29, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 31, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 32, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 33, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 35, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 36, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 37, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 38, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 39, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 40, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 8 }
            };
            context.OpportunityGenres.AddRange(opportunityGenres);
            await context.SaveChangesAsync(ct);
        });

        await context.Applications.SeedIfEmptyAsync(async () =>
        {
            seed.ConfirmedBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("Ultimate Dance Party", 27m, 160, 140, 1, seed.Opportunities[5].VenueId, seed.Opportunities[5].Period.Start, seed.Opportunities[5].Period.End, now.AddDays(2)).Generate());
            seed.ConfirmedApp = ApplicationFactory.Accepted(1, 6, seed.ConfirmedBooking);

            seed.PostedDoorSplitBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("Boogie Wonderland", 25m, 120, 100, 1, seed.Opportunities[52].VenueId, seed.Opportunities[52].Period.Start, seed.Opportunities[52].Period.End, now.AddDays(150)).Generate());
            seed.PostedDoorSplitApp = ApplicationFactory.Accepted(1, 53, seed.PostedDoorSplitBooking);

            seed.PostedVersusBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("Funk it up", 20m, 150, 130, 2, seed.Opportunities[53].VenueId, seed.Opportunities[53].Period.Start, seed.Opportunities[53].Period.End, now.AddDays(180)).Generate());
            seed.PostedVersusApp = ApplicationFactory.Accepted(2, 54, seed.PostedVersusBooking);

            seed.PostedFlatFeeBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("Boogie it up!", 20m, 150, 130, 2, seed.Opportunities[30].VenueId, seed.Opportunities[30].Period.Start, seed.Opportunities[30].Period.End, now.AddDays(-85)).Generate());
            seed.PostedFlatFeeApp = ApplicationFactory.Accepted(2, 31, seed.PostedFlatFeeBooking);

            seed.PostedVenueHireBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("VenueHire Spectacular", 30m, 200, 180, 1, seed.Opportunities[20].VenueId, seed.Opportunities[20].Period.Start, seed.Opportunities[20].Period.End, now.AddDays(-40)).Generate());
            seed.PostedVenueHireApp = ApplicationFactory.Accepted(1, 21, seed.PostedVenueHireBooking);

            seed.DoorSplitApp = ApplicationFactory.Create(1, 56);
            seed.VersusApp = ApplicationFactory.Create(1, 57);
            seed.VenueHireApp = ApplicationFactory.Create(1, 52);
            seed.FlatFeeApp = ApplicationFactory.Create(1, 55);

            seed.AwaitingPaymentBooking = BookingFactory.AwaitingPayment(ConcertFaker.GetFaker("Awaiting Show", 15m, 100, 80, 1, seed.Opportunities[32].VenueId, seed.Opportunities[32].Period.Start, seed.Opportunities[32].Period.End, now.AddDays(3)).Generate());
            seed.AwaitingPaymentApp = ApplicationFactory.Accepted(1, 33, seed.AwaitingPaymentBooking);

            seed.FinishedDoorSplitBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("DoorSplit Settlement Show", 20m, 100, 99, 1, seed.Opportunities[49].VenueId, seed.Opportunities[49].Period.Start, seed.Opportunities[49].Period.End, now.AddDays(-60)).Generate());
            seed.FinishedDoorSplitApp = ApplicationFactory.Accepted(1, 50, seed.FinishedDoorSplitBooking);

            seed.FinishedVersusBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("Versus Settlement Show", 20m, 100, 99, 1, seed.Opportunities[50].VenueId, seed.Opportunities[50].Period.Start, seed.Opportunities[50].Period.End, now.AddDays(-90)).Generate());
            seed.FinishedVersusApp = ApplicationFactory.Accepted(1, 51, seed.FinishedVersusBooking);

            seed.UpcomingFlatFeeBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("Upcoming FlatFee Show", 20m, 150, 150, 2, seed.Opportunities[57].VenueId, seed.Opportunities[57].Period.Start, seed.Opportunities[57].Period.End, now).Generate());
            seed.UpcomingFlatFeeApp = ApplicationFactory.Accepted(2, 58, seed.UpcomingFlatFeeBooking);

            seed.UpcomingVenueHireBooking = BookingFactory.Confirmed(ConcertFaker.GetFaker("Upcoming VenueHire Show", 30m, 200, 200, 1, seed.Opportunities[58].VenueId, seed.Opportunities[58].Period.Start, seed.Opportunities[58].Period.End, now).Generate());
            seed.UpcomingVenueHireApp = ApplicationFactory.Accepted(1, 59, seed.UpcomingVenueHireBooking);

            var applications = new ApplicationEntity[]
            {
                // Apps 1-20: Complete (past concerts)
                ApplicationFactory.Accepted(1, 1, BookingFactory.Complete(ConcertFaker.GetFaker("Rockin' all Night", 15m, 120, 80, 1, seed.Opportunities[0].VenueId, seed.Opportunities[0].Period.Start, seed.Opportunities[0].Period.End, now.AddDays(-58)).Generate())),
                ApplicationFactory.Accepted(2, 1, BookingFactory.Complete(ConcertFaker.GetFaker("Non Stop Party", 12m, 110, 70, 2, seed.Opportunities[0].VenueId, seed.Opportunities[0].Period.Start, seed.Opportunities[0].Period.End, now.AddDays(-55)).Generate())),
                ApplicationFactory.Accepted(3, 1, BookingFactory.Complete(ConcertFaker.GetFaker("Super Mix", 18m, 130, 100, 3, seed.Opportunities[0].VenueId, seed.Opportunities[0].Period.Start, seed.Opportunities[0].Period.End, now.AddDays(-52)).Generate())),
                ApplicationFactory.Accepted(4, 1, BookingFactory.Complete(ConcertFaker.GetFaker("Hip-Hop till you flip-flop", 10m, 100, 60, 4, seed.Opportunities[0].VenueId, seed.Opportunities[0].Period.Start, seed.Opportunities[0].Period.End, now.AddDays(-49)).Generate())),
                ApplicationFactory.Accepted(1, 2, BookingFactory.Complete(ConcertFaker.GetFaker("Dance the night away", 25m, 140, 110, 1, seed.Opportunities[1].VenueId, seed.Opportunities[1].Period.Start, seed.Opportunities[1].Period.End, now.AddDays(-46)).Generate())),
                ApplicationFactory.Accepted(2, 2, BookingFactory.Complete(ConcertFaker.GetFaker("Dizzy One", 20m, 150, 90, 2, seed.Opportunities[1].VenueId, seed.Opportunities[1].Period.Start, seed.Opportunities[1].Period.End, now.AddDays(-43)).Generate())),
                ApplicationFactory.Accepted(5, 2, BookingFactory.Complete(ConcertFaker.GetFaker("Beers and Boombox", 30m, 170, 150, 5, seed.Opportunities[1].VenueId, seed.Opportunities[1].Period.Start, seed.Opportunities[1].Period.End, now.AddDays(-40)).Generate())),
                ApplicationFactory.Accepted(6, 2, BookingFactory.Complete(ConcertFaker.GetFaker("Rockin' Tonight!", 16m, 130, 100, 6, seed.Opportunities[1].VenueId, seed.Opportunities[1].Period.Start, seed.Opportunities[1].Period.End, now.AddDays(-37)).Generate())),
                ApplicationFactory.Accepted(1, 3, BookingFactory.Complete(ConcertFaker.GetFaker("Groovin' All Night", 14m, 115, 75, 1, seed.Opportunities[2].VenueId, seed.Opportunities[2].Period.Start, seed.Opportunities[2].Period.End, now.AddDays(-34)).Generate())),
                ApplicationFactory.Accepted(2, 3, BookingFactory.Complete(ConcertFaker.GetFaker("Nonstop Vibes", 22m, 135, 100, 2, seed.Opportunities[2].VenueId, seed.Opportunities[2].Period.Start, seed.Opportunities[2].Period.End, now.AddDays(-31)).Generate())),
                ApplicationFactory.Accepted(7, 3, BookingFactory.Complete(ConcertFaker.GetFaker("Electric Dreams", 13m, 125, 85, 7, seed.Opportunities[2].VenueId, seed.Opportunities[2].Period.Start, seed.Opportunities[2].Period.End, now.AddDays(-28)).Generate())),
                ApplicationFactory.Accepted(8, 3, BookingFactory.Complete(ConcertFaker.GetFaker("Beat Drop Frenzy", 11m, 120, 90, 8, seed.Opportunities[2].VenueId, seed.Opportunities[2].Period.Start, seed.Opportunities[2].Period.End, now.AddDays(-25)).Generate())),
                ApplicationFactory.Accepted(1, 4, BookingFactory.Complete(ConcertFaker.GetFaker("Summer Jam", 19m, 140, 110, 1, seed.Opportunities[3].VenueId, seed.Opportunities[3].Period.Start, seed.Opportunities[3].Period.End, now.AddDays(-22)).Generate())),
                ApplicationFactory.Accepted(2, 4, BookingFactory.Complete(ConcertFaker.GetFaker("Midnight Madness", 17m, 135, 105, 2, seed.Opportunities[3].VenueId, seed.Opportunities[3].Period.Start, seed.Opportunities[3].Period.End, now.AddDays(-19)).Generate())),
                ApplicationFactory.Accepted(9, 4, BookingFactory.Complete(ConcertFaker.GetFaker("Like a Boss", 21m, 145, 115, 9, seed.Opportunities[3].VenueId, seed.Opportunities[3].Period.Start, seed.Opportunities[3].Period.End, now.AddDays(-16)).Generate())),
                ApplicationFactory.Accepted(10, 4, BookingFactory.Complete(ConcertFaker.GetFaker("Lights and Sound", 18m, 140, 120, 10, seed.Opportunities[3].VenueId, seed.Opportunities[3].Period.Start, seed.Opportunities[3].Period.End, now.AddDays(-13)).Generate())),
                ApplicationFactory.Accepted(1, 5, BookingFactory.Complete(ConcertFaker.GetFaker("Rhythm Nation", 26m, 155, 130, 1, seed.Opportunities[4].VenueId, seed.Opportunities[4].Period.Start, seed.Opportunities[4].Period.End, now.AddDays(-10)).Generate())),
                ApplicationFactory.Accepted(2, 5, BookingFactory.Complete(ConcertFaker.GetFaker("Bass Drop Party", 15m, 120, 100, 2, seed.Opportunities[4].VenueId, seed.Opportunities[4].Period.Start, seed.Opportunities[4].Period.End, now.AddDays(-7)).Generate())),
                ApplicationFactory.Accepted(11, 5, BookingFactory.Complete(ConcertFaker.GetFaker("Chill & Thrill", 28m, 160, 145, 11, seed.Opportunities[4].VenueId, seed.Opportunities[4].Period.Start, seed.Opportunities[4].Period.End, now.AddDays(-4)).Generate())),
                ApplicationFactory.Accepted(12, 5, BookingFactory.Complete(ConcertFaker.GetFaker("Vibin' till Night", 24m, 150, 130, 12, seed.Opportunities[4].VenueId, seed.Opportunities[4].Period.Start, seed.Opportunities[4].Period.End, now.AddDays(-1)).Generate())),
                // Apps 21-26: Accepted (upcoming concerts)
                seed.ConfirmedApp,
                ApplicationFactory.Accepted(2, 6, BookingFactory.Confirmed(ConcertFaker.GetFaker("Rock Your Soul", 23m, 130, 100, 2, seed.Opportunities[5].VenueId, seed.Opportunities[5].Period.Start, seed.Opportunities[5].Period.End, now.AddDays(5)).Generate())),
                ApplicationFactory.Accepted(13, 6, BookingFactory.Confirmed(ConcertFaker.GetFaker("Danceaway", 29m, 155, 140, 13, seed.Opportunities[5].VenueId, seed.Opportunities[5].Period.Start, seed.Opportunities[5].Period.End, now.AddDays(8)).Generate())),
                ApplicationFactory.Accepted(14, 6, BookingFactory.Confirmed(ConcertFaker.GetFaker("Bassline Groove Beats", 10m, 110, 70, 14, seed.Opportunities[5].VenueId, seed.Opportunities[5].Period.Start, seed.Opportunities[5].Period.End, now.AddDays(11)).Generate())),
                ApplicationFactory.Accepted(1, 7, BookingFactory.Confirmed(ConcertFaker.GetFaker("Once in a Lifetime!", 15m, 125, 90, 1, seed.Opportunities[6].VenueId, seed.Opportunities[6].Period.Start, seed.Opportunities[6].Period.End, now.AddDays(14)).Generate())),
                ApplicationFactory.Accepted(2, 7, BookingFactory.Confirmed(ConcertFaker.GetFaker("Jungle Fever", 30m, 180, 170, 2, seed.Opportunities[6].VenueId, seed.Opportunities[6].Period.Start, seed.Opportunities[6].Period.End, now.AddDays(17)).Generate())),
                // Apps 27-34: Pending (no concert)
                ApplicationFactory.Create(15, 7),
                ApplicationFactory.Create(16, 7),
                ApplicationFactory.Create(1, 8),
                ApplicationFactory.Create(2, 8),
                ApplicationFactory.Create(17, 8),
                ApplicationFactory.Create(18, 8),
                ApplicationFactory.Create(17, 40),
                ApplicationFactory.Create(18, 41),
                // App 35: Accepted (upcoming concert)
                ApplicationFactory.Accepted(1, 14, BookingFactory.Confirmed(ConcertFaker.GetFaker("Boogie Nights", 20m, 100, 80, 1, seed.Opportunities[13].VenueId, seed.Opportunities[13].Period.Start, seed.Opportunities[13].Period.End, now.AddDays(6)).Generate())),
                // Apps 36-38: Pending (no concert)
                ApplicationFactory.Create(2, 14),
                ApplicationFactory.Create(3, 14),
                ApplicationFactory.Create(4, 14),
                // App 39: Accepted (upcoming concert)
                seed.PostedDoorSplitApp,
                // Apps 40-41: Pending (no concert)
                seed.DoorSplitApp,
                ApplicationFactory.Create(7, 15),
                // App 42: Accepted (upcoming concert)
                ApplicationFactory.Accepted(8, 15, BookingFactory.Confirmed(ConcertFaker.GetFaker("Bass in the Air", 30m, 140, 120, 8, seed.Opportunities[14].VenueId, seed.Opportunities[14].Period.Start, seed.Opportunities[14].Period.End, now.AddDays(18)).Generate())),
                // Apps 43-44: Pending (no concert)
                ApplicationFactory.Create(9, 16),
                ApplicationFactory.Create(10, 16),
                // App 45: Accepted (upcoming concert)
                ApplicationFactory.Accepted(11, 16, BookingFactory.Confirmed(ConcertFaker.GetFaker("Jumpin and thumpin", 15m, 100, 80, 11, seed.Opportunities[15].VenueId, seed.Opportunities[15].Period.Start, seed.Opportunities[15].Period.End, now.AddDays(22)).Generate())),
                // Apps 46-48: Pending (no concert)
                ApplicationFactory.Create(12, 16),
                seed.VersusApp,
                ApplicationFactory.Create(14, 17),
                // App 49: Accepted (upcoming concert)
                seed.PostedVersusApp,
                // Apps 50-70: Pending (no concert)
                ApplicationFactory.Create(16, 17),
                ApplicationFactory.Create(1, 34),
                ApplicationFactory.Create(2, 34),
                ApplicationFactory.Create(19, 34),
                ApplicationFactory.Create(20, 34),
                ApplicationFactory.Create(1, 38),
                ApplicationFactory.Create(2, 38),
                ApplicationFactory.Create(12, 38),
                ApplicationFactory.Create(4, 38),
                ApplicationFactory.Create(1, 45),
                ApplicationFactory.Create(2, 46),
                ApplicationFactory.Create(3, 47),
                ApplicationFactory.Create(4, 48),
                ApplicationFactory.Create(5, 49),
                ApplicationFactory.Create(2, 50),
                ApplicationFactory.Create(2, 51),
                seed.VenueHireApp,
                ApplicationFactory.Create(2, 52),
                seed.FlatFeeApp,
                // App 71: PostedFlatFeeApp (declared before array)
                seed.PostedFlatFeeApp,
                // Apps 72-75: Pending (no concert)
                ApplicationFactory.Create(3, 31),
                ApplicationFactory.Create(1, 32),
                ApplicationFactory.Create(2, 32),
                ApplicationFactory.Create(3, 32),
                // App 76: AwaitingPayment (concert 33)
                seed.AwaitingPaymentApp,
                // App 77: PostedVenueHireApp (concert 34)
                seed.PostedVenueHireApp,
                // App 78: FinishedDoorSplitApp (concert 35) — VenueId=1, DoorSplit 70%
                seed.FinishedDoorSplitApp,
                // App 79: FinishedVersusApp (concert 36) — VenueId=1, Versus 100+70%
                seed.FinishedVersusApp,
                // App 80: UpcomingFlatFeeApp — VenueId=1, FlatFee (future)
                seed.UpcomingFlatFeeApp,
                // App 81: UpcomingVenueHireApp — VenueId=1, VenueHire (future)
                seed.UpcomingVenueHireApp,
            };
            context.Applications.AddRange(applications);
            await context.SaveChangesAsync(ct);
        });

        await context.ConcertGenres.SeedIfEmptyAsync(async () =>
        {
            var concertGenres = new ConcertGenreEntity[]
            {
                new ConcertGenreEntity { ConcertId = 1, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 1, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 2, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 2, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 3, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 3, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 4, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 6, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 6, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 7, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 8, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 8, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 9, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 9, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 10, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 11, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 12, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 13, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 14, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 15, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 16, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 17, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 17, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 18, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 18, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 19, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 19, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 20, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 21, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 21, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 22, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 23, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 24, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 25, GenreId = 8 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 29, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 29, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 8 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 7 },
            };
            context.ConcertGenres.AddRange(concertGenres);
            await context.SaveChangesAsync(ct);
        });

        await context.Tickets.SeedIfEmptyAsync(async () =>
        {
            var tickets = new TicketEntity[]
            {
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 1, Array.Empty<byte>(), now.AddDays(-56)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 2, Array.Empty<byte>(), now.AddDays(-53)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[2], 3, Array.Empty<byte>(), now.AddDays(-50)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 4, Array.Empty<byte>(), now.AddDays(-47)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[2], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 5, Array.Empty<byte>(), now.AddDays(-44)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 6, Array.Empty<byte>(), now.AddDays(-43)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 6, Array.Empty<byte>(), now.AddDays(-43)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 8, Array.Empty<byte>(), now.AddDays(-38)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 8, Array.Empty<byte>(), now.AddDays(-38)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 8, Array.Empty<byte>(), now.AddDays(-37)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], seed.FinishedDoorSplitBooking.Concert!.Id, Array.Empty<byte>(), now.AddDays(-60)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], seed.FinishedVersusBooking.Concert!.Id, Array.Empty<byte>(), now.AddDays(-90)),
            };
            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync(ct);

            await context.Reviews.SeedIfEmptyAsync(async () =>
            {
                var reviews = new ReviewEntity[]
                {
                    ReviewFactory.Create(tickets[0].Id, 4, "Amazing performance!"),
                    ReviewFactory.Create(tickets[1].Id, 5, "Loved every moment!"),
                    ReviewFactory.Create(tickets[2].Id, 5, "Unforgettable night!"),
                    ReviewFactory.Create(tickets[3].Id, 4, "Great energy from the crowd."),
                    ReviewFactory.Create(tickets[4].Id, 3, "Good, but the sound was a bit off."),
                    ReviewFactory.Create(tickets[5].Id, 5, "Perfect setlist and vibes!"),
                    ReviewFactory.Create(tickets[6].Id, 4, "Would attend again!"),
                    ReviewFactory.Create(tickets[7].Id, 5, "Fantastic indie atmosphere."),
                    ReviewFactory.Create(tickets[8].Id, 4, "Loved the venue!"),
                    ReviewFactory.Create(tickets[9].Id, 4, "Solid performance."),
                    ReviewFactory.Create(tickets[10].Id, 5, "Caught my new favorite artist!"),
                    ReviewFactory.Create(tickets[11].Id, 3, "Good music, but crowded."),
                    ReviewFactory.Create(tickets[12].Id, 5, "Indie dream come true."),
                    ReviewFactory.Create(tickets[13].Id, 4, "Chill night out."),
                    ReviewFactory.Create(tickets[14].Id, 5, "Incredible stage presence!"),
                    ReviewFactory.Create(tickets[15].Id, 4, "Would love to see them again."),
                    ReviewFactory.Create(tickets[16].Id, 5, "Next-level visuals."),
                    ReviewFactory.Create(tickets[17].Id, 4, "Very unique sound."),
                    ReviewFactory.Create(tickets[18].Id, 4, "Great crowd energy."),
                    ReviewFactory.Create(tickets[19].Id, 5, "Absolute fire show."),
                    ReviewFactory.Create(tickets[20].Id, 5, "Perfect DnB experience."),
                    ReviewFactory.Create(tickets[21].Id, 4, "Smooth lyrical vibes."),
                    ReviewFactory.Create(tickets[22].Id, 5, "Top-tier show!"),
                    ReviewFactory.Create(tickets[23].Id, 4, "Nice intimate gig."),
                    ReviewFactory.Create(tickets[24].Id, 3, "A bit too loud but still fun."),
                    ReviewFactory.Create(tickets[25].Id, 4, "Well organized event."),
                    ReviewFactory.Create(tickets[26].Id, 5, "Really enjoyed it."),
                    ReviewFactory.Create(tickets[27].Id, 5, "Brought my friends, all loved it."),
                    ReviewFactory.Create(tickets[28].Id, 3, "Solid but expected more."),
                    ReviewFactory.Create(tickets[29].Id, 4, "The lighting was amazing!"),
                    ReviewFactory.Create(tickets[30].Id, 5, "Instant classic."),
                    ReviewFactory.Create(tickets[31].Id, 4, "Had a great time."),
                    ReviewFactory.Create(tickets[32].Id, 4, "Venue was packed with energy.")
                };
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync(ct);
            });
        });
    }
}
