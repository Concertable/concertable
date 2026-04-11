using Concertable.Application.DTOs;
using Concertable.Core.Parameters;
using Concertable.Application.Results;
using Concertable.Application.Requests;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces;

public interface IReviewService
{
    Task<IPagination<ReviewDto>> GetByVenueIdAsync(int id, IPageParams pageParams);
    Task<IPagination<ReviewDto>> GetByArtistIdAsync(int id, IPageParams pageParams);
    Task<IPagination<ReviewDto>> GetByConcertIdAsync(int id, IPageParams pageParams);

    Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id);

    Task<ReviewDto> CreateAsync(CreateReviewRequest review);
}
