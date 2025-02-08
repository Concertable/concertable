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
    public class ListingRepository : Repository<Listing>, IListingRepository
    {
        public ListingRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Listing>> GetActiveByVenueIdAsync(int id)
        {
            var query = context.Listings //c
                .Where(l => l.VenueId == id && l.StartDate >= DateTime.Now)
                .Where(l => !context.Events.Any(e => e.ApplicationId == //doesnt have any events associated with it
                    context.ListingApplications //by checking the applications
                        .Where(la => la.ListingId == l.Id) //that have the same listing
                        .Select(la => la.Id) 
                        .FirstOrDefault())
                )
                .Include(l => l.ListingGenres) // Include the ListingGenres relationship
                .ThenInclude(lg => lg.Genre); // Include the related Genre entity

            return await query.ToListAsync();
        }
    }
}
