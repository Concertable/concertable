using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Payment.Infrastructure.Data.Configurations;

internal class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
{
    public void Configure(EntityTypeBuilder<TransactionEntity> builder)
    {
        builder.ToTable("Transactions", Schema.Name);
        builder.UseTptMappingStrategy();
        builder.HasIndex(t => t.PaymentIntentId).IsUnique();
        builder.HasIndex(t => t.FromUserId);
        builder.HasIndex(t => t.ToUserId);
    }
}

internal class TicketTransactionEntityConfiguration : IEntityTypeConfiguration<TicketTransactionEntity>
{
    public void Configure(EntityTypeBuilder<TicketTransactionEntity> builder)
    {
        builder.ToTable("TicketTransactions", Schema.Name);
    }
}

internal class SettlementTransactionEntityConfiguration : IEntityTypeConfiguration<SettlementTransactionEntity>
{
    public void Configure(EntityTypeBuilder<SettlementTransactionEntity> builder)
    {
        builder.ToTable("SettlementTransactions", Schema.Name);
    }
}
