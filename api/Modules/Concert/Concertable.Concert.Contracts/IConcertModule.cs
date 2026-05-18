using Concertable.Shared;

namespace Concertable.Concert.Contracts;

public interface IConcertModule
{
    Task<IPagination<ReviewDto>> GetReviewsByArtistAsync(int artistId, IPageParams pageParams);
    Task<IPagination<ReviewDto>> GetReviewsByVenueAsync(int venueId, IPageParams pageParams);
    Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);

    Task<int> GetVenueApplicationsAwaitingReviewCountAsync(int venueId, CancellationToken ct = default);
    Task<int> GetVenueOpenOpportunitiesCountAsync(int venueId, CancellationToken ct = default);
    Task<int> GetVenueUpcomingConcertsCountAsync(int venueId, CancellationToken ct = default);

    Task<int> GetArtistPendingApplicationsCountAsync(int artistId, CancellationToken ct = default);
    Task<int> GetArtistUpcomingConcertsCountAsync(int artistId, CancellationToken ct = default);
}
