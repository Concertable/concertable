using Concertable.Concert.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Concert.Infrastructure.Data.Configurations;

internal class TicketEntityConfiguration : IEntityTypeConfiguration<TicketEntity>
{
    public void Configure(EntityTypeBuilder<TicketEntity> builder)
    {
        builder.ToTable("Tickets", Schema.Name);
        builder.HasOne(t => t.Concert)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.ConcertId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
