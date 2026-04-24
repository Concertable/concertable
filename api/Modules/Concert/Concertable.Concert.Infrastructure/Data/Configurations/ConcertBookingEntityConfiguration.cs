using Concertable.Concert.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Concert.Infrastructure.Data.Configurations;

internal class ConcertBookingEntityConfiguration : IEntityTypeConfiguration<ConcertBookingEntity>
{
    public void Configure(EntityTypeBuilder<ConcertBookingEntity> builder)
    {
        builder.ToTable("ConcertBookings", Schema.Name);
        builder.HasOne(b => b.Application)
            .WithOne(a => a.Booking)
            .HasForeignKey<ConcertBookingEntity>(b => b.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
