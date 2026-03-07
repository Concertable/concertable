using Core.Entities;
using Application.Interfaces;
using Application.DTOs;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        private readonly IGeometryProvider geometryService;

        public EventRepository(ApplicationDbContext context, IGeometryProvider geometryService) : base(context)
        {
            this.geometryService = geometryService;
        }

        public async Task<IEnumerable<EventHeaderDto>> GetHeaders(int userId, EventParams eventParams)
        {
            var query = context.Events
                .Where(e => e.DatePosted != null)
                .Where(e => e.Application.Listing.EndDate > DateTime.UtcNow)
                .AsQueryable();

            if (eventParams.GenreIds.Any())
                query = query.Where(e => e.EventGenres.Any(eg => eventParams.GenreIds.Contains(eg.GenreId)));

            if (eventParams.OrderByRecent)
                query = query.OrderByDescending(e => e.DatePosted);
            else
                query = query.OrderBy(e => e.Application.Listing.StartDate);

            if (GeoHelper.HasValidCoordinates(eventParams))
            {
                var center = geometryService.CreatePoint(eventParams.Latitude!.Value, eventParams.Longitude!.Value);
                var radiusKm = eventParams.RadiusKm ?? 10;
                query = query.Where(e =>
                    e.Application.Listing.Venue.User.Location != null &&
                    e.Application.Listing.Venue.User.Location.Distance(center) <= radiusKm * 1000);
            }

            return await query
                .Select(e => new EventHeaderDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ImageUrl = e.Application.Artist.ImageUrl,
                    StartDate = e.Application.Listing.StartDate,
                    EndDate = e.Application.Listing.EndDate,
                    County = e.Application.Listing.Venue.User.County,
                    Town = e.Application.Listing.Venue.User.Town,
                    Latitude = e.Application.Listing.Venue.User.Location.Y,
                    Longitude = e.Application.Listing.Venue.User.Location.X,
                    DatePosted = e.DatePosted
                })
                .Take(10)
                .ToListAsync();
        }

        public async Task<Event> GetByIdAsync(int id)
        {
            return await context.Events
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
                        .ThenInclude(v => v.User)
                .Include(e => e.EventGenres)
                    .ThenInclude(eg => eg.Genre)
                .FirstAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.Listing.VenueId == id
                            && e.Application.Listing.StartDate >= DateTime.UtcNow
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingByArtistIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.ArtistId == id
                            && e.Application.Listing.StartDate >= DateTime.UtcNow
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<Event?> GetByApplicationIdAsync(int applicationId)
        {
            return await context.Events
                .Where(e => e.ApplicationId == applicationId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Event>> GetHistoryByArtistIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.ArtistId == id
                            && e.Application.Listing.StartDate < DateTime.UtcNow
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetHistoryByVenueIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.Listing.VenueId == id
                            && e.Application.Listing.StartDate < DateTime.UtcNow
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUnpostedByArtistIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.ArtistId == id && e.DatePosted == null)
                .Include(e => e.Application.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUnpostedByVenueIdAsync(int id)
        {
            return await context.Events
                .Where(e => e.Application.Listing.VenueId == id && e.DatePosted == null)
                .Include(e => e.Application.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<bool> ArtistHasEventOnDateAsync(int artistId, DateTime date)
        {
            return await context.Events
                .Where(e => e.Application.ArtistId == artistId)
                .AnyAsync(e => e.Application.Listing.StartDate.Date == date.Date);
        }

        public Task<bool> ListingHasEventAsync(int listingId)
        {
            return context.Events.AnyAsync(e => e.Application.ListingId == listingId);
        }

        public async Task<bool> VenueHasEventOnDateAsync(int venueId, DateTime date)
        {
            return await context.Events
                .Where(e => e.Application.Listing.VenueId == venueId)
                .AnyAsync(e => e.Application.Listing.StartDate.Date == date.Date);
        }
    }
}
