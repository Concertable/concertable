using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Data;

public class ApplicationDbContext : DbContextBase
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected ApplicationDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<ConcertEntity> Concerts { get; set; }
    public DbSet<ConcertGenreEntity> ConcertGenres { get; set; }
    public DbSet<ConcertImageEntity> ConcertImages { get; set; }
    public DbSet<GenreEntity> Genres { get; set; }
    public DbSet<OpportunityEntity> Opportunities { get; set; }
    public DbSet<OpportunityGenreEntity> OpportunityGenres { get; set; }
    public DbSet<OpportunityApplicationEntity> OpportunityApplications { get; set; }
    public DbSet<ConcertBookingEntity> ConcertBookings { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<TicketEntity> Tickets { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<VenueEntity> Venues { get; set; }
    public DbSet<VenueImageEntity> VenueImages { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<TicketTransactionEntity> TicketTransactions { get; set; }
    public DbSet<SettlementTransactionEntity> SettlementTransactions { get; set; }
    public DbSet<PreferenceEntity> Preferences { get; set; }
    public DbSet<GenrePreferenceEntity> GenrePreferences { get; set; }
    public DbSet<StripeEventEntity> StripeEvents { get; set; }
    public DbSet<ContractEntity> Contracts { get; set; }
    public DbSet<FlatFeeContractEntity> FlatFeeContracts { get; set; }
    public DbSet<DoorSplitContractEntity> DoorSplitContracts { get; set; }
    public DbSet<VersusContractEntity> VersusContracts { get; set; }
    public DbSet<VenueHireContractEntity> VenueHireContracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DbContextBase).Assembly);

        // Identity-owned tables — schema managed by IdentityDbContext migrations
        modelBuilder.Entity<UserEntity>().ToTable("Users", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<RefreshTokenEntity>().ToTable("RefreshTokens", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<EmailVerificationTokenEntity>().ToTable("EmailVerificationTokens", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<PasswordResetTokenEntity>().ToTable("PasswordResetTokens", t => t.ExcludeFromMigrations());

        // Artist-owned tables — schema managed by ArtistDbContext migrations
        modelBuilder.Entity<ArtistEntity>().ToTable("Artists", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ArtistGenreEntity>().ToTable("ArtistGenres", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ArtistRatingProjection>().ToTable("ArtistRatingProjections", t => t.ExcludeFromMigrations());
    }
}
