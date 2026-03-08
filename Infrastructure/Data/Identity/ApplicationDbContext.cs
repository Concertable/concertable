using Core.Entities;
using Core.Entities.Identity;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Metadata;

namespace Infrastructure.Data.Identity
{
    public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, int>(options)
    {
        public DbSet<Artist> Artists { get; set; }
        public DbSet<ArtistGenre> ArtistGenres { get; set; }
        public DbSet<Concert> Concerts { get; set; }
        public DbSet<ConcertGenre> ConcertGenres { get; set; }
        public DbSet<ConcertImage> ConcertImages { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingGenre> ListingGenres { get; set; }
        public DbSet<ListingApplication> ListingApplications { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SocialMedia> SocialMedias { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<VenueImage> VenueImages { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Preference> Preferences { get; set; }
        public DbSet<GenrePreference> GenrePreferences { get; set; }
        public DbSet<StripeEvent> StripeEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Location)
                .HasColumnType("geography");

            modelBuilder.Entity<StripeEvent>()
                .HasKey(e => e.EventId);

            /* Ensure an artist can only register for each listing once */
            modelBuilder.Entity<ListingApplication>()
                .HasIndex(la => new { la.ListingId, la.ArtistId })
                .IsUnique();

            modelBuilder.Entity<ListingApplication>()
                .Ignore(l => l.Concert);

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

            modelBuilder.Entity<ListingApplication>()
                .HasOne(r => r.Listing)
                .WithMany(l => l.Applications)
                .HasForeignKey(r => r.ListingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ListingApplication>()
                .HasOne(r => r.Artist)
                .WithMany(a => a.Applications)
                .HasForeignKey(r => r.ArtistId)
                .IsRequired();

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Concert)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.ConcertId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.ApplyConfiguration(new RoleConfiguration());
        }

    }
}
