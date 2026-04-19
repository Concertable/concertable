using Concertable.Core.Entities;

namespace Concertable.Seeding;

public class SeedData
{
    public const string TestPassword = "Password11!";

    public VenueManagerEntity VenueManager1 { get; set; } = null!;
    public VenueManagerEntity VenueManager2 { get; set; } = null!;
    public ArtistManagerEntity ArtistManager { get; set; } = null!;
    public CustomerEntity Customer { get; set; } = null!;
    public AdminEntity Admin { get; set; } = null!;

    public GenreEntity Rock { get; set; } = null!;
    public GenreEntity Jazz { get; set; } = null!;
    public GenreEntity HipHop { get; set; } = null!;
    public GenreEntity Electronic { get; set; } = null!;

    public ArtistEntity Artist { get; set; } = null!;
    public VenueEntity Venue { get; set; } = null!;

    public IReadOnlyList<OpportunityEntity> Opportunities { get; set; } = [];

    // Pending applications (no booking)
    public OpportunityApplicationEntity FlatFeeApp { get; set; } = null!;
    public OpportunityApplicationEntity VersusApp { get; set; } = null!;
    public OpportunityApplicationEntity DoorSplitApp { get; set; } = null!;
    public OpportunityApplicationEntity VenueHireApp { get; set; } = null!;

    // Accepted applications with bookings
    public OpportunityApplicationEntity ConfirmedApp { get; set; } = null!;
    public ConcertBookingEntity ConfirmedBooking { get; set; } = null!;

    public OpportunityApplicationEntity AwaitingPaymentApp { get; set; } = null!;
    public ConcertBookingEntity AwaitingPaymentBooking { get; set; } = null!;

    public OpportunityApplicationEntity PostedFlatFeeApp { get; set; } = null!;
    public ConcertBookingEntity PostedFlatFeeBooking { get; set; } = null!;

    public OpportunityApplicationEntity PostedDoorSplitApp { get; set; } = null!;
    public ConcertBookingEntity PostedDoorSplitBooking { get; set; } = null!;

    public OpportunityApplicationEntity PostedVersusApp { get; set; } = null!;
    public ConcertBookingEntity PostedVersusBooking { get; set; } = null!;

    public OpportunityApplicationEntity PostedVenueHireApp { get; set; } = null!;
    public ConcertBookingEntity PostedVenueHireBooking { get; set; } = null!;

    public OpportunityApplicationEntity FinishedDoorSplitApp { get; set; } = null!;
    public ConcertBookingEntity FinishedDoorSplitBooking { get; set; } = null!;

    public OpportunityApplicationEntity FinishedVersusApp { get; set; } = null!;
    public ConcertBookingEntity FinishedVersusBooking { get; set; } = null!;

    public OpportunityApplicationEntity UpcomingFlatFeeApp { get; set; } = null!;
    public ConcertBookingEntity UpcomingFlatFeeBooking { get; set; } = null!;

    public OpportunityApplicationEntity UpcomingVenueHireApp { get; set; } = null!;
    public ConcertBookingEntity UpcomingVenueHireBooking { get; set; } = null!;
}
