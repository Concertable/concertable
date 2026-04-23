using Concertable.Concert.Application.Requests;
using Concertable.Shared;

namespace Concertable.Concert.Application.Interfaces.Reviews;

internal interface IConcertReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int concertId, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryAsync(int concertId);
    Task<bool> CanCurrentUserReviewAsync(int concertId);
    Task<ReviewDto> CreateAsync(CreateReviewRequest request);
}
