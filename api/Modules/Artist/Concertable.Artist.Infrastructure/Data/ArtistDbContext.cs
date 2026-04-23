using Microsoft.EntityFrameworkCore;

namespace Concertable.Artist.Infrastructure.Data;

internal class ArtistDbContext(DbContextOptions<ArtistDbContext> options)
    : DbContextBase(options)
{
    public DbSet<ArtistEntity> Artists => Set<ArtistEntity>();
    public DbSet<ArtistGenreEntity> ArtistGenres => Set<ArtistGenreEntity>();
    public DbSet<ArtistRatingProjection> ArtistRatingProjections => Set<ArtistRatingProjection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArtistDbContext).Assembly);

        modelBuilder.Entity<GenreEntity>().ToTable("Genres", t => t.ExcludeFromMigrations());
    }
}
