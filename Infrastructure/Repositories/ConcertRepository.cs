using Core.Entities;
using Application.Interfaces;
using Application.DTOs;
using Application.Mappers;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ConcertRepository : Repository<Concert>, IConcertRepository
    {
        private readonly IGeometryProvider geometryService;
        private readonly TimeProvider timeProvider;

        public ConcertRepository(ApplicationDbContext context, IGeometryProvider geometryService, TimeProvider timeProvider) : base(context)
        {
            this.geometryService = geometryService;
            this.timeProvider = timeProvider;
        }

        public async Task<IEnumerable<ConcertHeaderDto>> GetHeaders(int userId, ConcertParams concertParams)
        {
            var query = context.Concerts
                .Where(e => e.DatePosted != null)
                .Where(e => e.Application.Listing.EndDate > timeProvider.GetUtcNow())
                .AsQueryable();

            if (concertParams.GenreIds.Any())
                query = query.Where(e => e.ConcertGenres.Any(eg => concertParams.GenreIds.Contains(eg.GenreId)));

            if (concertParams.OrderByRecent)
                query = query.OrderByDescending(e => e.DatePosted);
            else
                query = query.OrderBy(e => e.Application.Listing.StartDate);

            if (GeoHelper.HasValidCoordinates(concertParams))
            {
                var center = geometryService.CreatePoint(concertParams.Latitude!.Value, concertParams.Longitude!.Value);
                var radiusKm = concertParams.RadiusKm ?? 10;
                query = query.Where(e =>
                    e.Application.Listing.Venue.User.Location != null &&
                    e.Application.Listing.Venue.User.Location.Distance(center) <= radiusKm * 1000);
            }

            var concerts = await query
                .Take(10)
                .ToListAsync();

            return concerts.ToHeaderDtos();
        }

        public async Task<Concert> GetByIdAsync(int id)
        {
            return await context.Concerts
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
                .Include(e => e.ConcertGenres)
                    .ThenInclude(eg => eg.Genre)
                .FirstAsync();
        }

        public async Task<IEnumerable<Concert>> GetUpcomingByVenueIdAsync(int id)
        {
            return await context.Concerts
                .Where(e => e.Application.Listing.VenueId == id
                            && e.Application.Listing.StartDate >= timeProvider.GetUtcNow()
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Concert>> GetUpcomingByArtistIdAsync(int id)
        {
            return await context.Concerts
                .Where(e => e.Application.ArtistId == id
                            && e.Application.Listing.StartDate >= timeProvider.GetUtcNow()
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<Concert?> GetByApplicationIdAsync(int applicationId)
        {
            return await context.Concerts
                .Where(e => e.ApplicationId == applicationId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Concert>> GetHistoryByArtistIdAsync(int id)
        {
            return await context.Concerts
                .Where(e => e.Application.ArtistId == id
                            && e.Application.Listing.StartDate < timeProvider.GetUtcNow()
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Concert>> GetHistoryByVenueIdAsync(int id)
        {
            return await context.Concerts
                .Where(e => e.Application.Listing.VenueId == id
                            && e.Application.Listing.StartDate < timeProvider.GetUtcNow()
                            && e.DatePosted != null)
                .Include(e => e.Application)
                    .ThenInclude(a => a.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Concert>> GetUnpostedByArtistIdAsync(int id)
        {
            return await context.Concerts
                .Where(e => e.Application.ArtistId == id && e.DatePosted == null)
                .Include(e => e.Application.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Concert>> GetUnpostedByVenueIdAsync(int id)
        {
            return await context.Concerts
                .Where(e => e.Application.Listing.VenueId == id && e.DatePosted == null)
                .Include(e => e.Application.Listing)
                .Include(e => e.Application.Listing.Venue)
                    .ThenInclude(v => v.User)
                .Include(e => e.Application.Artist)
                    .ThenInclude(a => a.User)
                .ToListAsync();
        }

        public async Task<bool> ArtistHasConcertOnDateAsync(int artistId, DateTime date)
        {
            return await context.Concerts
                .Where(e => e.Application.ArtistId == artistId)
                .AnyAsync(e => e.Application.Listing.StartDate.Date == date.Date);
        }

        public Task<bool> ListingHasConcertAsync(int listingId)
        {
            return context.Concerts.AnyAsync(e => e.Application.ListingId == listingId);
        }

        public async Task<bool> VenueHasConcertOnDateAsync(int venueId, DateTime date)
        {
            return await context.Concerts
                .Where(e => e.Application.Listing.VenueId == venueId)
                .AnyAsync(e => e.Application.Listing.StartDate.Date == date.Date);
        }

    }
}
