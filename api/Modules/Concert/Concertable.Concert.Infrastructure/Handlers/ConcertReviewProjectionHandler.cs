using Concertable.Concert.Contracts.Events;
using Concertable.Concert.Domain;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Handlers;

internal class ConcertReviewProjectionHandler(ConcertDbContext context)
    : IIntegrationEventHandler<ReviewSubmittedEvent>
{
    public async Task HandleAsync(ReviewSubmittedEvent e, CancellationToken ct = default)
    {
        var projection = await context.ConcertRatingProjections
            .FirstOrDefaultAsync(p => p.ConcertId == e.ConcertId, ct);

        if (projection is null)
        {
            context.ConcertRatingProjections.Add(new ConcertRatingProjection
            {
                ConcertId = e.ConcertId,
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
