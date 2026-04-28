using Concertable.User.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Data.Infrastructure.Data.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users", "identity");
        builder.Property(u => u.Location).HasColumnType("geography");
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasDiscriminator(u => u.Role)
            .HasValue<UserEntity>(Role.Admin)
            .HasValue<VenueManagerEntity>(Role.VenueManager)
            .HasValue<ArtistManagerEntity>(Role.ArtistManager)
            .HasValue<CustomerEntity>(Role.Customer);
        builder.OwnsOne(u => u.Address, a =>
        {
            a.Property(x => x.County).HasColumnName("County");
            a.Property(x => x.Town).HasColumnName("Town");
        });
    }
}

public class CustomerEntityConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder) { }
}

public class ManagerEntityConfiguration : IEntityTypeConfiguration<ManagerEntity>
{
    public void Configure(EntityTypeBuilder<ManagerEntity> builder) { }
}
