using Concertable.Application.Interfaces;
using Concertable.Payment.Infrastructure.Data;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Payment.Infrastructure.Data.Seeders;

internal class PaymentTestSeeder : ITestSeeder
{
    public int Order => 5;

    private readonly PaymentDbContext context;
    private readonly SeedData seedData;

    public PaymentTestSeeder(PaymentDbContext context, SeedData seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.PayoutAccounts.SeedIfEmptyAsync(async () =>
        {
            seedData.VenueManager1StripeAccountId = "acct_test_venue1";
            seedData.ArtistManagerStripeAccountId = "acct_test_artist1";

            var venueManager1 = PayoutAccountEntity.Create(seedData.VenueManager1.Id);
            venueManager1.LinkAccount(seedData.VenueManager1StripeAccountId);
            venueManager1.LinkCustomer("cus_test_venue1");
            venueManager1.MarkVerified();

            var venueManager2 = PayoutAccountEntity.Create(seedData.VenueManager2.Id);
            venueManager2.LinkAccount("acct_test_venue2");
            venueManager2.LinkCustomer("cus_test_venue2");
            venueManager2.MarkVerified();

            var artistManager = PayoutAccountEntity.Create(seedData.ArtistManager.Id);
            artistManager.LinkAccount(seedData.ArtistManagerStripeAccountId);
            artistManager.LinkCustomer("cus_test_artist1");
            artistManager.MarkVerified();

            var artistManagerNoArtist = PayoutAccountEntity.Create(seedData.ArtistManagerNoArtist.Id);
            artistManagerNoArtist.LinkAccount("acct_test_artist2");
            artistManagerNoArtist.LinkCustomer("cus_test_artist2");
            artistManagerNoArtist.MarkVerified();

            var customer = PayoutAccountEntity.Create(seedData.Customer.Id);
            customer.LinkCustomer("cus_test_customer");

            context.PayoutAccounts.AddRange(
                venueManager1,
                venueManager2,
                artistManager,
                artistManagerNoArtist,
                customer);

            await context.SaveChangesAsync(ct);
        });
    }
}
