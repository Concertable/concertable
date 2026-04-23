using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Helpers;
using Concertable.Search.Application.Interfaces;
using Concertable.Search.Infrastructure.Data;
using Concertable.Search.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class ArtistHeaderRepository : IArtistHeaderRepository
{
    private readonly SearchDbContext context;
    private readonly IArtistSearchSpecification searchSpecification;
    private readonly IGeometrySpecification<ArtistSearchModel> geometrySpecification;
    private readonly ISortSpecification<ArtistHeaderDto> sortSpecification;
    private readonly IRatingSpecification<ArtistSearchModel> ratingSpecification;

    public ArtistHeaderRepository(
        SearchDbContext context,
        IArtistSearchSpecification searchSpecification,
        IGeometrySpecification<ArtistSearchModel> geometrySpecification,
        ISortSpecification<ArtistHeaderDto> sortSpecification,
        IRatingSpecification<ArtistSearchModel> ratingSpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.geometrySpecification = geometrySpecification;
        this.sortSpecification = sortSpecification;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<IPagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Artists.AsNoTracking(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        var dtos = sortSpecification.Apply(query.ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews.AsNoTracking())), searchParams);
        return await dtos.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Artists.AsNoTracking()
            .OrderBy(a => a.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews.AsNoTracking()))
            .Take(amount)
            .ToListAsync();
}
