using Concertable.Core.Entities.Contracts;
using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Identity;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    public DbSet<ArtistEntity> Artists { get; set; }
    public DbSet<ArtistGenreEntity> ArtistGenres { get; set; }
    public DbSet<ConcertEntity> Concerts { get; set; }
    public DbSet<ConcertGenreEntity> ConcertGenres { get; set; }
    public DbSet<ConcertImageEntity> ConcertImages { get; set; }
    public DbSet<GenreEntity> Genres { get; set; }
    public DbSet<ConcertOpportunityEntity> ConcertOpportunities { get; set; }
    public DbSet<OpportunityGenreEntity> OpportunityGenres { get; set; }
    public DbSet<ConcertApplicationEntity> ConcertApplications { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<SocialMediaEntity> SocialMedias { get; set; }
    public DbSet<TicketEntity> Tickets { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<VenueEntity> Venues { get; set; }
    public DbSet<VenueImageEntity> VenueImages { get; set; }
    public DbSet<VideoEntity> Videos { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<PreferenceEntity> Preferences { get; set; }
    public DbSet<GenrePreferenceEntity> GenrePreferences { get; set; }
    public DbSet<StripeEventEntity> StripeEvents { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    public DbSet<ContractEntity> Contracts { get; set; }
    public DbSet<FlatFeeContractEntity> FlatFeeContracts { get; set; }
    public DbSet<DoorSplitContractEntity> DoorSplitContracts { get; set; }
    public DbSet<VersusContractEntity> VersusContracts { get; set; }
    public DbSet<VenueHireContractEntity> VenueHireContracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.ToTable("Users");
            e.Property(u => u.Location).HasColumnType("geography");
            e.HasIndex(u => u.Email).IsUnique();
            e.HasDiscriminator(u => u.Role)
                .HasValue<UserEntity>(Role.Admin)
                .HasValue<VenueManagerEntity>(Role.VenueManager)
                .HasValue<ArtistManagerEntity>(Role.ArtistManager)
                .HasValue<CustomerEntity>(Role.Customer);
        });

        modelBuilder.Entity<StripeEventEntity>()
            .HasKey(e => e.EventId);

        modelBuilder.Entity<ConcertApplicationEntity>(e =>
        {
            e.HasIndex(ca => new { ca.OpportunityId, ca.ArtistId }).IsUnique();
        });

        modelBuilder.Entity<MessageEntity>()
            .HasOne(m => m.FromUser)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MessageEntity>()
            .HasOne(m => m.ToUser)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TransactionEntity>()
            .HasOne(p => p.FromUser)
            .WithMany()
            .HasForeignKey(p => p.FromUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TransactionEntity>()
            .HasOne(p => p.ToUser)
            .WithMany()
            .HasForeignKey(p => p.ToUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ConcertEntity>()
            .HasOne(e => e.Application)
            .WithOne()
            .HasForeignKey<ConcertEntity>(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ConcertGenreEntity>()
            .HasOne(cg => cg.Concert)
            .WithMany(c => c.ConcertGenres)
            .HasForeignKey(cg => cg.ConcertId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ConcertGenreEntity>()
            .HasOne(cg => cg.Genre)
            .WithMany(g => g.ConcertGenres)
            .HasForeignKey(cg => cg.GenreId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ConcertImageEntity>()
            .HasOne(ci => ci.Concert)
            .WithMany(c => c.Images)
            .HasForeignKey(ci => ci.ConcertId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ConcertApplicationEntity>()
            .HasOne(ca => ca.Opportunity)
            .WithMany(o => o.Applications)
            .HasForeignKey(ca => ca.OpportunityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ConcertApplicationEntity>()
            .HasOne(ca => ca.Artist)
            .WithMany(a => a.Applications)
            .HasForeignKey(ca => ca.ArtistId)
            .IsRequired();

        modelBuilder.Entity<TicketEntity>()
            .HasOne(t => t.Concert)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.ConcertId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TicketEntity>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.UserId)
            .IsRequired();

        modelBuilder.Entity<PreferenceEntity>()
            .HasOne(p => p.User)
            .WithOne(u => u.Preference)
            .HasForeignKey<PreferenceEntity>(p => p.UserId)
            .IsRequired();

        modelBuilder.Entity<ArtistManagerEntity>()
            .HasOne(am => am.Artist)
            .WithOne(a => a.User)
            .HasForeignKey<ArtistEntity>(a => a.UserId)
            .IsRequired();

        modelBuilder.Entity<VenueManagerEntity>()
            .HasOne(vm => vm.Venue)
            .WithOne(v => v.User)
            .HasForeignKey<VenueEntity>(v => v.UserId)
            .IsRequired();

        modelBuilder.Entity<RefreshTokenEntity>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ContractEntity>()
            .UseTptMappingStrategy()
            .HasOne(c => c.Opportunity)
            .WithOne(o => o.Contract)
            .HasForeignKey<ContractEntity>(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
