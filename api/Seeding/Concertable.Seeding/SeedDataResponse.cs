namespace Concertable.Seeding;

public class SeedDataResponse
{
    public string TestPassword { get; set; } = null!;
    public SeededUser Customer { get; set; } = null!;
    public SeededVenueManager VenueManager1 { get; set; } = null!;
    public SeededArtistManager ArtistManager { get; set; } = null!;
    public SeededApplication PendingFlatFeeApp { get; set; } = null!;
    public SeededApplication PendingVenueHireApp { get; set; } = null!;
    public SeededApplication PendingDoorSplitApp { get; set; } = null!;
    public SeededApplication PendingVersusApp { get; set; } = null!;
    public SeededApplication PostedFlatFeeApp { get; set; } = null!;
    public SeededApplication PostedDoorSplitApp { get; set; } = null!;
    public SeededApplication PostedVersusApp { get; set; } = null!;
    public SeededApplication PostedVenueHireApp { get; set; } = null!;
    public SeededApplication FinishedDoorSplitApp { get; set; } = null!;
    public SeededApplication FinishedVersusApp { get; set; } = null!;
    public SeededApplication UpcomingFlatFeeApp { get; set; } = null!;
    public SeededApplication UpcomingVenueHireApp { get; set; } = null!;
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

public class SeededApplication
{
    public int ApplicationId { get; set; }
    public int? ConcertId { get; set; }
}

public class SeededConcert
{
    public int Id { get; set; }
}
