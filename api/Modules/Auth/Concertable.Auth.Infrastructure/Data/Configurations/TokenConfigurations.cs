using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Auth.Infrastructure.Data.Configurations;

internal class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("RefreshTokens", Schema.Name);
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.Token).IsUnique();
    }
}

internal class EmailVerificationTokenEntityConfiguration : IEntityTypeConfiguration<EmailVerificationTokenEntity>
{
    public void Configure(EntityTypeBuilder<EmailVerificationTokenEntity> builder)
    {
        builder.ToTable("EmailVerificationTokens", Schema.Name);
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.Token).IsUnique();
    }
}

internal class PasswordResetTokenEntityConfiguration : IEntityTypeConfiguration<PasswordResetTokenEntity>
{
    public void Configure(EntityTypeBuilder<PasswordResetTokenEntity> builder)
    {
        builder.ToTable("PasswordResetTokens", Schema.Name);
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.Token).IsUnique();
    }
}
