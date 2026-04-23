using Concertable.Search.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Search.Infrastructure.Data.Configurations;

internal sealed class ConcertSearchModelGenreConfiguration : IEntityTypeConfiguration<ConcertSearchModelGenre>
{
    public void Configure(EntityTypeBuilder<ConcertSearchModelGenre> builder)
    {
        builder.ToTable("ConcertSearchModelGenres");
        builder.HasKey(x => new { x.ConcertSearchModelId, x.GenreId });
    }
}
