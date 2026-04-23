using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Helpers;
using Concertable.Search.Application.Interfaces;
using Concertable.Search.Infrastructure.Data;
using Concertable.Search.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class VenueHeaderRepository : IVenueHeaderRepository
{
    private readonly SearchDbContext context;
    private readonly IVenueSearchSpecification searchSpecification;
    private readonly IGeometrySpecification<VenueSearchModel> geometrySpecification;
    private readonly ISortSpecification<VenueHeaderDto> sortSpecification;
    private readonly IRatingSpecification<VenueSearchModel> ratingSpecification;

    public VenueHeaderRepository(
        SearchDbContext context,
        IVenueSearchSpecification searchSpecification,
        IGeometrySpecification<VenueSearchModel> geometrySpecification,
        ISortSpecification<VenueHeaderDto> sortSpecification,
        IRatingSpecification<VenueSearchModel> ratingSpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.geometrySpecification = geometrySpecification;
        this.sortSpecification = sortSpecification;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<IPagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Venues.AsNoTracking(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        var dtos = sortSpecification.Apply(query.ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews.AsNoTracking())), searchParams);
        return await dtos.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Venues.AsNoTracking()
            .OrderBy(v => v.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews.AsNoTracking()))
            .Take(amount)
            .ToListAsync();
}
