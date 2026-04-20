using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using Concertable.Data.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Search.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Search.Infrastructure.Repositories;

internal class ArtistHeaderRepository : IArtistHeaderRepository
{
    private readonly ReadDbContext context;
    private readonly IArtistSearchSpecification searchSpecification;
    private readonly IGeometrySpecification<ArtistEntity> geometrySpecification;
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;
    private readonly ISortSpecification<ArtistHeaderDto> sortSpecification;

    public ArtistHeaderRepository(
        ReadDbContext context,
        IArtistSearchSpecification searchSpecification,
        IGeometrySpecification<ArtistEntity> geometrySpecification,
        IRatingSpecification<ArtistEntity> ratingSpecification,
        ISortSpecification<ArtistHeaderDto> sortSpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.geometrySpecification = geometrySpecification;
        this.ratingSpecification = ratingSpecification;
        this.sortSpecification = sortSpecification;
    }

    public async Task<IPagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Artists.AsNoTracking(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        var dtos = sortSpecification.Apply(query.ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews)), searchParams);
        return await dtos.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount) =>
        await context.Artists.AsNoTracking()
            .OrderBy(a => a.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();
}
