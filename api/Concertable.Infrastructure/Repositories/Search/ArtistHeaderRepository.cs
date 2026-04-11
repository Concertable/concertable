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

public class ArtistHeaderRepository : IArtistHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IArtistSearchSpecification searchSpecification;
    private readonly IGeometrySpecification<ArtistEntity> geometrySpecification;
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;
    private readonly ISortSpecification<ArtistHeaderDto> sortSpecification;

    public ArtistHeaderRepository(
        ApplicationDbContext context,
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
