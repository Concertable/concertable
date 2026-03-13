using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Identity;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Artist> Artists { get; set; }
    public DbSet<ArtistGenre> ArtistGenres { get; set; }
    public DbSet<Concert> Concerts { get; set; }
    public DbSet<ConcertGenre> ConcertGenres { get; set; }
    public DbSet<ConcertImage> ConcertImages { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<ConcertOpportunity> ConcertOpportunities { get; set; }
    public DbSet<OpportunityGenre> OpportunityGenres { get; set; }
    public DbSet<ConcertApplication> ConcertApplications { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<SocialMedia> SocialMedias { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<VenueImage> VenueImages { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Preference> Preferences { get; set; }
    public DbSet<GenrePreference> GenrePreferences { get; set; }
    public DbSet<StripeEvent> StripeEvents { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.Property(u => u.Location).HasColumnType("geography");
            e.HasIndex(u => u.Email).IsUnique();
            e.HasDiscriminator(u => u.Role)
                .HasValue<User>(Role.Admin)
                .HasValue<VenueManager>(Role.VenueManager)
                .HasValue<ArtistManager>(Role.ArtistManager)
                .HasValue<Customer>(Role.Customer);
        });

        modelBuilder.Entity<StripeEvent>()
            .HasKey(e => e.EventId);

        modelBuilder.Entity<ConcertApplication>()
            .HasIndex(ca => new { ca.OpportunityId, ca.ArtistId })
            .IsUnique();

        modelBuilder.Entity<ConcertApplication>()
            .Ignore(ca => ca.Concert);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.FromUser)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.ToUser)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(p => p.FromUser)
            .WithMany()
            .HasForeignKey(p => p.FromUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Transaction>()
            .HasOne(p => p.ToUser)
            .WithMany()
            .HasForeignKey(p => p.ToUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Concert>()
            .HasOne(e => e.Application)
            .WithOne()
            .HasForeignKey<Concert>(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ConcertGenre>()
            .HasOne(cg => cg.Concert)
            .WithMany(c => c.ConcertGenres)
            .HasForeignKey(cg => cg.ConcertId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ConcertGenre>()
            .HasOne(cg => cg.Genre)
            .WithMany(g => g.ConcertGenres)
            .HasForeignKey(cg => cg.GenreId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ConcertImage>()
            .HasOne(ci => ci.Concert)
            .WithMany(c => c.Images)
            .HasForeignKey(ci => ci.ConcertId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ConcertApplication>()
            .HasOne(ca => ca.Opportunity)
            .WithMany(o => o.Applications)
            .HasForeignKey(ca => ca.OpportunityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ConcertApplication>()
            .HasOne(ca => ca.Artist)
            .WithMany(a => a.Applications)
            .HasForeignKey(ca => ca.ArtistId)
            .IsRequired();

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Concert)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.ConcertId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.UserId)
            .IsRequired();

        modelBuilder.Entity<Preference>()
            .HasOne(p => p.User)
            .WithOne(u => u.Preference)
            .HasForeignKey<Preference>(p => p.UserId)
            .IsRequired();

        modelBuilder.Entity<ArtistManager>()
            .HasOne(am => am.Artist)
            .WithOne(a => a.User)
            .HasForeignKey<Artist>(a => a.UserId)
            .IsRequired();

        modelBuilder.Entity<VenueManager>()
            .HasOne(vm => vm.Venue)
            .WithOne(v => v.User)
            .HasForeignKey<Venue>(v => v.UserId)
            .IsRequired();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
