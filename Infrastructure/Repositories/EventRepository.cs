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
using GeoCoordinatePortable;

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

        public async Task<IEnumerable<EventHeaderDto>> GetFilteredAsync(EventParams eventParams)
        {
            var query = context.Events
                .Include(e => e.Application.Listing.Venue.User)
                .Include(e => e.EventGenre)
                .AsQueryable();

            if (eventParams.Latitude.HasValue && eventParams.Longitude.HasValue)
            {
                double lat = eventParams.Latitude.Value;
                double lon = eventParams.Longitude.Value;
                int radius = eventParams.RadiusKm ?? 10;

                var (minLat, maxLat, minLon, maxLon) = GeoApproximatorHelper.GetBoundingBox(lat, lon, radius);

                query = query.Where(e =>
                    e.Application.Listing.Venue.User.Latitude >= minLat &&
                    e.Application.Listing.Venue.User.Latitude <= maxLat &&
                    e.Application.Listing.Venue.User.Longitude >= minLon &&
                    e.Application.Listing.Venue.User.Longitude <= maxLon);
            }

            //if (eventParams.GenreIds.Count() > 0)
            //    query = query.Where(e =>
            //        e.EventGenre.Any(eg => eventParams.GenreIds.Contains(eg.GenreId)));


            if (eventParams.OrderByRecent)
                query = query.OrderByDescending(e => e.DatePosted);

            var eventHeaders = await query
            .Select(e => new EventHeaderDto
            {
                Id = e.Id,
                Name = e.Name,
                ImageUrl = e.Application.Artist.ImageUrl,
                County = e.Application.Listing.Venue.User.County,
                Town = e.Application.Listing.Venue.User.Town,
                Latitude = e.Application.Listing.Venue.User.Latitude,
                Longitude = e.Application.Listing.Venue.User.Longitude,
                StartDate = e.Application.Listing.StartDate, 
                EndDate = e.Application.Listing.EndDate 
            })
            .ToListAsync();

            return eventHeaders;
        }

        public async Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id)
        {
            var query = context.Events // Get all events
                .Where(e => e.Application.Listing.VenueId == id // That are associated with the venue
                            && e.Application.Listing.StartDate >= DateTime.Now // And event hasn't passed yet
                            && e.DatePosted != null) // And has been posted
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing); // To retrieve the start date

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingByArtistIdAsync(int id)
        {
            var query = context.Events // Get all events
                .Where(e => e.Application.ArtistId == id // That are associated with the artist
                            && e.Application.Listing.StartDate >= DateTime.Now // And event hasn't passed yet
                            && e.DatePosted != null) // And has been posted
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

        public async Task<IEnumerable<Event>> GetHistoryByArtistIdAsync(int id)
        {
            var query = context.Events // Get all events
            .Where(e => e.Application.ArtistId == id // That are associated with the artist
                        && e.Application.Listing.StartDate < DateTime.Now // That have already started
                        && e.DatePosted != null) // And have been posted
            .Include(e => e.Application)
                .ThenInclude(a => a.Listing); // To retrieve start/end dates

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetHistoryByVenueIdAsync(int id)
        {
            var query = context.Events // Get all events
            .Where(e => e.Application.Listing.VenueId == id // That are associated with the venue
                        && e.Application.Listing.StartDate < DateTime.Now // That have already started
                        && e.DatePosted != null) // And have been posted
            .Include(e => e.Application)
                .ThenInclude(a => a.Listing); // To retrieve start/end dates

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUnpostedByArtistIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.ArtistId == id && e.DatePosted == null)
                .Include(e => e.Application.Listing)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUnpostedByVenueIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.Listing.VenueId == id && e.DatePosted == null)
                .Include(e => e.Application.Listing)
                .ToListAsync();
        }

    }
}
