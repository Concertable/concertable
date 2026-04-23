using Concertable.Concert.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Concert.Infrastructure.Data.Configurations;

internal class ConcertEntityConfiguration : IEntityTypeConfiguration<ConcertEntity>
{
    public void Configure(EntityTypeBuilder<ConcertEntity> builder)
    {
        builder.ToTable("Concerts");
        builder.HasOne(e => e.Booking)
            .WithOne(b => b.Concert)
            .HasForeignKey<ConcertEntity>(e => e.BookingId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
