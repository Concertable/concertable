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
using Infrastructure.Helpers;
using Core.Responses;
using Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Event> GetByIdAsync(int id)
        {
            var query = context.Events
            .Where(e => e.Id == id)
            .Include(e => e.Application)
                .ThenInclude(la => la.Artist)
                    .ThenInclude(a => a.User)
            .Include(e => e.Application)
                .ThenInclude(la => la.Artist)
                    .ThenInclude(a => a.ArtistGenres)
                        .ThenInclude(ag => ag.Genre)
            .Include(e => e.Application.Listing)
                .ThenInclude(l => l.Venue)
                    .ThenInclude(v => v.User); 


            return await query.FirstAsync();
        }


        public async Task<PaginationResponse<Event>> GetHeadersAsync(SearchParams searchParams)
        {
            var query = context.Events.AsQueryable();
            query = query.Select(v => new Event
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

        public async Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id)
        {
            var query = context.Events //Get all events
                .Where(e => e.Application.Listing.VenueId == id // That are associated with the venue
                            && e.Application.Listing.StartDate >= DateTime.Now) // And event hasnt passed yet
                .Include(e => e.Application)
                .ThenInclude(a => a.Listing); // To retrieve the start date

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingByArtistIdAsync(int id)
        {
            var query = context.Events //Get all events
                .Where(e => e.Application.ArtistId == id // That are associated with the artist
                            && e.Application.Listing.StartDate >= DateTime.Now) // And event hasnt passed yet
                .Include(e => e.Application)
                .ThenInclude(a => a.Listing); // To retrieve the start date

            return await query.ToListAsync();
        }
    }
}
