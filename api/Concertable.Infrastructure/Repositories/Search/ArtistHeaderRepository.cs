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

public class ArtistHeaderRepository : IArtistHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IArtistSearchSpecification searchSpecification;
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;
    private readonly IGeometrySpecification<ArtistEntity> geometrySpecification;

    public ArtistHeaderRepository(
        ApplicationDbContext context,
        IArtistSearchSpecification searchSpecification,
        IRatingSpecification<ArtistEntity> ratingSpecification,
        IGeometrySpecification<ArtistEntity> geometrySpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.ratingSpecification = ratingSpecification;
        this.geometrySpecification = geometrySpecification;
    }

    public async Task<Pagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Artists.AsNoTracking().AsQueryable(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        return await query
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount)
    {
        return await context.Artists
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();
    }
}
