namespace Concertable.Seeding;

public class SeedDataResponse
{
    public string TestPassword { get; set; } = null!;
    public SeededUser Customer { get; set; } = null!;
    public SeededVenueManager VenueManager1 { get; set; } = null!;
    public SeededArtistManager ArtistManager { get; set; } = null!;
    public SeededApplication PostedFlatFeeApp { get; set; } = null!;
    public SeededApplication PostedDoorSplitApp { get; set; } = null!;
    public SeededApplication PostedVersusApp { get; set; } = null!;
    public SeededApplication PostedVenueHireApp { get; set; } = null!;
    public SeededApplication FinishedDoorSplitApp { get; set; } = null!;
    public SeededApplication FinishedVersusApp { get; set; } = null!;
}

public class SeededUser
{
    public string Email { get; set; } = null!;
}

public class SeededVenueManager
{
    public string StripeAccountId { get; set; } = null!;
}

public class SeededArtistManager
{
    public string StripeAccountId { get; set; } = null!;
}

public class SeededApplication
{
    public int ApplicationId { get; set; }
    public SeededConcert Concert { get; set; } = null!;
}

public class SeededConcert
{
    public int Id { get; set; }
}
