namespace Application.Interfaces;

public interface IReviewValidator
{
    Task<bool> CanUserReviewConcertAsync(int concertId);
    Task<bool> CanUserReviewVenueAsync(int venueId);
    Task<bool> CanUserReviewArtistAsync(int artistId);
}
