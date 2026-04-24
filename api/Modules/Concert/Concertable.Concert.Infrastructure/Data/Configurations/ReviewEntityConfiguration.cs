using Concertable.Concert.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Concert.Infrastructure.Data.Configurations;

internal class ReviewEntityConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.ToTable("Reviews", Schema.Name);
        builder.HasOne(r => r.Ticket)
            .WithMany()
            .HasForeignKey(r => r.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
