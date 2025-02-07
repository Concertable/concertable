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

namespace Infrastructure.Repositories
{
    public class VenueRepository : BaseEntityRepository<Venue>, IVenueRepository
    {
        public VenueRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Venue>> GetHeadersAsync(SearchParams? searchParams)
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
            return await query.ToListAsync();
        }

        public async Task<Venue?> GetByUserIdAsync(int id)
        {
            var query = context.Venues.AsQueryable();
            query = query.Where(v => v.UserId == id);
                

            return await query.FirstOrDefaultAsync();
        }
    }
}
