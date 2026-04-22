using Concertable.Concert.Contracts.Events;
using Concertable.Venue.Domain;
using Concertable.Venue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Venue.Infrastructure.Handlers;

internal class VenueReviewProjectionHandler(VenueDbContext context)
    : IIntegrationEventHandler<ReviewSubmittedEvent>
{
    public async Task HandleAsync(ReviewSubmittedEvent e, CancellationToken ct = default)
    {
        var projection = await context.VenueRatingProjections
            .FirstOrDefaultAsync(p => p.VenueId == e.VenueId, ct);

        if (projection is null)
        {
            context.VenueRatingProjections.Add(new VenueRatingProjection
            {
                VenueId = e.VenueId,
                AverageRating = e.Stars,
                ReviewCount = 1
            });
        }
        else
        {
            var total = projection.AverageRating * projection.ReviewCount + e.Stars;
            projection.ReviewCount++;
            projection.AverageRating = Math.Round(total / projection.ReviewCount, 1);
        }

        await context.SaveChangesAsync(ct);
    }
}
