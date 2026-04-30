using Concertable.Venue.Domain;

namespace Concertable.Seeding;

public class SeedData
{
    public const string TestPassword = "Password11!";

    public VenueManagerEntity VenueManager1 { get; set; } = null!;
    public VenueManagerEntity VenueManager2 { get; set; } = null!;
    public ArtistManagerEntity ArtistManager { get; set; } = null!;
    public ArtistManagerEntity ArtistManagerNoArtist { get; set; } = null!;
    public CustomerEntity Customer { get; set; } = null!;
    public AdminEntity Admin { get; set; } = null!;

    public IReadOnlyList<Guid> CustomerIds { get; set; } = [];
    public IReadOnlyList<Guid> ArtistManagerIds { get; set; } = [];
    public IReadOnlyList<Guid> VenueManagerIds { get; set; } = [];

    public string VenueManager1StripeAccountId { get; set; } = null!;
    public string ArtistManagerStripeAccountId { get; set; } = null!;

    public GenreEntity Rock { get; set; } = null!;
    public GenreEntity Jazz { get; set; } = null!;
    public GenreEntity HipHop { get; set; } = null!;
    public GenreEntity Electronic { get; set; } = null!;

    public ArtistEntity Artist { get; set; } = null!;
    public VenueEntity Venue { get; set; } = null!;

    public IReadOnlyList<OpportunityEntity> Opportunities { get; set; } = [];

    public IReadOnlyList<ContractEntity> Contracts { get; set; } = [];

    public FlatFeeContractEntity FlatFeeAppContract { get; set; } = null!;
    public FlatFeeContractEntity ConfirmedAppContract { get; set; } = null!;
    public FlatFeeContractEntity AwaitingPaymentAppContract { get; set; } = null!;
    public VersusContractEntity VersusAppContract { get; set; } = null!;
    public DoorSplitContractEntity DoorSplitAppContract { get; set; } = null!;
    public VenueHireContractEntity VenueHireAppContract { get; set; } = null!;
    public FlatFeeContractEntity PostedFlatFeeAppContract { get; set; } = null!;
    public DoorSplitContractEntity PostedDoorSplitAppContract { get; set; } = null!;
    public VersusContractEntity PostedVersusAppContract { get; set; } = null!;
    public VenueHireContractEntity PostedVenueHireAppContract { get; set; } = null!;

    // Pending applications (no booking)
    public ApplicationEntity FlatFeeApp { get; set; } = null!;
    public ApplicationEntity VersusApp { get; set; } = null!;
    public ApplicationEntity DoorSplitApp { get; set; } = null!;
    public ApplicationEntity VenueHireApp { get; set; } = null!;

    // Accepted applications with bookings
    public ApplicationEntity ConfirmedApp { get; set; } = null!;
    public BookingEntity ConfirmedBooking { get; set; } = null!;

    public ApplicationEntity AwaitingPaymentApp { get; set; } = null!;
    public BookingEntity AwaitingPaymentBooking { get; set; } = null!;

    public ApplicationEntity PostedFlatFeeApp { get; set; } = null!;
    public BookingEntity PostedFlatFeeBooking { get; set; } = null!;

    public ApplicationEntity PostedDoorSplitApp { get; set; } = null!;
    public BookingEntity PostedDoorSplitBooking { get; set; } = null!;

    public ApplicationEntity PostedVersusApp { get; set; } = null!;
    public BookingEntity PostedVersusBooking { get; set; } = null!;

    public ApplicationEntity PostedVenueHireApp { get; set; } = null!;
    public BookingEntity PostedVenueHireBooking { get; set; } = null!;

    public ApplicationEntity FinishedDoorSplitApp { get; set; } = null!;
    public BookingEntity FinishedDoorSplitBooking { get; set; } = null!;

    public ApplicationEntity FinishedVersusApp { get; set; } = null!;
    public BookingEntity FinishedVersusBooking { get; set; } = null!;

    public ApplicationEntity UpcomingFlatFeeApp { get; set; } = null!;
    public BookingEntity UpcomingFlatFeeBooking { get; set; } = null!;

    public ApplicationEntity UpcomingVenueHireApp { get; set; } = null!;
    public BookingEntity UpcomingVenueHireBooking { get; set; } = null!;
}
