using Concertable.Concert.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Concert.Infrastructure.Data.Configurations;

internal class ConcertLifecycleEntityConfiguration : IEntityTypeConfiguration<ConcertLifecycleEntity>
{
    public void Configure(EntityTypeBuilder<ConcertLifecycleEntity> builder)
    {
        builder.ToTable("ConcertLifecycles", Schema.Name);
        builder.Property(e => e.Stage).HasConversion<int>();
        builder.HasIndex(e => new { e.OpportunityId, e.ArtistId });
    }
}
