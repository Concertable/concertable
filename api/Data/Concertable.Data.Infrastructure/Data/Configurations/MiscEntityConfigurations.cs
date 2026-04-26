using Concertable.Core.Entities;
using Concertable.Messaging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Data.Infrastructure.Data.Configurations;

public class MessageEntityConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.ToTable("Messages");
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

