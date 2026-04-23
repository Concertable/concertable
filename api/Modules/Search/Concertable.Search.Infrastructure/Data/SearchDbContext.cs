using Concertable.Artist.Domain;
using Concertable.Artist.Infrastructure.Data.Configurations;
using Concertable.Concert.Domain;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Data.Infrastructure;
using Concertable.Search.Domain.Models;
using Concertable.Shared;
using Concertable.Venue.Domain;
using Concertable.Venue.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Data;

internal class SearchDbContext(DbContextOptions<SearchDbContext> options)
    : DbContextBase(options)
{
    public DbSet<ArtistSearchModel> Artists => Set<ArtistSearchModel>();
    public DbSet<VenueSearchModel> Venues => Set<VenueSearchModel>();
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();
    public DbSet<ArtistRatingProjection> ArtistRatingProjections => Set<ArtistRatingProjection>();
    public DbSet<VenueRatingProjection> VenueRatingProjections => Set<VenueRatingProjection>();
    public DbSet<ConcertRatingProjection> ConcertRatingProjections => Set<ConcertRatingProjection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConcertDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchDbContext).Assembly);
        modelBuilder.Entity<GenreEntity>().ToTable("Genres", t => t.ExcludeFromMigrations());

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ArtistRatingProjectionConfiguration).Assembly,
            t => t == typeof(ArtistRatingProjectionConfiguration));
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(VenueRatingProjectionConfiguration).Assembly,
            t => t == typeof(VenueRatingProjectionConfiguration));

        modelBuilder.Entity<ArtistRatingProjection>().ToTable("ArtistRatingProjections", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<VenueRatingProjection>().ToTable("VenueRatingProjections", t => t.ExcludeFromMigrations());
    }
}
