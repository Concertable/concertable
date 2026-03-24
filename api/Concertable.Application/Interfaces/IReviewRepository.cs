using Core.Interfaces;
using Application.DTOs;
using Core.Entities;
using Core.Parameters;
using Application.Responses;

namespace Application.Interfaces;

public interface IReviewRepository : IRepository<ReviewEntity>
{
    Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);

    Task<Pagination<ReviewEntity>> GetByConcertIdAsync(int concertId, IPageParams pageParams);
    Task<Pagination<ReviewEntity>> GetByArtistIdAsync(int artistId, IPageParams pageParams);
    Task<Pagination<ReviewEntity>> GetByVenueIdAsync(int venueId, IPageParams pageParams);

    Task<bool> CanUserIdReviewConcertIdAsync(Guid userId, int concertId);
    Task<bool> CanUserIdReviewArtistIdAsync(Guid userId, int artistId);
    Task<bool> CanUserIdReviewVenueIdAsync(Guid userId, int venueId);
}
