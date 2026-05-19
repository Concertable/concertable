using Concertable.DataAccess.Infrastructure;
using Concertable.Search.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Data;

internal sealed class SearchConfigurationProvider(
    IEnumerable<IRatingProjectionConfigurationProvider> ratingProviders)
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ArtistSearchModelConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistSearchModelGenreConfiguration());
        modelBuilder.ApplyConfiguration(new VenueSearchModelConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertSearchModelConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertSearchModelGenreConfiguration());

        foreach (var provider in ratingProviders)
            provider.Configure(modelBuilder);
    }
}
