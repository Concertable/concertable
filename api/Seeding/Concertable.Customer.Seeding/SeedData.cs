namespace Concertable.Customer.Seeding;

public class SeedData
{
    public const string TestPassword = "Password11!";
    public const int UpcomingConcertId = 13;

    public SeedCustomer Customer => SeedCustomers.Customer1;
    public IReadOnlyList<Guid> CustomerIds { get; } = [.. SeedCustomers.All.Select(c => c.Id)];
}
