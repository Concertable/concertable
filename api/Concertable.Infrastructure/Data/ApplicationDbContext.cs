using Concertable.Core.Entities;
using Concertable.Data.Infrastructure;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;
using SharedSchema = Concertable.Data.Infrastructure.Schema;

namespace Concertable.Infrastructure.Data;

public class ApplicationDbContext : DbContextBase
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected ApplicationDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<TicketTransactionEntity> TicketTransactions { get; set; }
    public DbSet<SettlementTransactionEntity> SettlementTransactions { get; set; }
    public DbSet<PreferenceEntity> Preferences { get; set; }
    public DbSet<GenrePreferenceEntity> GenrePreferences { get; set; }
    public DbSet<StripeEventEntity> StripeEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name is string n
                     && n.StartsWith("Concertable.")
                     && n.EndsWith(".Infrastructure")
                     && n != "Concertable.Concert.Infrastructure"
                     && n != "Concertable.Search.Infrastructure"))
        {
            modelBuilder.ApplyConfigurationsFromAssembly(asm);
        }

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
    }
}
