using Concertable.Shared;

namespace Concertable.Artist.Application.Interfaces;

internal interface IArtistReviewService
{
    Task<ReviewSummaryDto> GetSummaryAsync(int artistId);
    Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams);
    Task<bool> CanCurrentUserReviewAsync(int artistId);
}
