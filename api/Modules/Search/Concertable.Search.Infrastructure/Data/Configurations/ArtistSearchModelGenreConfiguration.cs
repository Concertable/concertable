using Concertable.Search.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Search.Infrastructure.Data.Configurations;

internal sealed class ArtistSearchModelGenreConfiguration : IEntityTypeConfiguration<ArtistSearchModelGenre>
{
    public void Configure(EntityTypeBuilder<ArtistSearchModelGenre> builder)
    {
        builder.ToTable("ArtistSearchModelGenres");
        builder.HasKey(x => new { x.ArtistSearchModelId, x.GenreId });
        builder.HasOne(x => x.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistSearchModelId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
