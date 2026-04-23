using Concertable.Concert.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Concert.Infrastructure.Data.Configurations;

internal class ConcertRatingProjectionConfiguration : IEntityTypeConfiguration<ConcertRatingProjection>
{
    public void Configure(EntityTypeBuilder<ConcertRatingProjection> builder)
    {
        builder.ToTable("ConcertRatingProjections");
        builder.HasKey(p => p.ConcertId);
        builder.Property(p => p.ConcertId).ValueGeneratedNever();
    }
}
