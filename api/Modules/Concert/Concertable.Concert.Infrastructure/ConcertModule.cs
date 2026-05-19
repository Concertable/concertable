using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Contracts;
using Concertable.Customer.Review.Contracts;
using Concertable.Shared;

namespace Concertable.Concert.Infrastructure;

// TEMPORARY Phase 1: IConcertModule still hosts the review facade methods (legacy callers in Artist.Infrastructure /
// Venue.Infrastructure). They delegate to ICustomerReviewModule (Customer.Review.Contracts). Callers migrate to
// ICustomerReviewModule directly in a later phase; these forward methods get deleted then.
internal sealed class ConcertModule(
    ICustomerReviewModule customerReviewModule,
    IConcertDashboardRepository dashboardRepository) : IConcertModule
{
    public Task<IPagination<ReviewDto>> GetReviewsByArtistAsync(int artistId, IPageParams pageParams) =>
        customerReviewModule.GetReviewsByArtistAsync(artistId, pageParams);

    public Task<IPagination<ReviewDto>> GetReviewsByVenueAsync(int venueId, IPageParams pageParams) =>
        customerReviewModule.GetReviewsByVenueAsync(venueId, pageParams);

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        customerReviewModule.CanUserReviewArtistAsync(userId, artistId);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        customerReviewModule.CanUserReviewVenueAsync(userId, venueId);

    public Task<VenueDashboardCountsDto?> GetVenueDashboardCountsAsync(int venueId, CancellationToken ct = default) =>
        dashboardRepository.GetVenueCountsAsync(venueId, ct);

    public Task<ArtistDashboardCountsDto?> GetArtistDashboardCountsAsync(int artistId, CancellationToken ct = default) =>
        dashboardRepository.GetArtistCountsAsync(artistId, ct);
}
