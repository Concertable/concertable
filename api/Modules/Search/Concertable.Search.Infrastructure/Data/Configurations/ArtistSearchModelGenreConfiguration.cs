using Concertable.Search.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Search.Infrastructure.Data.Configurations;

internal sealed class ArtistSearchModelGenreConfiguration : IEntityTypeConfiguration<ArtistSearchModelGenre>
{
    public void Configure(EntityTypeBuilder<ArtistSearchModelGenre> builder)
    {
        builder.ToTable("ArtistGenres");
        builder.HasKey(x => new { x.ArtistId, x.GenreId });
        builder.HasOne(x => x.Genre)
            .WithMany()
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
