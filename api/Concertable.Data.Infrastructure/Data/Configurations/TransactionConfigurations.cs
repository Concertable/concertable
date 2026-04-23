using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Data.Infrastructure.Data.Configurations;

public class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
{
    public void Configure(EntityTypeBuilder<TransactionEntity> builder)
    {
        builder.ToTable("Transactions");
        builder.UseTptMappingStrategy();
        builder.HasIndex(t => t.PaymentIntentId).IsUnique();
        builder.HasOne(p => p.FromUser)
            .WithMany()
            .HasForeignKey(p => p.FromUserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(p => p.ToUser)
            .WithMany()
            .HasForeignKey(p => p.ToUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class TicketTransactionEntityConfiguration : IEntityTypeConfiguration<TicketTransactionEntity>
{
    public void Configure(EntityTypeBuilder<TicketTransactionEntity> builder)
    {
        builder.ToTable("TicketTransactions");
    }
}

public class SettlementTransactionEntityConfiguration : IEntityTypeConfiguration<SettlementTransactionEntity>
{
    public void Configure(EntityTypeBuilder<SettlementTransactionEntity> builder)
    {
        builder.ToTable("SettlementTransactions");
    }
}
