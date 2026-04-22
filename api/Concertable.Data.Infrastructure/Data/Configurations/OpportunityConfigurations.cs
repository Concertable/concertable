using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Data.Infrastructure.Data.Configurations;

public class OpportunityEntityConfiguration : IEntityTypeConfiguration<OpportunityEntity>
{
    public void Configure(EntityTypeBuilder<OpportunityEntity> builder)
    {
        builder.ToTable("Opportunities");
        builder.OwnsOne(o => o.Period, p =>
        {
            p.Property(x => x.Start).HasColumnName("StartDate");
            p.Property(x => x.End).HasColumnName("EndDate");
        });
        builder.HasOne(o => o.Venue)
            .WithMany()
            .HasForeignKey(o => o.VenueId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class OpportunityApplicationEntityConfiguration : IEntityTypeConfiguration<OpportunityApplicationEntity>
{
    public void Configure(EntityTypeBuilder<OpportunityApplicationEntity> builder)
    {
        builder.ToTable("OpportunityApplications");
        builder.HasIndex(ca => new { ca.OpportunityId, ca.ArtistId }).IsUnique();
        builder.HasOne(ca => ca.Opportunity)
            .WithMany(o => o.Applications)
            .HasForeignKey(ca => ca.OpportunityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(ca => ca.Artist)
            .WithMany()
            .HasForeignKey(ca => ca.ArtistId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
