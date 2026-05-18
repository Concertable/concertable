using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Contracts;
using Concertable.Shared;

namespace Concertable.Concert.Infrastructure;

internal sealed class ConcertModule(
    IArtistReviewRepository artistReviewRepository,
    IVenueReviewRepository venueReviewRepository,
    IReviewValidator reviewValidator,
    IApplicationRepository applicationRepository,
    IOpportunityRepository opportunityRepository,
    IConcertRepository concertRepository) : IConcertModule
{
    public Task<IPagination<ReviewDto>> GetReviewsByArtistAsync(int artistId, IPageParams pageParams) =>
        artistReviewRepository.GetByArtistAsync(artistId, pageParams);

    public Task<IPagination<ReviewDto>> GetReviewsByVenueAsync(int venueId, IPageParams pageParams) =>
        venueReviewRepository.GetByVenueAsync(venueId, pageParams);

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        reviewValidator.CanUserReviewArtistAsync(userId, artistId);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        reviewValidator.CanUserReviewVenueAsync(userId, venueId);

    public Task<int> GetVenueApplicationsAwaitingReviewCountAsync(int venueId, CancellationToken ct = default) =>
        applicationRepository.CountVenueAwaitingReviewAsync(venueId, ct);

    public Task<int> GetVenueOpenOpportunitiesCountAsync(int venueId, CancellationToken ct = default) =>
        opportunityRepository.CountVenueOpenAsync(venueId, ct);

    public Task<int> GetVenueUpcomingConcertsCountAsync(int venueId, CancellationToken ct = default) =>
        concertRepository.CountVenueUpcomingAsync(venueId, ct);
}
