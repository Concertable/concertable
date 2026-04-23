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

