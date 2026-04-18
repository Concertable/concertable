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

    public OpportunityApplicationEntity FlatFeeApp { get; set; } = null!;
    public OpportunityApplicationEntity SettledApp { get; set; } = null!;
    public OpportunityApplicationEntity AwaitingPaymentApp { get; set; } = null!;
    public OpportunityApplicationEntity VersusApp { get; set; } = null!;
    public OpportunityApplicationEntity DoorSplitApp { get; set; } = null!;
    public OpportunityApplicationEntity VenueHireApp { get; set; } = null!;
    public OpportunityApplicationEntity PostedFlatFeeApp { get; set; } = null!;
    public OpportunityApplicationEntity PostedDoorSplitApp { get; set; } = null!;
    public OpportunityApplicationEntity PostedVersusApp { get; set; } = null!;
    public OpportunityApplicationEntity PostedVenueHireApp { get; set; } = null!;
    public OpportunityApplicationEntity FinishedDoorSplitApp { get; set; } = null!;
    public OpportunityApplicationEntity FinishedVersusApp { get; set; } = null!;
}
