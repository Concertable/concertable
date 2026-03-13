using Application.DTOs;
using Application.Interfaces.Search;
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
    private readonly IVenueHeaderSpecification specification;
    private readonly IRatingSpecification<Venue> ratingSpecification;

    public VenueHeaderRepository(
        ApplicationDbContext context,
        IVenueHeaderSpecification specification,
        IRatingSpecification<Venue> ratingSpecification)
    {
        this.context = context;
        this.specification = specification;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<Pagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = specification.Apply(context.Venues.AsQueryable(), searchParams);
        return await query.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount)
    {
        var ratings = ratingSpecification.Apply(context.Reviews);

        return await (from v in context.Venues
                      join r in ratings on v.Id equals r.EntityId into rg
                      from rating in rg.DefaultIfEmpty()
                      orderby v.Id
                      select new VenueHeaderDto
                      {
                          Id = v.Id,
                          Name = v.Name,
                          ImageUrl = v.ImageUrl,
                          Rating = rating != null ? rating.AverageRating : 0.0,
                          County = v.User.County ?? string.Empty,
                          Town = v.User.Town ?? string.Empty,
                          Latitude = v.User.Location != null ? v.User.Location.Y : 0.0,
                          Longitude = v.User.Location != null ? v.User.Location.X : 0.0
                      })
                     .Take(amount)
                     .ToListAsync();
    }
}
