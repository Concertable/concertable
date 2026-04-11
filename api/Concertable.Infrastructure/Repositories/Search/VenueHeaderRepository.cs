using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Results;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Search;

public class VenueHeaderRepository : IVenueHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IVenueSearchSpecification searchSpecification;
    private readonly IGeometrySpecification<VenueEntity> geometrySpecification;
    private readonly IRatingSpecification<VenueEntity> ratingSpecification;
    private readonly ISortSpecification<VenueHeaderDto> sortSpecification;

    public VenueHeaderRepository(
        ApplicationDbContext context,
        IVenueSearchSpecification searchSpecification,
        IGeometrySpecification<VenueEntity> geometrySpecification,
        IRatingSpecification<VenueEntity> ratingSpecification,
        ISortSpecification<VenueHeaderDto> sortSpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.geometrySpecification = geometrySpecification;
        this.ratingSpecification = ratingSpecification;
        this.sortSpecification = sortSpecification;
    }

    public async Task<IPagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Venues.AsNoTracking(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        var dtos = sortSpecification.Apply(query.ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews)), searchParams);
        return await dtos.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Venues.AsNoTracking()
            .OrderBy(v => v.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();
}
