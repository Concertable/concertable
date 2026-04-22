using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Data.Infrastructure.Data.Configurations;

public class StripeEventEntityConfiguration : IEntityTypeConfiguration<StripeEventEntity>
{
    public void Configure(EntityTypeBuilder<StripeEventEntity> builder)
    {
        builder.ToTable("StripeEvents");
        builder.HasKey(e => e.EventId);
    }
}

public class MessageEntityConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.ToTable("Messages");
        builder.HasOne(m => m.FromUser)
            .WithMany()
            .HasForeignKey(m => m.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.ToUser)
            .WithMany()
            .HasForeignKey(m => m.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TicketEntityConfiguration : IEntityTypeConfiguration<TicketEntity>
{
    public void Configure(EntityTypeBuilder<TicketEntity> builder)
    {
        builder.ToTable("Tickets");
        builder.HasOne(t => t.Concert)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.ConcertId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .IsRequired();
    }
}

public class PreferenceEntityConfiguration : IEntityTypeConfiguration<PreferenceEntity>
{
    public void Configure(EntityTypeBuilder<PreferenceEntity> builder)
    {
        builder.ToTable("Preferences");
        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<PreferenceEntity>(p => p.UserId)
            .IsRequired();
    }
}

public class ConcertEntityConfiguration : IEntityTypeConfiguration<ConcertEntity>
{
    public void Configure(EntityTypeBuilder<ConcertEntity> builder)
    {
        builder.ToTable("Concerts");
        builder.HasOne(e => e.Booking)
            .WithOne(b => b.Concert)
            .HasForeignKey<ConcertEntity>(e => e.BookingId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ConcertGenreEntityConfiguration : IEntityTypeConfiguration<ConcertGenreEntity>
{
    public void Configure(EntityTypeBuilder<ConcertGenreEntity> builder)
    {
        builder.ToTable("ConcertGenres");
        builder.HasOne(cg => cg.Concert)
            .WithMany(c => c.ConcertGenres)
            .HasForeignKey(cg => cg.ConcertId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.HasOne(cg => cg.Genre)
            .WithMany()
            .HasForeignKey(cg => cg.GenreId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}

public class ConcertImageEntityConfiguration : IEntityTypeConfiguration<ConcertImageEntity>
{
    public void Configure(EntityTypeBuilder<ConcertImageEntity> builder)
    {
        builder.ToTable("ConcertImages");
        builder.HasOne(ci => ci.Concert)
            .WithMany(c => c.Images)
            .HasForeignKey(ci => ci.ConcertId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
