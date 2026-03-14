using Application.DTOs;
using Application.Interfaces.Search;
using Application.Mappers;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search;

public class VenueHeaderRepository : IVenueHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IVenueSearchSpecification specification;
    private readonly IGeometrySpecification<Venue> geometrySpecification;

    public VenueHeaderRepository(ApplicationDbContext context, IVenueSearchSpecification specification, IGeometrySpecification<Venue> geometrySpecification)
    {
        this.context = context;
        this.specification = specification;
        this.geometrySpecification = geometrySpecification;
    }

    public async Task<Pagination<Venue>> SearchAsync(SearchParams searchParams)
    {
        var query = specification.Apply(context.Venues.AsQueryable(), searchParams);
        query = geometrySpecification.Apply(query, searchParams);
        return await query.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount)
    {
        var venues = await context.Venues
            .Include(v => v.User)
            .OrderBy(v => v.Id)
            .Take(amount)
            .ToListAsync();

        return venues.ToHeaderDtos();
    }
}
