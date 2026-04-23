using Concertable.Concert.Domain;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Data.Infrastructure;
using Concertable.Search.Domain.Models;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Data;

internal class SearchDbContext(DbContextOptions<SearchDbContext> options)
    : DbContextBase(options)
{
    public DbSet<ArtistSearchModel> Artists => Set<ArtistSearchModel>();
    public DbSet<VenueSearchModel> Venues => Set<VenueSearchModel>();
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConcertDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchDbContext).Assembly);
        modelBuilder.Entity<GenreEntity>().ToTable("Genres", t => t.ExcludeFromMigrations());
    }
}
