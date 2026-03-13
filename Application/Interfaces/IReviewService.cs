using Application.DTOs;
using Core.Parameters;
using Application.Responses;
using Application.Requests;

namespace Application.Interfaces;

public interface IReviewService
{
    Task SetAverageRatingAsync(VenueDto venue);

    Task<Pagination<ReviewDto>> GetByVenueIdAsync(int id, IPageParams pageParams);
    Task<Pagination<ReviewDto>> GetByArtistIdAsync(int id, IPageParams pageParams);
    Task<Pagination<ReviewDto>> GetByConcertIdAsync(int id, IPageParams pageParams);

    Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id);

    Task<ReviewDto> CreateAsync(CreateReviewRequest review);
    Task<bool> CanUserReviewConcertIdAsync(int concertId);
    Task<bool> CanUserReviewVenueIdAsync(int venueId);
    Task<bool> CanUserReviewArtistIdAsync(int artistId);
}
