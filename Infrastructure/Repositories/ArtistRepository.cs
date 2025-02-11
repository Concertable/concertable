using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Responses;
using Infrastructure.Helpers;

namespace Infrastructure.Repositories
{
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        public ArtistRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PaginationResponse<Artist>> GetHeadersAsync(SearchParams searchParams)
        {
            var query = context.Artists.AsQueryable();
            query = query.Select(v => new Artist
            {
                Id = v.Id,
                Name = v.Name
            });

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
            {
                query = query.OrderBy(v => searchParams.Sort);
            }
            return await PaginationHelper.CreatePaginatedResponseAsync(query, searchParams.PageNumber, searchParams.PageSize);
        }

        public async Task<Artist?> GetByUserIdAsync(int id)
        {
            var query = context.Artists.AsQueryable();
            query = query.Where(v => v.UserId == id);

            return await query.FirstOrDefaultAsync();
        }
    }
}
