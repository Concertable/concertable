using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Search;

public class VenueHeaderRepository : IVenueHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IVenueSearchSpecification searchSpecification;
    private readonly IRatingSpecification<VenueEntity> ratingSpecification;
    private readonly IGeometrySpecification<VenueEntity> geometrySpecification;

    public VenueHeaderRepository(
        ApplicationDbContext context,
        IVenueSearchSpecification searchSpecification,
        IRatingSpecification<VenueEntity> ratingSpecification,
        IGeometrySpecification<VenueEntity> geometrySpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.ratingSpecification = ratingSpecification;
        this.geometrySpecification = geometrySpecification;
    }

    public async Task<Pagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Venues.AsNoTracking().AsQueryable(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        return await query
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount)
    {
        return await context.Venues
            .AsNoTracking()
            .OrderBy(v => v.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();
    }
}
