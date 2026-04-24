using Concertable.Data.Infrastructure.Data;
using Concertable.Search.Infrastructure.Data.Configurations;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;
using SharedSchema = Concertable.Data.Infrastructure.Schema;

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

        modelBuilder.Entity<GenreEntity>().ToTable("Genres", SharedSchema.Name, t => t.ExcludeFromMigrations());
    }
}
