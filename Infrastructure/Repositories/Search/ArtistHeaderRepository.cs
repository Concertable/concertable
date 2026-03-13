using Application.DTOs;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search;

public class ArtistHeaderRepository : IArtistHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IArtistHeaderSpecification specification;
    private readonly IRatingSpecification<Artist> ratingSpecification;

    public ArtistHeaderRepository(
        ApplicationDbContext context,
        IArtistHeaderSpecification specification,
        IRatingSpecification<Artist> ratingSpecification)
    {
        this.context = context;
        this.specification = specification;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<Pagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = specification.Apply(context.Artists.AsQueryable(), searchParams);
        return await query.ToPaginationAsync(searchParams);
    }

    public async Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount)
    {
        var ratings = ratingSpecification.Apply(context.Reviews);

        return await (from a in context.Artists
                      join r in ratings on a.Id equals r.EntityId into rg
                      from rating in rg.DefaultIfEmpty()
                      orderby a.Id
                      select new ArtistHeaderDto
                      {
                          Id = a.Id,
                          Name = a.Name,
                          ImageUrl = a.ImageUrl,
                          Rating = rating != null ? rating.AverageRating : 0.0,
                          County = a.User.County ?? string.Empty,
                          Town = a.User.Town ?? string.Empty,
                          Latitude = a.User.Location != null ? a.User.Location.Y : 0.0,
                          Longitude = a.User.Location != null ? a.User.Location.X : 0.0
                      })
                     .Take(amount)
                     .ToListAsync();
    }
}
