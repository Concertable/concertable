using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Extensions;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Search;

public class ConcertHeaderRepository : IConcertHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IConcertSearchSpecification searchSpecification;
    private readonly IRatingSpecification<ConcertEntity> ratingSpecification;
    private readonly IGeometrySpecification<ConcertEntity> geometrySpecification;
    private readonly ISortSpecification<ConcertHeaderDto> sortSpecification;
    private readonly TimeProvider timeProvider;

    public ConcertHeaderRepository(
        ApplicationDbContext context,
        IConcertSearchSpecification searchSpecification,
        IRatingSpecification<ConcertEntity> ratingSpecification,
        IGeometrySpecification<ConcertEntity> geometrySpecification,
        ISortSpecification<ConcertHeaderDto> sortSpecification,
        TimeProvider timeProvider)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.ratingSpecification = ratingSpecification;
        this.geometrySpecification = geometrySpecification;
        this.sortSpecification = sortSpecification;
        this.timeProvider = timeProvider;
    }

    public async Task<IPagination<ConcertHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Concerts.AsNoTracking(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        var dtos = sortSpecification.Apply(query.ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews)), searchParams);
        return await dtos.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Concerts.AsNoTracking()
            .Active(timeProvider.GetUtcNow().DateTime)
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
            : query.OrderBy(c => c.Booking.Application.Opportunity.Period.Start);

        return await query
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(10)
            .ToListAsync();
    }

}
