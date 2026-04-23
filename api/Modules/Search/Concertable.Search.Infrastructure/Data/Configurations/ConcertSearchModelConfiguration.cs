using Concertable.Search.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Search.Infrastructure.Data.Configurations;

internal sealed class ConcertSearchModelConfiguration : IEntityTypeConfiguration<ConcertSearchModel>
{
    public void Configure(EntityTypeBuilder<ConcertSearchModel> builder)
    {
        builder.ToTable("ConcertSearchModels");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(x => x.Venue)
            .WithMany()
            .HasForeignKey(x => x.VenueId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasMany(x => x.Genres)
            .WithOne(x => x.Concert)
            .HasForeignKey(x => x.ConcertSearchModelId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
