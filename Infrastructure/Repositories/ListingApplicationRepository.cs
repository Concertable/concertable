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
    public class ListingApplicationRepository : Repository<ListingApplication>, IListingApplicationRepository
    {
        public ListingApplicationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ListingApplication>> GetAllForListingIdAsync(int id)
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

        public async Task<(Artist, Venue)> GetArtistAndVenueByIdAsync(int id)
        {
            var query = await context.ListingApplications
                .Where(la => la.Id == id)
                .Select(la => new
                {
                    la.Artist,
                    la.Listing.Venue
                })
                .FirstAsync();

            return (query.Artist, query.Venue);
        }

    }
}
