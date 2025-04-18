using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities.Identity;
using Application.DTOs;

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

        public async Task<VenueManager> GetOwnerByIdAsync(int listingId)
        {
            var query = context.Users
                .Where(u => EF.Property<string>(u, "Discriminator") == "VenueManager")
                .OfType<VenueManager>()
                .Where(vm => vm.Venue != null && vm.Venue.Listings.Any(l => l.Id == listingId));

            return await query.FirstAsync();
        }

        public async new Task<Listing?> GetByIdAsync(int id)
        {
            var query = context.Listings
                .Where(l => l.Id == id)
                .Include(l => l.ListingGenres)
                    .ThenInclude(lg => lg.Genre);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Listing?> GetWithVenueByIdAsync(int id)
        {
            var query = context.Listings
                .Where(l => l.Id == id)
                .Include(l => l.Venue);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Listing?> GetByApplicationIdAsync(int id)
        {
            var query = context.Listings
                .Include(l => l.ListingGenres)
                    .ThenInclude(lg => lg.Genre)
                .Include(l => l.Venue)
                    .ThenInclude(v => v.User)
                .Where(l => l.Applications.Any(a => a.Id == id));

            return await query.FirstOrDefaultAsync();
        }
    }
}
