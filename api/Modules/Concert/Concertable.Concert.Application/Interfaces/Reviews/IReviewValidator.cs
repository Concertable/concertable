namespace Concertable.Concert.Application.Interfaces.Reviews;

internal interface IReviewValidator
{
    Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId);
    Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);
}
