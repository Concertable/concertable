using Concertable.Data.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Data;

internal sealed class AppDbConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StripeEventEntityConfiguration());
        modelBuilder.ApplyConfiguration(new MessageEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PreferenceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TicketTransactionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementTransactionEntityConfiguration());
    }
}
