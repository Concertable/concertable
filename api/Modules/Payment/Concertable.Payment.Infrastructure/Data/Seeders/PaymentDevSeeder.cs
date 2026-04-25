using Concertable.Application.Interfaces;
using Concertable.Payment.Infrastructure.Data;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Payment.Infrastructure.Data.Seeders;

internal class PaymentDevSeeder : IDevSeeder
{
    public int Order => 5;

    private readonly PaymentDbContext context;
    private readonly SeedData seedData;

    public PaymentDevSeeder(PaymentDbContext context, SeedData seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.PayoutAccounts.SeedIfEmptyAsync(async () =>
        {
            foreach (var customerId in seedData.CustomerIds)
            {
                var account = PayoutAccountEntity.Create(customerId);
                account.LinkCustomer($"cus_dev_{customerId:N}");
                context.PayoutAccounts.Add(account);
            }

            for (int i = 0; i < seedData.ArtistManagerIds.Count; i++)
            {
                var managerId = seedData.ArtistManagerIds[i];
                var account = PayoutAccountEntity.Create(managerId);
                account.LinkAccount($"acct_dev_artist_{i + 1}");
                account.LinkCustomer($"cus_dev_artist_{i + 1}");
                account.MarkVerified();
                context.PayoutAccounts.Add(account);
            }

            for (int i = 0; i < seedData.VenueManagerIds.Count; i++)
            {
                var managerId = seedData.VenueManagerIds[i];
                var account = PayoutAccountEntity.Create(managerId);
                account.LinkAccount($"acct_dev_venue_{i + 1}");
                account.LinkCustomer($"cus_dev_venue_{i + 1}");
                account.MarkVerified();
                context.PayoutAccounts.Add(account);
            }

            await context.SaveChangesAsync(ct);
        });
    }
}
