using Concertable.Artist.Domain;
using Concertable.Artist.Infrastructure.Data.Configurations;
using ArtistSchema = Concertable.Artist.Infrastructure.Schema;
using VenueSchema = Concertable.Venue.Infrastructure.Schema;
using Concertable.Concert.Domain;
using Concertable.Data.Infrastructure;
using Concertable.Search.Domain.Models;
using SharedSchema = Concertable.Data.Infrastructure.Schema;
using Concertable.Shared;
using Concertable.Venue.Domain;
using Concertable.Venue.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Data;

internal class SearchDbContext(DbContextOptions<SearchDbContext> options)
    : DbContextBase(options), ISearchDbContext
{
    IQueryable<ArtistSearchModel> ISearchDbContext.Artists => Set<ArtistSearchModel>();
    IQueryable<VenueSearchModel> ISearchDbContext.Venues => Set<VenueSearchModel>();
    IQueryable<ConcertSearchModel> ISearchDbContext.Concerts => Set<ConcertSearchModel>();
    IQueryable<ArtistRatingProjection> ISearchDbContext.ArtistRatingProjections => Set<ArtistRatingProjection>();
    IQueryable<VenueRatingProjection> ISearchDbContext.VenueRatingProjections => Set<VenueRatingProjection>();
    IQueryable<ConcertRatingProjection> ISearchDbContext.ConcertRatingProjections => Set<ConcertRatingProjection>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    public override int SaveChanges() =>
        throw new InvalidOperationException($"{nameof(SearchDbContext)} is read-only.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new InvalidOperationException($"{nameof(SearchDbContext)} is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"{nameof(SearchDbContext)} is read-only.");

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"{nameof(SearchDbContext)} is read-only.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchDbContext).Assembly);
        modelBuilder.Entity<GenreEntity>().ToTable("Genres", SharedSchema.Name, t => t.ExcludeFromMigrations());

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ArtistRatingProjectionConfiguration).Assembly,
            t => t == typeof(ArtistRatingProjectionConfiguration));
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(VenueRatingProjectionConfiguration).Assembly,
            t => t == typeof(VenueRatingProjectionConfiguration));

        modelBuilder.Entity<ArtistRatingProjection>()
            .ToTable("ArtistRatingProjections", ArtistSchema.Name, t => t.ExcludeFromMigrations());
        modelBuilder.Entity<VenueRatingProjection>()
            .ToTable("VenueRatingProjections", VenueSchema.Name, t => t.ExcludeFromMigrations());

        modelBuilder.Entity<ConcertRatingProjection>(b =>
        {
            b.ToTable("ConcertRatingProjections", "concert", t => t.ExcludeFromMigrations());
            b.HasKey(p => p.ConcertId);
        });
    }
}
