using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces;

public interface IArtistReviewRepository
{
    Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryAsync(int id);
    Task<bool> CanReviewAsync(Guid userId, int id);
}
