using Application.DTOs;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search;

public class ArtistHeaderRepository : IArtistHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IArtistSearchSpecification searchSpecification;
    private readonly IRatingSpecification<Artist> ratingSpecification;
    private readonly IGeometrySpecification<Artist> geometrySpecification;

    public ArtistHeaderRepository(
        ApplicationDbContext context,
        IArtistSearchSpecification searchSpecification,
        IRatingSpecification<Artist> ratingSpecification,
        IGeometrySpecification<Artist> geometrySpecification)
    {
        this.context = context;
        this.searchSpecification = searchSpecification;
        this.ratingSpecification = ratingSpecification;
        this.geometrySpecification = geometrySpecification;
    }

    public async Task<Pagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = searchSpecification.Apply(context.Artists.AsQueryable(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        return await query
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount)
    {
        return await context.Artists
            .OrderBy(a => a.Id)
            .ToHeaderDtos(ratingSpecification.ApplyAggregate(context.Reviews))
            .Take(amount)
            .ToListAsync();
    }
}
