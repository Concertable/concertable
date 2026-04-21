using Concertable.Artist.Domain;
using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data.Configurations;
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
        modelBuilder.ApplyConfiguration(new ArtistEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistGenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistRatingProjectionConfiguration());
    }
}
