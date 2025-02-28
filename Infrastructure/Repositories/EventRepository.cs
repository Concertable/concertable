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
using Application.Responses;
using Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class EventRepository : HeaderRepository<Event, EventHeaderDto>, IEventRepository
    {
        public EventRepository(
            ApplicationDbContext context
            ) : base(context)
        {
        }

        protected override Expression<Func<Event, EventHeaderDto>> Selector => e => new EventHeaderDto
        {
            Id = e.Id,
            Name = e.Name,
            ImageUrl = e.Application.Artist.ImageUrl,
            StartDate = e.Application.Listing.StartDate,
            EndDate = e.Application.Listing.EndDate,
            County = e.Application.Listing.Venue.User.County,
            Town = e.Application.Listing.Venue.User.Town,
            Latitude = e.Application.Listing.Venue.User.Latitude,
            Longitude = e.Application.Listing.Venue.User.Longitude
        };

        protected override List<Expression<Func<Event, bool>>> Filters(SearchParams searchParams) {
            var filters = new List<Expression<Func<Event, bool>>>();

            if (searchParams.Date != null)
            {
                filters.Add(e => e.Application.Listing.StartDate >= searchParams.Date);
            }

            if (searchParams.GenreIds != null && searchParams.GenreIds.Any())
            {
                filters.Add(e => e.Application.Artist.ArtistGenres.Any(ag => searchParams.GenreIds.Contains(ag.GenreId)));
            }

            return filters;
        }

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

        public async Task<Event?> GetByApplicationIdAsync(int applicationId)
        {
            var query = context.Events
                .Where(e => e.ApplicationId == applicationId);

            return await query.FirstOrDefaultAsync();
        }
    }
}
