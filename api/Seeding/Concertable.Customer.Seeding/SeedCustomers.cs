namespace Concertable.Customer.Seeding;

public sealed record SeedCustomer(Guid Id, string Email);

public static class SeedCustomers
{
    public static readonly SeedCustomer Customer1 =
        new(new Guid("c0000000-0000-0000-0000-000000000001"), "customer1@test.com");
    public static readonly SeedCustomer Customer2 =
        new(new Guid("c0000000-0000-0000-0000-000000000002"), "customer2@test.com");
    public static readonly SeedCustomer Customer3 =
        new(new Guid("c0000000-0000-0000-0000-000000000003"), "customer3@test.com");

    public static IReadOnlyList<SeedCustomer> All { get; } = [Customer1, Customer2, Customer3];
}
