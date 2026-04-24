using Microsoft.EntityFrameworkCore;
using SharedSchema = Concertable.Data.Infrastructure.Schema;

namespace Concertable.Artist.Infrastructure.Data;

internal class ArtistDbContext(
    DbContextOptions<ArtistDbContext> options,
    ArtistConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<ArtistEntity> Artists => Set<ArtistEntity>();
    public DbSet<ArtistGenreEntity> ArtistGenres => Set<ArtistGenreEntity>();
    public DbSet<ArtistRatingProjection> ArtistRatingProjections => Set<ArtistRatingProjection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);

        provider.Configure(modelBuilder);

        modelBuilder.Entity<GenreEntity>().ToTable("Genres", SharedSchema.Name, t => t.ExcludeFromMigrations());
    }
}
