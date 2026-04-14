using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces;

public interface IConcertReviewRepository
{
    Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryAsync(int id);
    Task<bool> CanReviewAsync(Guid userId, int id);
    Task<ReviewEntity> AddAsync(ReviewEntity review);
    Task SaveChangesAsync();
}
