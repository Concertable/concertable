using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace Infrastructure.Repositories
{
    public class ListingApplicationRepository : Repository<ListingApplication>, IListingApplicationRepository
    {
        public ListingApplicationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ListingApplication>> GetForListingIdAsync(int id)
        {
            var query = context.ListingApplications
            .Where(la => la.ListingId == id)
            .Include(la => la.Artist)
                .ThenInclude(a => a.User)
            .Include(la => la.Artist)
                .ThenInclude(a => a.ArtistGenres)
                    .ThenInclude(ag => ag.Genre); 

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ListingApplication>> GetActiveByArtistIdAsync(int artistId)
        {
            var query = context.ListingApplications
            .Include(a => a.Listing)
            .ThenInclude(l => l.Venue)
            .Where(a =>
                a.ArtistId == artistId &&
                (
                    // Check an Event exists where the ApplicationId matches this application's ID
                    !context.Events.Any(e => e.ApplicationId == a.Id) ||
                    a.Listing.EndDate > DateTime.UtcNow
                ));

            return await query.ToListAsync();
        }

        public async Task<(Artist, Venue)> GetArtistAndVenueByIdAsync(int id)
        {
            var query = await context.ListingApplications
                .Where(la => la.Id == id)
                .Include(la => la.Artist)              // Ensure the Artist is loaded
                    .ThenInclude(a => a.User)          // Ensure the Artist's User is loaded
                .Include(la => la.Artist)              // Ensure the Artist is loaded
                    .ThenInclude(a => a.ArtistGenres)
                .Include(la => la.Listing)             // Ensure the Listing is loaded (to access Venue)
                    .ThenInclude(l => l.Venue)         // Ensure the Venue is loaded
                        .ThenInclude(v => v.User)      // Ensure the Venue's User is loaded
                .Include(la => la.Listing.ListingGenres)
                .FirstAsync();

            return (query.Artist, query.Listing.Venue);
        }



        public async Task<decimal> GetListingPayByIdAsync(int id)
        {
            var query = context.ListingApplications
                .Where(la => la.Id == id)
                .Select(la => la.Listing.Pay);

            return await query.FirstAsync();
        }

        public async new Task<ListingApplication?> GetByIdAsync(int id)
        {
            var query = context.ListingApplications
                .Where(la => la.Id == id)
                .Include(la => la.Artist)
                    .ThenInclude(la => la.ArtistGenres)
                        .ThenInclude(la => la.Genre)
                .Include(la => la.Artist.User)
                .Include(la => la.Listing);

            return await query.FirstOrDefaultAsync();
        }
    }
}
