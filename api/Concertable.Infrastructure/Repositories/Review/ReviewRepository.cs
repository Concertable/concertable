using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Responses;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Review;

public class ReviewRepository<TEntity> : IReviewRepository<TEntity>
    where TEntity : class, IIdEntity, IReviewable<TEntity>
{
    private readonly ApplicationDbContext context;
    private readonly IReviewSpecification<TEntity> spec;

    public ReviewRepository(ApplicationDbContext context, IReviewSpecification<TEntity> spec)
    {
        this.context = context;
        this.spec = spec;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
        spec.Apply(context.Reviews, id)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public async Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
        await spec.Apply(context.Reviews, id)
            .ToSummaryDto()
            .FirstOrDefaultAsync()
            ?? new ReviewSummaryDto(0, null);
}
