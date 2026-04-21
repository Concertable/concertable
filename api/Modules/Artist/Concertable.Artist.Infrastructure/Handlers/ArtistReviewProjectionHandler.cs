using Concertable.Artist.Domain;
using Concertable.Artist.Infrastructure.Data;
using Concertable.Concert.Contracts.Events;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Artist.Infrastructure.Handlers;

internal class ArtistReviewProjectionHandler(ArtistDbContext context)
    : IIntegrationEventHandler<ReviewSubmittedEvent>
{
    public async Task HandleAsync(ReviewSubmittedEvent e, CancellationToken ct = default)
    {
        var projection = await context.ArtistRatingProjections
            .FirstOrDefaultAsync(p => p.ArtistId == e.ArtistId, ct);

        if (projection is null)
        {
            context.ArtistRatingProjections.Add(new ArtistRatingProjection
            {
                ArtistId = e.ArtistId,
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
