using Concertable.Concert.Infrastructure.Data.Configurations;
using Concertable.Data.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Data;

internal sealed class ConcertConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConcertEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertGenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertImageEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertBookingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertRatingProjectionConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityApplicationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityGenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TicketEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistReadModelConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistReadModelGenreConfiguration());
        modelBuilder.ApplyConfiguration(new VenueReadModelConfiguration());
    }
}
