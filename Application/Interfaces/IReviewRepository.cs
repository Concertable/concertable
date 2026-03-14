using Core.Interfaces;
using Application.DTOs;
using Core.Entities;
using Core.Parameters;
using Application.Responses;

namespace Application.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);

    Task<Pagination<Review>> GetByConcertIdAsync(int concertId, IPageParams pageParams);
    Task<Pagination<Review>> GetByArtistIdAsync(int artistId, IPageParams pageParams);
    Task<Pagination<Review>> GetByVenueIdAsync(int venueId, IPageParams pageParams);

    Task<bool> CanUserIdReviewConcertIdAsync(int userId, int concertId);
    Task<bool> CanUserIdReviewArtistIdAsync(int userId, int artistId);
    Task<bool> CanUserIdReviewVenueIdAsync(int userId, int venueId);
}
