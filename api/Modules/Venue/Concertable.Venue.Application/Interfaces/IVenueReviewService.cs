using Concertable.Shared;

namespace Concertable.Venue.Application.Interfaces;

internal interface IVenueReviewService
{
    Task<ReviewSummaryDto> GetSummaryAsync(int venueId);
    Task<IPagination<ReviewDto>> GetAsync(int venueId, IPageParams pageParams);
    Task<bool> CanCurrentUserReviewAsync(int venueId);
}
