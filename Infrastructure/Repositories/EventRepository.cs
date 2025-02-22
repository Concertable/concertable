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
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IHeaderRepositoryFactory headerRepositoryFactory;

        public EventRepository(
            ApplicationDbContext context, 
            IReviewRepository reviewRepository,
            IHeaderRepositoryFactory headerRepositoryFactory
            ) : base(context) 
        {
            this.reviewRepository = reviewRepository;
            this.headerRepositoryFactory = headerRepositoryFactory;
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


        public async Task<PaginationResponse<EventHeaderDto>> GetRawHeadersAsync(SearchParams searchParams)
        {
            Expression<Func<Event, EventHeaderDto>> selector = e => new EventHeaderDto
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

            var filters = new List<Expression<Func<Event, bool>>>(); 

            if (searchParams.Date != null)
            {
                filters.Add(e => e.Application.Listing.StartDate >= searchParams.Date);
            }

            var headerRepository = headerRepositoryFactory.Create(selector, filters);

            // Delegate the call to header repository
            return await headerRepository.GetRawHeadersAsync(searchParams);
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
