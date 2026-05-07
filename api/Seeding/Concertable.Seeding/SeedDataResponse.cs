namespace Concertable.Seeding;

public class SeedDataResponse
{
    public string TestPassword { get; set; } = null!;
    public SeedUser Customer { get; set; } = null!;
    public SeedVenueManager VenueManager1 { get; set; } = null!;
    public SeedArtistManager ArtistManager1 { get; set; } = null!;
    public SeedOpportunity FreshVenueHireOpportunity { get; set; } = null!;
    public SeedApplication PendingFlatFeeApp { get; set; } = null!;
    public SeedApplication PendingVenueHireApp { get; set; } = null!;
    public SeedApplication PendingDoorSplitApp { get; set; } = null!;
    public SeedApplication PendingVersusApp { get; set; } = null!;
    public SeedApplication PostedFlatFeeApp { get; set; } = null!;
    public SeedApplication PostedDoorSplitApp { get; set; } = null!;
    public SeedApplication PostedVersusApp { get; set; } = null!;
    public SeedApplication PostedVenueHireApp { get; set; } = null!;
    public SeedApplication FinishedDoorSplitApp { get; set; } = null!;
    public SeedApplication FinishedVersusApp { get; set; } = null!;
    public SeedApplication PastVersusApp { get; set; } = null!;
    public SeedApplication PastFlatFeeApp { get; set; } = null!;
    public SeedApplication PastVenueHireApp { get; set; } = null!;
    public SeedApplication PastDoorSplitApp { get; set; } = null!;
    public SeedApplication UpcomingFlatFeeApp { get; set; } = null!;
    public SeedApplication UpcomingVenueHireApp { get; set; } = null!;
    public SeedConcert FlatFeeUpcomingConcert { get; set; } = null!;
    public SeedConcert VenueHireUpcomingConcert { get; set; } = null!;
    public SeedConcert DoorSplitUpcomingConcert { get; set; } = null!;
    public SeedConcert VersusUpcomingConcert { get; set; } = null!;
}

public class SeedOpportunity
{
    public int OpportunityId { get; set; }
    public int VenueId { get; set; }
}

public class SeedUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
}

public class SeedVenueManager
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
    public int VenueId { get; set; }
}

public class SeedArtistManager
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
    public string StripeCustomerId { get; set; } = null!;
}

public class SeedApplication
{
    public int ApplicationId { get; set; }
    public int? ConcertId { get; set; }
}

public class SeedConcert
{
    public int Id { get; set; }
}
