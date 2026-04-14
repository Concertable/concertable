using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces;

public interface IReviewRepository<TEntity>
    where TEntity : class, IIdEntity, IReviewable<TEntity>
{
    Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryAsync(int id);
}
