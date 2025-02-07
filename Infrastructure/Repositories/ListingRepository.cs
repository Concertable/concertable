using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ListingRepository : BaseEntityRepository<Listing>, IListingRepository
    {
        public ListingRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Listing>> GetActiveByVenueIdAsync(int id)
        {
            var query = context.Listings //c
                .Where(l => l.VenueId == id && l.StartDate >= DateTime.Today)
                .Where(l => !context.Registers //Listings that DONT have an event
                    .Where(r => r.ListingId == l.Id) // Find registers linked to the listing
                    .Join(context.Events, r => r.Id, e => e.RegisterId, (r, e) => e) // Find events linked to those registers
                    .Any()) // Ensure no event exists
                .Include(l => l.ListingGenres) // Include the ListingGenres relationship
                .ThenInclude(lg => lg.Genre); // Include the related Genre entity


            return await query.ToListAsync();
        }
    }
}
