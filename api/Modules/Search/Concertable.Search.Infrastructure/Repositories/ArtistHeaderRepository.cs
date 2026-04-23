using Concertable.Application.Interfaces;
using Concertable.Data.Application;
using Concertable.Infrastructure.Helpers;
using Concertable.Search.Application.Interfaces;
using Concertable.Search.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class ArtistHeaderRepository : IArtistHeaderRepository
{
    private readonly IReadDbContext context;
    private readonly IArtistSearchSpecification searchSpecification;
    private readonly IGeometrySpecification<ArtistEntity> geometrySpecification;
    private readonly ISortSpecification<ArtistHeaderDto> sortSpecification;
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;

    public ArtistHeaderRepository(
        IReadDbContext context,
        IArtistSearchSpecification searchSpecification,
        IGeometrySpecification<ArtistEntity> geometrySpecification,
        ISortSpecification<ArtistHeaderDto> sortSpecification,
        IRatingSpecification<ArtistEntity> ratingSpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.geometrySpecification = geometrySpecification;
        this.sortSpecification = sortSpecification;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<IPagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Artists, searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        var dtos = sortSpecification.Apply(query.ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews)), searchParams);
        return await dtos.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Artists
            .OrderBy(a => a.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();
}
