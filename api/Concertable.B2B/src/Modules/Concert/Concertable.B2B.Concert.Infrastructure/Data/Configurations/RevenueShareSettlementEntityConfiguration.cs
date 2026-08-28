using Concertable.B2B.Concert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.B2B.Concert.Infrastructure.Data.Configurations;

internal sealed class RevenueShareSettlementEntityConfiguration : IEntityTypeConfiguration<RevenueShareSettlementEntity>
{
    public void Configure(EntityTypeBuilder<RevenueShareSettlementEntity> builder)
    {
        builder.ToTable(Schema.Tables.RevenueShareSettlements, Schema.Name);

        // The payer review is one all-or-nothing value: both columns null until FreezeReviewedGross, both set after.
        builder.OwnsOne(e => e.Review);

        // One record per concert, and it exists only for a revenue-share deal — no navigation on the
        // concert side, so deal-type-specific settlement data never accretes onto ConcertEntity. The 1:1
        // relationship makes the ConcertId FK unique on its own.
        builder.HasOne<ConcertEntity>()
            .WithOne()
            .HasForeignKey<RevenueShareSettlementEntity>(e => e.ConcertId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
