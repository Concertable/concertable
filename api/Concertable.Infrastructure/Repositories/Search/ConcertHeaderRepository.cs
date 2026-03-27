using Application.DTOs;
using Application.Interfaces.Search;
using Application.Responses;
using Concertable.Infrastructure.Data;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search;

public class ConcertHeaderRepository : IConcertHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IConcertSearchSpecification searchSpecification;
    private readonly IRatingSpecification<ConcertEntity> ratingSpecification;
    private readonly IGeometrySpecification<ConcertEntity> geometrySpecification;
    private readonly TimeProvider timeProvider;

    public ConcertHeaderRepository(
        ApplicationDbContext context,
        IConcertSearchSpecification searchSpecification,
        IRatingSpecification<ConcertEntity> ratingSpecification,
        IGeometrySpecification<ConcertEntity> geometrySpecification,
        TimeProvider timeProvider)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.ratingSpecification = ratingSpecification;
        this.geometrySpecification = geometrySpecification;
        this.timeProvider = timeProvider;
    }

    public async Task<Pagination<ConcertHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Concerts.AsNoTracking().AsQueryable(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        return await query
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Concerts.AsNoTracking().Active(timeProvider.GetUtcNow().DateTime)
            .OrderByDescending(c => c.DatePosted)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();

    public async Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync() =>
        await context.Concerts.AsNoTracking().Active(timeProvider.GetUtcNow().DateTime)
            .OrderByDescending(c => c.TotalTickets - c.AvailableTickets)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(10)
            .ToListAsync();

    public async Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync() =>
        await context.Concerts.AsNoTracking().Active(timeProvider.GetUtcNow().DateTime)
            .Where(c => c.Price == 0)
            .OrderByDescending(c => c.DatePosted)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(10)
            .ToListAsync();

    public async Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams)
    {
        var query = context.Concerts.AsNoTracking().Active(timeProvider.GetUtcNow().DateTime);

        if (concertParams.GenreIds.Any())
            query = query.Where(c => c.ConcertGenres.Any(eg => concertParams.GenreIds.Contains(eg.GenreId)));

        query = geometrySpecification.Apply(query, concertParams);

        query = concertParams.OrderByRecent
            ? query.OrderByDescending(c => c.DatePosted)
            : query.OrderBy(c => c.Application.Opportunity.StartDate);

        return await query
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(10)
            .ToListAsync();
    }
}
