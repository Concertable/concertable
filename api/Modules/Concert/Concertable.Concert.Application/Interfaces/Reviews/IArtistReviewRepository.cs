using Concertable.Shared;

namespace Concertable.Concert.Application.Interfaces.Reviews;

internal interface IArtistReviewRepository
{
    Task<IPagination<ReviewDto>> GetByArtistAsync(int artistId, IPageParams pageParams);
    Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
}
