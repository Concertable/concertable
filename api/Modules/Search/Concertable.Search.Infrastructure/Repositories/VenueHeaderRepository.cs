using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Data.Application;
using Concertable.Infrastructure.Helpers;
using Concertable.Search.Application.Interfaces;
using Concertable.Search.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class VenueHeaderRepository : IVenueHeaderRepository
{
    private readonly IReadDbContext context;
    private readonly IVenueSearchSpecification searchSpecification;
    private readonly IGeometrySpecification<VenueEntity> geometrySpecification;
    private readonly ISortSpecification<VenueHeaderDto> sortSpecification;
    private readonly IRatingSpecification<VenueEntity> ratingSpecification;

    public VenueHeaderRepository(
        IReadDbContext context,
        IVenueSearchSpecification searchSpecification,
        IGeometrySpecification<VenueEntity> geometrySpecification,
        ISortSpecification<VenueHeaderDto> sortSpecification,
        IRatingSpecification<VenueEntity> ratingSpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.geometrySpecification = geometrySpecification;
        this.sortSpecification = sortSpecification;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<IPagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Venues, searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        var dtos = sortSpecification.Apply(query.ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews)), searchParams);
        return await dtos.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Venues
            .OrderBy(v => v.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();
}
