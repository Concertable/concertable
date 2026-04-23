using Concertable.Concert.Domain;
using Concertable.Shared;

namespace Concertable.Concert.Infrastructure.Mappers;

internal static class ReviewSummaryMappers
{
    public static ReviewSummaryDto ToReviewSummaryDto(this ConcertRatingProjection? projection) =>
        projection is null
            ? new ReviewSummaryDto(0, null)
            : new ReviewSummaryDto(projection.ReviewCount, projection.AverageRating);
}
