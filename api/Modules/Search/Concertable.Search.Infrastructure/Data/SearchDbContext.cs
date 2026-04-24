using Concertable.Artist.Domain;
using Concertable.Concert.Domain;
using Concertable.Data.Infrastructure;
using Concertable.Search.Domain.Models;
using Concertable.Venue.Domain;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Data;

internal class SearchDbContext(
    DbContextOptions<SearchDbContext> options,
    SearchConfigurationProvider provider)
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
        => provider.Configure(modelBuilder);
}
