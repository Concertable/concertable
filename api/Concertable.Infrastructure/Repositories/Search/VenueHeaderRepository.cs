using Application.DTOs;
using Application.Interfaces.Search;
using Application.Responses;
using Concertable.Infrastructure.Data;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Helpers;
using Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search;

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
