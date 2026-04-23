using Concertable.Data.Infrastructure;
using Concertable.Search.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Data;

internal class SearchDbContext(DbContextOptions<SearchDbContext> options)
    : DbContextBase(options)
{
    public DbSet<ArtistSearchModel> ArtistSearchModels => Set<ArtistSearchModel>();
    public DbSet<VenueSearchModel> VenueSearchModels => Set<VenueSearchModel>();
    public DbSet<ConcertSearchModel> ConcertSearchModels => Set<ConcertSearchModel>();
    public DbSet<ArtistSearchModelGenre> ArtistSearchModelGenres => Set<ArtistSearchModelGenre>();
    public DbSet<ConcertSearchModelGenre> ConcertSearchModelGenres => Set<ConcertSearchModelGenre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchDbContext).Assembly);
    }
}
