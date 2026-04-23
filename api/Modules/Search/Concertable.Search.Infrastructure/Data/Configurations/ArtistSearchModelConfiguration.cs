using Concertable.Search.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Search.Infrastructure.Data.Configurations;

internal sealed class ArtistSearchModelConfiguration : IEntityTypeConfiguration<ArtistSearchModel>
{
    public void Configure(EntityTypeBuilder<ArtistSearchModel> builder)
    {
        builder.ToTable("ArtistSearchModels");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired();
    }
}
