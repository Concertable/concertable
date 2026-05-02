using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.User.Infrastructure.Data.Configurations;

internal sealed class VenueManagerEntityConfiguration : IEntityTypeConfiguration<VenueManagerEntity>
{
    public void Configure(EntityTypeBuilder<VenueManagerEntity> builder)
    {
        builder.Property(x => x.VenueId);
    }
}
