using Concertable.Shared;

namespace Concertable.Concert.Application.Interfaces.Reviews;

internal interface IVenueReviewRepository
{
    Task<IPagination<ReviewDto>> GetByVenueAsync(int venueId, IPageParams pageParams);
    Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);
}
