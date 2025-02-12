using Core.Entities;
using Application.Interfaces;
using Infrastructure.Repositories;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using Core.Responses;
using Core.Entities.Identity;

namespace Infrastructure.Repositories
{
    public class VenueRepository : Repository<Venue>, IVenueRepository
    {
        public VenueRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PaginationResponse<Venue>> GetHeadersAsync(SearchParams? searchParams)
        {
            var query = context.Venues.AsQueryable();
            query = query.Select(v => new Venue
            {
                Id = v.Id,
                Name = v.Name,
                ImageUrl = v.ImageUrl,
            });

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
            {
                query = query.OrderBy(v => searchParams.Sort);
            }
            return await PaginationHelper.CreatePaginatedResponseAsync(query, searchParams.PageNumber, searchParams.PageSize);
        }

        public async Task<Venue> GetByIdAsync(int id)
        {
            var query = context.Venues
                .Where(v => v.Id == id)
                .Include(v => v.User);

            return await query.FirstAsync();
        } 

        public async Task<Venue?> GetByUserIdAsync(int id)
        {
            var query = context.Venues
                .Where(v => v.UserId == id)
                .Include(v => v.User);

            return await query.FirstOrDefaultAsync();
        }
    }
}
