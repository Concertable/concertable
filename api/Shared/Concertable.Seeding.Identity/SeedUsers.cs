namespace Concertable.Seeding.Identity;

public static class SeedUsers
{
    public const int ManagerCount = 35;
    public const int TotalCount = 1 + ManagerCount * 2;

    public static Guid ArtistManagerId(int n) => new($"a1000000-0000-0000-0000-{n:D12}");
    public static string ArtistManagerEmail(int n) => $"artistmanager{n}@test.com";

    public static Guid VenueManagerId(int n) => new($"b1000000-0000-0000-0000-{n:D12}");
    public static string VenueManagerEmail(int n) => $"venuemanager{n}@test.com";

    public static readonly Guid Admin = new("a0000000-0000-0000-0000-000000000001");
    public const string AdminEmail = "admin@test.com";
}
