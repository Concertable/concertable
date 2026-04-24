using Concertable.Search.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Search.Infrastructure.Data.Configurations;

internal sealed class ConcertSearchModelGenreConfiguration : IEntityTypeConfiguration<ConcertSearchModelGenre>
{
    public void Configure(EntityTypeBuilder<ConcertSearchModelGenre> builder)
    {
        builder.ToTable("ConcertGenres", t => t.ExcludeFromMigrations());
        builder.HasKey(x => new { x.ConcertId, x.GenreId });
        builder.HasOne(x => x.Genre)
            .WithMany()
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
