using Concertable.Application.Interfaces;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Contract.Infrastructure.Data.Seeders;

internal class ContractTestSeeder : ITestSeeder
{
    public int Order => 3;

    private readonly ContractDbContext context;

    public ContractTestSeeder(ContractDbContext context)
    {
        this.context = context;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Contracts.SeedIfEmptyAsync(async () =>
        {
            var contracts = new ContractEntity[]
            {
                FlatFeeContractEntity.Create(100m, PaymentMethod.Cash),         // 1  — FlatFeeApp
                FlatFeeContractEntity.Create(100m, PaymentMethod.Cash),         // 2  — ConfirmedApp
                FlatFeeContractEntity.Create(100m, PaymentMethod.Cash),         // 3  — AwaitingPaymentApp
                VersusContractEntity.Create(100m, 70m, PaymentMethod.Cash),     // 4  — VersusApp
                DoorSplitContractEntity.Create(70m, PaymentMethod.Cash),        // 5  — DoorSplitApp
                VenueHireContractEntity.Create(150m, PaymentMethod.Cash),       // 6  — VenueHireApp
                FlatFeeContractEntity.Create(100m, PaymentMethod.Cash),         // 7  — PostedFlatFeeApp
                DoorSplitContractEntity.Create(70m, PaymentMethod.Cash),        // 8  — PostedDoorSplitApp
                VersusContractEntity.Create(100m, 70m, PaymentMethod.Cash),     // 9  — PostedVersusApp
                VenueHireContractEntity.Create(150m, PaymentMethod.Cash),       // 10 — PostedVenueHireApp
            };

            context.Contracts.AddRange(contracts);
            await context.SaveChangesAsync(ct);
        });
    }
}
