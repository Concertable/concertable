using Application.DTOs;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search
{
    public class ArtistHeaderRepository : IArtistHeaderRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IArtistSearchSpecification specification;

        public ArtistHeaderRepository(ApplicationDbContext context, IArtistSearchSpecification specification)
        {
            this.context = context;
            this.specification = specification;
        }

        public async Task<Pagination<Artist>> SearchAsync(SearchParams searchParams)
        {
            var query = specification.Apply(context.Artists.AsQueryable(), searchParams);
            return await query.ToPaginationAsync(searchParams);
        }

        public async Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount)
        {
            return await context.Artists
                .Include(a => a.User)
                .OrderBy(a => a.Id)
                .Take(amount)
                .Select(a => new ArtistHeaderDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl,
                    County = a.User.County,
                    Town = a.User.Town,
                    Latitude = a.User.Location != null ? a.User.Location.Y : (double?)null,
                    Longitude = a.User.Location != null ? a.User.Location.X : (double?)null
                })
                .ToListAsync();
        }
    }
}
