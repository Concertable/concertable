using Concertable.Artist.Infrastructure.Data.Configurations;
using Concertable.Data.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Artist.Infrastructure.Data;

internal sealed class ArtistConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ArtistEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistGenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistRatingProjectionConfiguration());
    }
}
