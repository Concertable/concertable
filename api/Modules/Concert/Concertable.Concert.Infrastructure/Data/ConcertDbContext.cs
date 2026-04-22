using Concertable.Concert.Domain;
using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Data;

internal class ConcertDbContext(DbContextOptions<ConcertDbContext> options)
    : DbContextBase(options)
{
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();
    public DbSet<ConcertBookingEntity> ConcertBookings => Set<ConcertBookingEntity>();
    public DbSet<ConcertGenreEntity> ConcertGenres => Set<ConcertGenreEntity>();
    public DbSet<ConcertImageEntity> ConcertImages => Set<ConcertImageEntity>();
    public DbSet<OpportunityEntity> Opportunities => Set<OpportunityEntity>();
    public DbSet<OpportunityApplicationEntity> OpportunityApplications => Set<OpportunityApplicationEntity>();
    public DbSet<OpportunityGenreEntity> OpportunityGenres => Set<OpportunityGenreEntity>();
    public DbSet<ContractEntity> Contracts => Set<ContractEntity>();
    public DbSet<FlatFeeContractEntity> FlatFeeContracts => Set<FlatFeeContractEntity>();
    public DbSet<DoorSplitContractEntity> DoorSplitContracts => Set<DoorSplitContractEntity>();
    public DbSet<VersusContractEntity> VersusContracts => Set<VersusContractEntity>();
    public DbSet<VenueHireContractEntity> VenueHireContracts => Set<VenueHireContractEntity>();
    public DbSet<TicketEntity> Tickets => Set<TicketEntity>();
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();
    public DbSet<ArtistReadModel> ArtistReadModels => Set<ArtistReadModel>();
    public DbSet<VenueReadModel> VenueReadModels => Set<VenueReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConcertDbContext).Assembly);
    }
}
