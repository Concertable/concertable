using Concertable.Core.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Application.Results;

namespace Concertable.Application.Interfaces;

public interface IReviewRepository : IRepository<ReviewEntity>
{
    Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);

    Task<IPagination<ReviewDto>> GetByConcertIdAsync(int concertId, IPageParams pageParams);
    Task<IPagination<ReviewDto>> GetByArtistIdAsync(int artistId, IPageParams pageParams);
    Task<IPagination<ReviewDto>> GetByVenueIdAsync(int venueId, IPageParams pageParams);

    Task<bool> CanUserIdReviewConcertIdAsync(Guid userId, int concertId);
    Task<bool> CanUserIdReviewArtistIdAsync(Guid userId, int artistId);
    Task<bool> CanUserIdReviewVenueIdAsync(Guid userId, int venueId);
}
