using Concertable.Shared;

namespace Concertable.Concert.Contracts;

public interface IConcertModule
{
    Task<IPagination<ReviewDto>> GetReviewsByArtistAsync(int artistId, IPageParams pageParams);
    Task<IPagination<ReviewDto>> GetReviewsByVenueAsync(int venueId, IPageParams pageParams);
    Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);
}
