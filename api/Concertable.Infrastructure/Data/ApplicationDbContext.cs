using Concertable.Core.Entities;
using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;
using SharedSchema = Concertable.Data.Infrastructure.Schema;

namespace Concertable.Infrastructure.Data;

public class ApplicationDbContext : DbContextBase
{
    private readonly IEnumerable<IEntityTypeConfigurationProvider> _moduleProviders;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IEnumerable<IEntityTypeConfigurationProvider> moduleProviders)
        : base(options)
    {
        _moduleProviders = moduleProviders;
    }

    protected ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
        _moduleProviders = [];
    }

    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<TicketTransactionEntity> TicketTransactions { get; set; }
    public DbSet<SettlementTransactionEntity> SettlementTransactions { get; set; }
    public DbSet<PreferenceEntity> Preferences { get; set; }
    public DbSet<GenrePreferenceEntity> GenrePreferences { get; set; }
    public DbSet<StripeEventEntity> StripeEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var provider in _moduleProviders)
            provider.Configure(modelBuilder);

        new AppDbConfigurationProvider().Configure(modelBuilder);

        // Shared reference data — schema managed by SharedDbContext migrations (runs first).
        modelBuilder.Entity<GenreEntity>().ToTable("Genres", SharedSchema.Name, t => t.ExcludeFromMigrations());

        // Identity-owned tables — schema managed by IdentityDbContext migrations
        modelBuilder.Entity<UserEntity>().ToTable("Users", "identity", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<RefreshTokenEntity>().ToTable("RefreshTokens", "identity", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<EmailVerificationTokenEntity>().ToTable("EmailVerificationTokens", "identity", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<PasswordResetTokenEntity>().ToTable("PasswordResetTokens", "identity", t => t.ExcludeFromMigrations());

        // Artist-owned tables — schema managed by ArtistDbContext migrations
        modelBuilder.Entity<ArtistEntity>().ToTable("Artists", "artist", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ArtistGenreEntity>().ToTable("ArtistGenres", "artist", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ArtistRatingProjection>().ToTable("ArtistRatingProjections", "artist", t => t.ExcludeFromMigrations());

        // Venue-owned tables — schema managed by VenueDbContext migrations
        modelBuilder.Entity<VenueEntity>().ToTable("Venues", "venue", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<VenueImageEntity>().ToTable("VenueImages", "venue", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<VenueRatingProjection>().ToTable("VenueRatingProjections", "venue", t => t.ExcludeFromMigrations());

        // Concert-owned tables — schema managed by ConcertDbContext migrations
        modelBuilder.Entity<ConcertEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ConcertGenreEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ConcertImageEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ConcertBookingEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ConcertRatingProjection>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<OpportunityEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<OpportunityApplicationEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<OpportunityGenreEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ContractEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<FlatFeeContractEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<DoorSplitContractEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<VersusContractEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<VenueHireContractEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<TicketEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ReviewEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ArtistReadModel>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ArtistReadModelGenre>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<VenueReadModel>().ToTable(t => t.ExcludeFromMigrations());

        // Payment-owned tables — schema managed by PaymentDbContext migrations
        modelBuilder.Entity<TransactionEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<TicketTransactionEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<SettlementTransactionEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<StripeEventEntity>().ToTable(t => t.ExcludeFromMigrations());
        modelBuilder.Entity<PayoutAccountEntity>().ToTable(t => t.ExcludeFromMigrations());
    }
}
