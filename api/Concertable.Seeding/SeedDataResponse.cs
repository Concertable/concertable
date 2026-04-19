namespace Concertable.Seeding;

public class SeedDataResponse
{
    public string TestPassword { get; set; } = null!;
    public SeededUser Customer { get; set; } = null!;
    public SeededVenueManager VenueManager1 { get; set; } = null!;
    public SeededArtistManager ArtistManager { get; set; } = null!;
    public SeededOpportunityApplication PendingFlatFeeApp { get; set; } = null!;
    public SeededOpportunityApplication PendingVenueHireApp { get; set; } = null!;
    public SeededOpportunityApplication PendingDoorSplitApp { get; set; } = null!;
    public SeededOpportunityApplication PendingVersusApp { get; set; } = null!;
    public SeededOpportunityApplication PostedFlatFeeApp { get; set; } = null!;
    public SeededOpportunityApplication PostedDoorSplitApp { get; set; } = null!;
    public SeededOpportunityApplication PostedVersusApp { get; set; } = null!;
    public SeededOpportunityApplication PostedVenueHireApp { get; set; } = null!;
    public SeededOpportunityApplication FinishedDoorSplitApp { get; set; } = null!;
    public SeededOpportunityApplication FinishedVersusApp { get; set; } = null!;
    public SeededOpportunityApplication UpcomingFlatFeeApp { get; set; } = null!;
    public SeededOpportunityApplication UpcomingVenueHireApp { get; set; } = null!;
    public SeededConcert FlatFeeUpcomingConcert { get; set; } = null!;
    public SeededConcert VenueHireUpcomingConcert { get; set; } = null!;
    public SeededConcert DoorSplitUpcomingConcert { get; set; } = null!;
    public SeededConcert VersusUpcomingConcert { get; set; } = null!;
}

public class SeededUser
{
    public string Email { get; set; } = null!;
}

public class SeededVenueManager
{
    public string Email { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
}

public class SeededArtistManager
{
    public string Email { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
}

public class SeededOpportunityApplication
{
    public int ApplicationId { get; set; }
    public int? ConcertId { get; set; }
}

public class SeededConcert
{
    public int Id { get; set; }
}
