using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.User.Infrastructure.Data.Configurations;

internal sealed class ArtistManagerEntityConfiguration : IEntityTypeConfiguration<ArtistManagerEntity>
{
    public void Configure(EntityTypeBuilder<ArtistManagerEntity> builder)
    {
        builder.Property(x => x.ArtistId);
    }
}
