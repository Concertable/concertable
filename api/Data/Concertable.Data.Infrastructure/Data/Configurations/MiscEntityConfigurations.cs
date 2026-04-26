using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Data.Infrastructure.Data.Configurations;

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
